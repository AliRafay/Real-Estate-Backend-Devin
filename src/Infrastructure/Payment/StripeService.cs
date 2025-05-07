using Demo.WebApi.Application.Common.Exceptions;
using Demo.WebApi.Application.Common.Interfaces;
using Demo.WebApi.Application.Payment;
using Demo.WebApi.Domain.Common.Enums;
using Demo.WebApi.Infrastructure.Common.Extensions;
using Demo.WebApi.Infrastructure.Common.External;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using Demo.WebApi.Shared.Localization;
using Account = Stripe.Account;
using AccountService = Stripe.AccountService;
using CustomerService = Stripe.CustomerService;

namespace Demo.WebApi.Infrastructure.Payment;
public class StripeService : IStripeService
{
    private readonly StripeSettings _stripeSettings;
    private readonly ICurrentUser _currentUser;
    private readonly IStringLocalizer<StripeService> _localizer;
    private readonly ILogger<StripeService> _logger;
    private readonly LinkSettings _linkSettings;

    public StripeService(
        IOptions<StripeSettings> stripeSettings,
        ICurrentUser currentUser,
        IStringLocalizer<StripeService> localizer,
        ILogger<StripeService> logger,
        IOptions<LinkSettings> linkSettings)
    {
        _stripeSettings = stripeSettings.Value;
        _currentUser = currentUser;
        StripeConfiguration.ApiKey = _stripeSettings.ApiKey;
        _localizer = localizer;
        _logger = logger;
        _linkSettings = linkSettings.Value;
    }

    public async Task<string> AddStripeCustomerAsync(AddStripeCustomerRequest request, CancellationToken cancellationToken)
    {
        CustomerCreateOptions customerOptions = new CustomerCreateOptions
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Metadata = new Dictionary<string, string> { { "role", request.Role } }
        };

        return (await new CustomerService().CreateAsync(customerOptions, null, cancellationToken)).Id;
    }

    public async Task<SetupIntentResponse> SetupIntentForFuturePaymentAsync(string stripeId, string role, CancellationToken cancellationToken, Dictionary<string, string>? metaData = null)
    {
        Dictionary<string, string> metaDataDict = new Dictionary<string, string> { { "role", role } };

        if (metaData != null)
        {
            foreach (var obj in metaData)
            {
                metaDataDict.Add(obj.Key, obj.Value);
            }
        }

        var options = new SetupIntentCreateOptions
        {
            Metadata = metaDataDict,
            Customer = stripeId

        };

        try
        {
            var service = new SetupIntentService();
            SetupIntent setupIntent = await service.CreateAsync(options);
            return new SetupIntentResponse { ClientSecret = setupIntent.ClientSecret };
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"STRIPE: Setup Intent Creation for {role} {stripeId} Failed at {DateTime.UtcNow}");
            throw new NotFoundException(e.Message);
        }
    }

    public async Task<PaymentIntentResponse> CreatePaymentIntentAsync(decimal amount, Currency currency, string? stripeCustomerId, string? paymentToken, bool authOnly, Dictionary<string, string>? metaData, decimal retryAmount = 0M)
    {
        Dictionary<string, string> metaDataDict = new Dictionary<string, string>();

        if (metaData != null)
        {
            foreach (var obj in metaData)
            {
                metaDataDict.Add(obj.Key, obj.Value);
            }
        }

        var options = new PaymentIntentCreateOptions
        {
            Amount = amount.ToLowestCurrencyUnit(currency),
            Currency = currency.ToString(),
            PaymentMethodTypes = new List<string> { "card" },
            Customer = stripeCustomerId,
            PaymentMethod = paymentToken,
            Confirm = paymentToken != null,
            UseStripeSdk = true,
            Metadata = metaDataDict,
        };

        if (authOnly)
            options.CaptureMethod = "manual";

        try
        {
            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            return new PaymentIntentResponse { PaymentIntentId = intent.Id, Status = intent.Status, ClientSecret = intent.ClientSecret, MetaData = intent.Metadata, LatestChargeId = intent.LatestChargeId, AmountCharged = amount };
        }
        catch (Exception e)
        {
            if (retryAmount == 0)
            {
                _logger.LogError(e, $"STRIPE: Authorization Failed at {DateTime.UtcNow}, StripeCustomerId = {stripeCustomerId}, PaymentToken = {paymentToken}, Amount = {amount}, Currency = {currency}");
                throw new NotFoundException(e.Message);
            }

            return await CreatePaymentIntentAsync(amount, currency, stripeCustomerId, paymentToken, authOnly, metaData);
        }
    }

    public async Task<PaymentIntent> CapturePaymentAsync(string intentId, decimal? amount = null, Currency? currency = null)
    {
        try
        {
            var service = new PaymentIntentService();
            PaymentIntentCaptureOptions options = new PaymentIntentCaptureOptions();
            if (amount is not null)
                options.AmountToCapture = amount!.Value.ToLowestCurrencyUnit(currency!.Value);

            PaymentIntent intent = await service.CaptureAsync(intentId, options);
            return intent;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"STRIPE: Payment Capture Failed at {DateTime.UtcNow}, IntentionId = {intentId}");
            throw new NotFoundException(e.Message);
        }
    }

    public async Task<PaymentIntent> CancelPaymentAsync(string intentId)
    {
        try
        {
            var service = new PaymentIntentService();
            PaymentIntent intent = await service.GetAsync(intentId);
            if (intent.Status != "canceled")
                intent = await service.CancelAsync(intentId);
            return intent;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"STRIPE: Payment Cancel Failed at {DateTime.UtcNow}, IntentionId = {intentId}");
            throw new NotFoundException(e.Message);
        }
    }

    public async Task<PaymentIntent> UpdatePaymentIntentAsync(string intentId, Dictionary<string, string>? metaData = null)
    {
        try
        {
            var service = new PaymentIntentService();
            var currentIntent = await service.GetAsync(intentId) ??
                throw new NotFoundException(string.Format(_localizer[MessageConstants.RecordNotFound], _localizer[EntityConstants.Payment]));

            var options = new PaymentIntentUpdateOptions();

            if (metaData is not null)
                options.Metadata = metaData;

            PaymentIntent intent = await service.UpdateAsync(intentId, options);

            return intent;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"STRIPE: Payment Update Failed at {DateTime.UtcNow}, IntentionId = {intentId}");
            throw new NotFoundException(e.Message);
        }
    }

    public async Task UpdateCustomerBalanceTransAsync(string customerId, string transactionId, Dictionary<string, string>? metaData = null)
    {
        try
        {
            var options = new CustomerBalanceTransactionUpdateOptions
            {
                Metadata = metaData,
            };
            var service = new CustomerBalanceTransactionService();
            await service.UpdateAsync(customerId, transactionId, options);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"STRIPE: Payment Update Failed at {DateTime.UtcNow}, transactionId = {transactionId}");
            throw new NotFoundException(e.Message);
        }
    }

    public async Task<Charge> UpdateChargeMetadataAsync(string paymentId, Dictionary<string, string>? metaData = null)
    {
        try
        {
            var service = new ChargeService();
            var currentIntent = await service.GetAsync(paymentId) ??
                throw new NotFoundException(string.Format(_localizer[MessageConstants.RecordNotFound], _localizer[EntityConstants.Payment]));

            var options = new ChargeUpdateOptions();
            if (metaData is not null)
                options.Metadata = metaData;
            Charge charge = await service.UpdateAsync(paymentId, options);

            return charge;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"STRIPE: Payment Update Failed at {DateTime.UtcNow}, chargeId = {paymentId}");
            throw new NotFoundException(e.Message);
        }
    }

    public async Task<string> CreateTransfer(decimal amount, Currency currency, string destination, Dictionary<string, string>? metaData = null, string? checkAvailabiltyChargeId = null)
    {
        try
        {
            Dictionary<string, string> metaDataDict = new Dictionary<string, string> { { "account", destination } };

            if (metaData != null)
            {
                foreach (var obj in metaData)
                {
                    metaDataDict.Add(obj.Key, obj.Value);
                }
            }

            var options = new TransferCreateOptions
            {
                Amount = amount.ToLowestCurrencyUnit(currency),
                Currency = currency.ToString(),
                Destination = destination,
                Metadata = metaDataDict
            };

            if (!checkAvailabiltyChargeId.IsNullOrEmpty())
                options.SourceTransaction = checkAvailabiltyChargeId;
            var service = new TransferService();
            var transfer = await service.CreateAsync(options);

            return _localizer[MessageConstants.TransferSuccessful];
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"STRIPE: CreateTransfer Failed at {DateTime.UtcNow} for Destination = {destination}");
            throw new NotFoundException(e.Message);
        }
    }

    public async Task<string> CreateTransferToPlatform(decimal amount, Currency currency, string source, Dictionary<string, string> metaData)
    {
        try
        {
            metaData.Add("account", source);

            var options = new TransferCreateOptions
            {
                Amount = amount.ToLowestCurrencyUnit(currency),
                Currency = currency.ToString(),
                Destination = _stripeSettings.PlatformAccountId,
                Metadata = metaData
            };

            var requestOptions = new RequestOptions();
            requestOptions.StripeAccount = source;

            var service = new TransferService();
            var transfer = await service.CreateAsync(options, requestOptions);
            if (transfer.DestinationPaymentId != null)
                await UpdateChargeMetadataAsync(transfer.DestinationPaymentId, metaData);
            return _localizer[MessageConstants.TransferSuccessful];
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"STRIPE: CreateTransferToPlatfrom Failed at {DateTime.UtcNow} from source = {source}");
            throw new NotFoundException(e.Message);
        }
    }

    public async Task<StripeList<CustomerBalanceTransaction>> GetTransactions(string stripeCustomerId, string startingAfter)
    {
        var options = new CustomerBalanceTransactionListOptions { Limit = 100, EndingBefore = startingAfter };
        var service = new CustomerBalanceTransactionService();
        StripeList<CustomerBalanceTransaction> customerBalanceTransactions = await service.ListAsync(
            stripeCustomerId,
            options);
        return customerBalanceTransactions;
    }

    public async Task<List<BalanceAmount>> GetConnectedAccountBalanceAsync(string? accountId)
    {
        var requestOptions = new RequestOptions();
        requestOptions.StripeAccount = accountId;

        var service = new BalanceService();
        return (await service.GetAsync(requestOptions)).Available;
    }

    public async Task<Account> CreateConnectedAccountAsync(CreateConnectedAccountRequest request, Dictionary<string, string> metaData)
    {
        try
        {
            string stripeDocumentFront = request.DocumentFront != null ? await UploadImageToStripe(request.DocumentFront!) : string.Empty;
            string stripeDocumentBack = request.DocumentBack != null ? await UploadImageToStripe(request.DocumentBack!) : string.Empty;

            var options = new AccountCreateOptions
            {
                Type = "express",
                Email = request.Email,
                Capabilities = new AccountCapabilitiesOptions
                {
                    Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true }
                },
                BusinessType = "individual",
                BusinessProfile = new AccountBusinessProfileOptions
                {
                    ProductDescription = "Y-Drive"
                },
                ExternalAccount = new AccountBankAccountOptions
                {
                    AccountHolderName = request.AccountHolderName,
                    AccountHolderType = "individual",
                    AccountNumber = request.AccountNumber,
                    RoutingNumber = request.RoutingNumber,
                    Country = request.Country,
                    Currency = request.Currency,
                },
                Individual = new AccountIndividualOptions
                {
                    Dob = request.Dob == null ? null : new DobOptions
                    {
                        Day = request.Dob.GetValueOrDefault().Day,
                        Month = request.Dob.GetValueOrDefault().Month,
                        Year = request.Dob.GetValueOrDefault().Year,
                    },
                    Address = new AddressOptions
                    {
                        Line1 = request.Address,
                        PostalCode = request.PostalCode,
                        City = request.City,
                        State = request.State
                    },
                    Verification = new AccountIndividualVerificationOptions
                    {
                        Document = new AccountIndividualVerificationDocumentOptions
                        {
                            Back = stripeDocumentFront,
                            Front = stripeDocumentBack
                        }
                    },

                    IdNumber = request.IdNumber
                },
                Metadata = metaData
            };

            var service = new AccountService();
            var response = await service.CreateAsync(options);
            return response;
        }
        catch (NotFoundException e)
        {
            _logger.LogError(e, $"CreateConnectedAccountAsync failed at {DateTime.UtcNow}");
            throw new NotFoundException(e.Message);
        }
    }

    public async Task<Account> UpdateConnectedAccountAsync(string id, CreateConnectedAccountRequest request, Dictionary<string, string> metaData)
    {
        try
        {
            var options = new AccountUpdateOptions
            {
                Email = request.Email
            };

            var service = new AccountService();
            var response = await service.UpdateAsync(id, options);
            return response;
        }
        catch (NotFoundException e)
        {
            _logger.LogError(e, $"UpdateConnectedAccountAsync failed at {DateTime.UtcNow}");
            throw new NotFoundException(e.Message);
        }
    }

    public async Task<Account> GetConnectedAccountInfo(string AccountId)
    {
        var stripeAccountService = new AccountService();
        var stripeAccountInfo = await stripeAccountService.GetAsync(AccountId);

        return stripeAccountInfo;
    }

    public async Task<string?> RefreshConnectedAccountLinkAsync(string accountId)
    {
        return (await CreateConnectedAccountLinkAsync(accountId)).Url;
    }

    public async Task<string> GetDashboardLinkAsync(string accountId, CancellationToken cancellationToken)
    {
        try
        {
            var service = new LoginLinkService();
            return (await service.CreateAsync(accountId)).Url;
        }
        catch (NotFoundException e)
        {
            _logger.LogError(e, $"GetDashboardLinkAsync failed at {DateTime.UtcNow}");
            throw new NotFoundException(e.Message);
        }
    }

    public async Task<CreateConnectedAccountResponse> CreateConnectedAccountLinkAsync(string accountId)
    {
        var options = new AccountLinkCreateOptions
        {
            Account = accountId,
            RefreshUrl = $"{_linkSettings.StripeRedirectPath}?account={accountId}",
            ReturnUrl = _linkSettings.StripeReturnPath,
            Type = "account_onboarding",
            Collect = "eventually_due",
        };

        var service = new AccountLinkService();
        CreateConnectedAccountResponse response = new CreateConnectedAccountResponse();
        return (await service.CreateAsync(options)).Adapt(response);
    }

    private async Task<string> UploadImageToStripe(IFormFile file)
    {
        using (var stream = file.OpenReadStream())
        {
            var fileService = new FileService();
            var options = new FileCreateOptions
            {
                File = stream,
                Purpose = FilePurpose.IdentityDocument,
            };

            var stripeFile = await fileService.CreateAsync(options);

            return stripeFile.Id;
        }
    }
}

using Demo.WebApi.Domain.Common.Enums;
using Stripe;

namespace Demo.WebApi.Application.Payment;
public interface IStripeService : IScopedService
{
    Task<string> AddStripeCustomerAsync(AddStripeCustomerRequest request, CancellationToken cancellationToken);

    Task<SetupIntentResponse> SetupIntentForFuturePaymentAsync(string stripeId, string role, CancellationToken cancellationToken, Dictionary<string, string>? metaData = null);

    Task<PaymentIntentResponse> CreatePaymentIntentAsync(decimal amount, Currency currency, string? stripeCustomerId, string paymentToken, bool authOnly, Dictionary<string, string>? metaData, decimal retryAmount = 0M);

    Task<PaymentIntent> CapturePaymentAsync(string intentId, decimal? amount = null, Currency? currency = null);

    Task<PaymentIntent> CancelPaymentAsync(string intentId);
    Task<PaymentIntent> UpdatePaymentIntentAsync(string intentId, Dictionary<string, string>? metaData = null);

    Task<List<BalanceAmount>> GetConnectedAccountBalanceAsync(string? AccountId);

    Task<Account> GetConnectedAccountInfo(string AccountId);

    Task<string?> RefreshConnectedAccountLinkAsync(string accountId);

    Task<string> GetDashboardLinkAsync(string accountId, CancellationToken cancellationToken);

    Task<string> CreateTransfer(decimal amount, Currency currency, string destination, Dictionary<string, string>? metaData = null, string? checkAvailabiltyChargeId = null);

    Task<string> CreateTransferToPlatform(decimal amount, Currency currency, string source, Dictionary<string, string> metaData);

    Task<Charge> UpdateChargeMetadataAsync(string paymentId, Dictionary<string, string>? metaData = null);

    Task UpdateCustomerBalanceTransAsync(string customerId, string transactionId, Dictionary<string, string>? metaData = null);

    Task<Account> CreateConnectedAccountAsync(CreateConnectedAccountRequest request, Dictionary<string, string> metaData);

    Task<CreateConnectedAccountResponse> CreateConnectedAccountLinkAsync(string accountId);

    Task<Account> UpdateConnectedAccountAsync(string id, CreateConnectedAccountRequest request, Dictionary<string, string> metaData);

    Task<StripeList<CustomerBalanceTransaction>> GetTransactions(string stripeCustomerId, string startingAfter);
}

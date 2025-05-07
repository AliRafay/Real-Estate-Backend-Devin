using Demo.WebApi.Application.Buyers;
using Demo.WebApi.Application.Common.Models;
using Demo.WebApi.Infrastructure.Common.Extensions;
using Demo.WebApi.Shared.Authorization;

namespace Demo.WebApi.Host.Controllers.Buyers;

public class BuyersController : VersionNeutralApiController
{
    private readonly IBuyerService _buyerService;

    public BuyersController(IBuyerService buyerService)
    {
        _buyerService = buyerService;
    }

    [HttpGet]
    [MustHavePermission(AppAction.View, AppResource.Buyers)]
    [OpenApiOperation("Get list of all buyers with pagination.", "")]
    public async Task<HttpResponseDto<PaginationResponse<BuyerDto>>> SearchAsync([FromQuery] BuyerListFilter filter, CancellationToken cancellationToken)
    {
        return (await _buyerService.SearchAsync(filter, cancellationToken)).ToInformationResponse();
    }

    [HttpGet("{id:int}")]
    [MustHavePermission(AppAction.View, AppResource.Buyers)]
    [OpenApiOperation("Get a buyer's details.", "")]
    public async Task<HttpResponseDto<BuyerDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return (await _buyerService.GetAsync(id, cancellationToken)).ToInformationResponse();
    }

    [HttpPost]
    [MustHavePermission(AppAction.Create, AppResource.Buyers)]
    [OpenApiOperation("Creates a new buyer.", "")]
    public async Task<HttpResponseDto<string>> CreateAsync(CreateBuyerRequest request, CancellationToken cancellationToken)
    {
        var id = await _buyerService.CreateAsync(request, cancellationToken);
        return HttpResponseExtension.InformationResponse($"Buyer created successfully with ID: {id}");
    }

    [HttpPut]
    [MustHavePermission(AppAction.Update, AppResource.Buyers)]
    [OpenApiOperation("Updates a buyer.", "")]
    public async Task<HttpResponseDto<string>> UpdateAsync(UpdateBuyerRequest request, CancellationToken cancellationToken)
    {
        var id = await _buyerService.UpdateAsync(request, cancellationToken);
        return HttpResponseExtension.InformationResponse($"Buyer updated successfully with ID: {id}");
    }

    [HttpDelete("{id:int}")]
    [MustHavePermission(AppAction.Delete, AppResource.Buyers)]
    [OpenApiOperation("Delete a buyer.", "")]
    public async Task<HttpResponseDto<string>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _buyerService.DeleteAsync(id, cancellationToken);
        return HttpResponseExtension.InformationResponse("Buyer Deleted Successfully.");
    }

    [HttpPost("toggle-block-status")]
    [MustHavePermission(AppAction.Update, AppResource.Buyers)]
    [OpenApiOperation("Block or unblock a buyer.", "")]
    public async Task<HttpResponseDto<string>> ToggleBlockStatusAsync(ToggleBuyerStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _buyerService.ToggleBlockStatusAsync(request, cancellationToken);
        return HttpResponseExtension.InformationResponse(result);
    }
}

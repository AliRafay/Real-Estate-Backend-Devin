using Demo.WebApi.Application.Common.Models;

namespace Demo.WebApi.Application.Buyers;

public interface IBuyerService : ITransientService
{
    Task<BuyerDto> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<PaginationResponse<BuyerDto>> SearchAsync(BuyerListFilter filter, CancellationToken cancellationToken);
    Task<Guid> CreateAsync(CreateBuyerRequest request, CancellationToken cancellationToken);
    Task<Guid> UpdateAsync(UpdateBuyerRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<string> ToggleBlockStatusAsync(ToggleBuyerStatusRequest request, CancellationToken cancellationToken);
}

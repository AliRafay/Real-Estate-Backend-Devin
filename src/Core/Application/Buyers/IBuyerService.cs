using Demo.WebApi.Application.Common.Models;

namespace Demo.WebApi.Application.Buyers;

public interface IBuyerService : ITransientService
{
    Task<BuyerDto> GetAsync(int id, CancellationToken cancellationToken);
    Task<PaginationResponse<BuyerDto>> SearchAsync(BuyerListFilter filter, CancellationToken cancellationToken);
    Task<int> CreateAsync(CreateBuyerRequest request, CancellationToken cancellationToken);
    Task<int> UpdateAsync(UpdateBuyerRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task<string> ToggleBlockStatusAsync(ToggleBuyerStatusRequest request, CancellationToken cancellationToken);
}

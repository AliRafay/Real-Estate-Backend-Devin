using Ardalis.Specification;
using Demo.WebApi.Application.Buyers;
using Demo.WebApi.Application.Common.Exceptions;
using Demo.WebApi.Application.Common.Models;
using Demo.WebApi.Application.Common.Persistence;
using Demo.WebApi.Domain.Common.Enums;
using Demo.WebApi.Domain.Public;
using Mapster;
using Microsoft.Extensions.Localization;

namespace Demo.WebApi.Infrastructure.Public;

public class BuyerService : IBuyerService
{
    private readonly IRepository<Buyer> _repository;
    private readonly IStringLocalizer _localizer;

    public BuyerService(IRepository<Buyer> repository, IStringLocalizer<BuyerService> localizer)
    {
        _repository = repository;
        _localizer = localizer;
    }

    public async Task<BuyerDto> GetAsync(int id, CancellationToken cancellationToken)
    {
        var buyer = await _repository.GetByIdAsync(id, cancellationToken);

        if (buyer is null)
        {
            throw new NotFoundException(_localizer["Buyer Not Found."]);
        }

        return buyer.Adapt<BuyerDto>();
    }

    public async Task<PaginationResponse<BuyerDto>> SearchAsync(BuyerListFilter filter, CancellationToken cancellationToken)
    {
        var spec = new BuyersBySearchFilterSpec(filter);
        
        var buyers = await _repository.ListAsync(spec, cancellationToken);
        var buyerDtos = buyers.Adapt<List<BuyerDto>>();
            
        var count = await _repository.CountAsync(new BuyersBySearchFilterSpec(filter, true), cancellationToken);
            
        return new PaginationResponse<BuyerDto>(buyerDtos, count, filter.PageNumber, filter.PageSize);
    }

    public async Task<int> CreateAsync(CreateBuyerRequest request, CancellationToken cancellationToken)
    {
        var buyer = request.Adapt<Buyer>();
        
        buyer.Status = BuyerStatus.Active;
        buyer.RegistrationDate = DateTime.UtcNow;
        
        var existingBuyer = await _repository.FirstOrDefaultAsync(
            new BuyerByEmailSpec(request.Email), 
            cancellationToken);
            
        if (existingBuyer != null)
        {
            throw new ConflictException(_localizer["Buyer with this email already exists."]);
        }
        
        await _repository.AddAsync(buyer, cancellationToken);
        
        return buyer.Id;
    }

    public async Task<int> UpdateAsync(UpdateBuyerRequest request, CancellationToken cancellationToken)
    {
        var buyer = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (buyer is null)
        {
            throw new NotFoundException(_localizer["Buyer Not Found."]);
        }
        
        var existingBuyer = await _repository.FirstOrDefaultAsync(
            new BuyerByEmailSpec(request.Email), 
            cancellationToken);
            
        if (existingBuyer != null && !existingBuyer.Id.Equals(request.Id))
        {
            throw new ConflictException(_localizer["Buyer with this email already exists."]);
        }
        
        request.Adapt(buyer);
        
        await _repository.UpdateAsync(buyer, cancellationToken);
        
        return buyer.Id;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var buyer = await _repository.GetByIdAsync(id, cancellationToken);
        
        if (buyer is null)
        {
            throw new NotFoundException(_localizer["Buyer Not Found."]);
        }
        
        await _repository.DeleteAsync(buyer, cancellationToken);
    }

    public async Task<string> ToggleBlockStatusAsync(ToggleBuyerStatusRequest request, CancellationToken cancellationToken)
    {
        var buyer = await _repository.GetByIdAsync(request.BuyerId, cancellationToken);
        
        if (buyer is null)
        {
            throw new NotFoundException(_localizer["Buyer Not Found."]);
        }
        
        if (request.Block)
        {
            buyer.Status = BuyerStatus.Blocked;
            await _repository.UpdateAsync(buyer, cancellationToken);
            return _localizer["Buyer Blocked Successfully."];
        }
        else
        {
            buyer.Status = BuyerStatus.Active;
            await _repository.UpdateAsync(buyer, cancellationToken);
            return _localizer["Buyer Unblocked Successfully."];
        }
    }
}

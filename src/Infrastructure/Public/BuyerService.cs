using Demo.WebApi.Application.Buyers;
using Demo.WebApi.Application.Common.Exceptions;
using Demo.WebApi.Application.Common.Models;
using Demo.WebApi.Application.Common.Specification;
using Demo.WebApi.Domain.Common.Enums;
using Demo.WebApi.Domain.Public;
using Demo.WebApi.Infrastructure.Common.Extensions;
using Demo.WebApi.Infrastructure.Persistence.Context;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Demo.WebApi.Infrastructure.Public;

public class BuyerService : IBuyerService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IStringLocalizer _localizer;

    public BuyerService(ApplicationDbContext dbContext, IStringLocalizer<BuyerService> localizer)
    {
        _dbContext = dbContext;
        _localizer = localizer;
    }

    public async Task<BuyerDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var buyer = await _dbContext.Buyers
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (buyer is null)
        {
            throw new NotFoundException(_localizer["Buyer Not Found."]);
        }

        return buyer.Adapt<BuyerDto>();
    }

    public async Task<PaginationResponse<BuyerDto>> SearchAsync(BuyerListFilter filter, CancellationToken cancellationToken)
    {
        var spec = new BuyersBySearchFilterSpec(filter);
        
        var buyers = await _dbContext.Buyers
            .WithSpecification(spec)
            .AsNoTracking()
            .ProjectToType<BuyerDto>()
            .ToListAsync(cancellationToken);
            
        var count = await _dbContext.Buyers
            .WithSpecification(new BuyersBySearchFilterSpec(filter, true))
            .CountAsync(cancellationToken);
            
        return new PaginationResponse<BuyerDto>(buyers, count, filter.PageNumber, filter.PageSize);
    }

    public async Task<Guid> CreateAsync(CreateBuyerRequest request, CancellationToken cancellationToken)
    {
        var buyer = request.Adapt<Buyer>();
        
        buyer.Status = BuyerStatus.Active;
        buyer.RegistrationDate = DateTime.UtcNow;
        
        if (await _dbContext.Buyers.AnyAsync(b => b.Email == request.Email, cancellationToken))
        {
            throw new ConflictException(_localizer["Buyer with this email already exists."]);
        }
        
        await _dbContext.Buyers.AddAsync(buyer, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return buyer.Id;
    }

    public async Task<Guid> UpdateAsync(UpdateBuyerRequest request, CancellationToken cancellationToken)
    {
        var buyer = await _dbContext.Buyers.FindAsync(new object[] { request.Id }, cancellationToken);
        
        if (buyer is null)
        {
            throw new NotFoundException(_localizer["Buyer Not Found."]);
        }
        
        if (await _dbContext.Buyers.AnyAsync(b => b.Email == request.Email && b.Id != request.Id, cancellationToken))
        {
            throw new ConflictException(_localizer["Buyer with this email already exists."]);
        }
        
        request.Adapt(buyer);
        
        _dbContext.Buyers.Update(buyer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return buyer.Id;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var buyer = await _dbContext.Buyers.FindAsync(new object[] { id }, cancellationToken);
        
        if (buyer is null)
        {
            throw new NotFoundException(_localizer["Buyer Not Found."]);
        }
        
        _dbContext.Buyers.Remove(buyer);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> ToggleBlockStatusAsync(ToggleBuyerStatusRequest request, CancellationToken cancellationToken)
    {
        var buyer = await _dbContext.Buyers.FindAsync(new object[] { request.BuyerId }, cancellationToken);
        
        if (buyer is null)
        {
            throw new NotFoundException(_localizer["Buyer Not Found."]);
        }
        
        if (request.Block)
        {
            buyer.Status = BuyerStatus.Blocked;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return _localizer["Buyer Blocked Successfully."];
        }
        else
        {
            buyer.Status = BuyerStatus.Active;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return _localizer["Buyer Unblocked Successfully."];
        }
    }
}

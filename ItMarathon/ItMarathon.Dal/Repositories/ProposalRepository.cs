using ItMarathon.Dal.Common.Contracts;
using ItMarathon.Dal.Context;
using ItMarathon.Dal.Entities;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace ItMarathon.Dal.Repositories;

public class ProposalRepository(ApplicationDbContext repositoryContext) :
    RepositoryBase<Proposal>(repositoryContext), IProposalRepository
{
    public async Task<IEnumerable<Proposal>> GetProposalsAsync(bool trackChanges,  ODataQueryOptions queryOptions)
    {
        IQueryable<Proposal> query = FindAll(trackChanges);
        
       
         // Apply filtering
    if (queryOptions.Filter != null)
    {
        var settings = new ODataQuerySettings();

        var filteredQuery = queryOptions.Filter.ApplyTo(query, settings);

        query = filteredQuery as IQueryable<Proposal> ?? query;  
    }

     if (queryOptions.OrderBy != null)
    {
        query = (IQueryable<Proposal>)queryOptions.OrderBy.ApplyTo(query);
    }
    else
    {
        // Default ordering, e.g., by Proposal Id ascending
        query = query.OrderBy(p => p.Id);
    }
    
    if (queryOptions.Skip != null)
    {
        query = query.Skip(queryOptions.Skip.Value);
    }

    if (queryOptions.Top != null)
    {
        query = query.Take(queryOptions.Top.Value);
    }

        query = query
            .Include(p => p.AppUser)
            .Include(p => p.Photos)
            .Include(p => p.Properties!)
                .ThenInclude(properties => properties.PropertyDefinition)
            .Include(p => p.Properties!)
                .ThenInclude(properties => properties.PredefinedValue)
                    .ThenInclude(prop => prop!.ParentPropertyValue);

        return await query.ToListAsync();
    }

    public async Task<Proposal?> GetProposalAsync(long proposalId, bool trackChanges)
        => await FindByCondition(c => c.Id.Equals(proposalId), trackChanges)
        .Include(p => p.AppUser)
        .Include(p => p.Photos)
        .Include(p => p.Properties!)
            .ThenInclude(properties => properties.PropertyDefinition)
        .Include(p => p.Properties!)
            .ThenInclude(properties => properties.PredefinedValue)
                .ThenInclude(prop => prop!.ParentPropertyValue)
        .SingleOrDefaultAsync();

   public async Task<int> GetProposalsCountAsync(ODataQueryOptions<Proposal> queryOptions)
{
    IQueryable<Proposal> query = FindAll(false); 

    if (queryOptions.Filter != null)
    {
        var settings = new ODataQuerySettings();

        var filteredQuery = queryOptions.Filter.ApplyTo(query, settings);

        query = filteredQuery as IQueryable<Proposal> ?? query;  
    }

    return await query.CountAsync();
}


    public void CreateProposal(Proposal proposal) => Create(proposal);

    public void DeleteProposal(Proposal proposal) => Delete(proposal);
}

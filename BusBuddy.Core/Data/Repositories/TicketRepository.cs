using BusBuddy.Core.Data.Interfaces;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;

namespace BusBuddy.Core.Data.Repositories
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(BusBuddyDbContext context, IUserContextService userContextService)
            : base(context, userContextService)
        {
        }
        // Add Ticket-specific methods here if needed
    }

    public interface ITicketRepository : IRepository<Ticket>
    {
        // Add Ticket-specific methods here if needed
    }
}

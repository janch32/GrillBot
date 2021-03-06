using System.Linq;
using System.Threading.Tasks;
using Grillbot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Database.Repository
{
    public class TeamSearchRepository : RepositoryBase
    {
        public TeamSearchRepository(GrillBotContext context) : base(context)
        {
        }

        public IQueryable<TeamSearch> GetAllSearches()
        {
            return Context.TeamSearch.AsQueryable();
        }

        public async Task AddSearchAsync(ulong userID, ulong channelID, ulong messageID)
        {
            var entity = new TeamSearch()
            {
                UserId = userID.ToString(),
                MessageId = messageID.ToString(),
                ChannelId = channelID.ToString()
            };

            await Context.TeamSearch.AddAsync(entity).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task RemoveSearchAsync(int id)
        {
            var row = await FindSearchByID(id).ConfigureAwait(false);

            if (row == null)
                return;

            Context.TeamSearch.Remove(row);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<TeamSearch> FindSearchByID(int id)
        {
            return await Context.TeamSearch.FirstOrDefaultAsync(o => o.Id == id).ConfigureAwait(false);
        }
    }
}
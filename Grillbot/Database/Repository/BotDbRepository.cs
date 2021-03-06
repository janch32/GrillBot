﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Database.Repository
{
    public class BotDbRepository : RepositoryBase
    {
        public BotDbRepository(GrillBotContext context) : base(context)
        {
        }

        public async Task<Dictionary<string, int>> GetTableRowsCount()
        {
            var autoReplyCount = await Context.AutoReply.CountAsync().ConfigureAwait(false);
            var emoteStatsCount = await Context.EmoteStats.CountAsync().ConfigureAwait(false);
            var channelStatsCount = await Context.ChannelStats.CountAsync().ConfigureAwait(false);
            var teamSearchCount = await Context.TeamSearch.CountAsync().ConfigureAwait(false);
            var tempUnverifyCount = await Context.TempUnverify.CountAsync().ConfigureAwait(false);
            var unverifyLogCount = await Context.UnverifyLog.CountAsync().ConfigureAwait(false);
            var commandLogCount = await Context.CommandLog.CountAsync().ConfigureAwait(false);

            return new Dictionary<string, int>()
            {
                { "AutoReply", autoReplyCount },
                { "EmoteStats", emoteStatsCount },
                { "ChannelStats", channelStatsCount },
                { "TeamSearch", teamSearchCount },
                { "TempUnverify", tempUnverifyCount },
                { "UnverifyLog", unverifyLogCount },
                { "CommandLog", commandLogCount }
            };
        }
    }
}

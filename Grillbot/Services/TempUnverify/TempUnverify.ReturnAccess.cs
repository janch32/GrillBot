﻿using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Database;
using Grillbot.Database.Entity;
using Grillbot.Database.Entity.UnverifyLog;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        private void ReturnAccess(object item)
        {
            if (item is TempUnverifyItem unverify)
            {
                var guild = Client.GetGuild(unverify.GuildIDSnowflake);
                if (guild == null) return;

                var user = guild.GetUserFromGuildAsync(unverify.UserID).Result;
                if (user == null)
                {
                    Logger.Write($"Invalid unverify. User not found. {JsonConvert.SerializeObject(unverify)}");
                    return;
                }

                var rolesToReturn = unverify.DeserializedRolesToReturn;
                var roles = guild.Roles.Where(o => rolesToReturn.Contains(o.Name) && !user.Roles.Any(x => x.Id == o.Id)).ToList();

                var isAutoRemove = (unverify.GetEndDatetime() - DateTime.Now).Ticks <= 0;

                if (isAutoRemove)
                {
                    using (var repository = new GrillBotRepository(Config))
                    {
                        var data = new UnverifyLogRemove()
                        {
                            Overrides = unverify.DeserializedChannelOverrides,
                            Roles = unverify.DeserializedRolesToReturn
                        };

                        data.SetUser(user);

                        repository.TempUnverify
                            .LogOperationAsync(UnverifyLogOperation.AutoRemove, Client.CurrentUser, guild, data)
                            .GetAwaiter()
                            .GetResult();
                    }
                }

                var overrides = unverify.DeserializedChannelOverrides
                    .Select(o => new { channel = guild.GetChannel(o.ChannelIdSnowflake), channelOverride = o })
                    .Where(o => o.channel != null)
                    .ToList();

                Logger.Write($"ReturnAccess User: {user.GetFullName()} ({user.Id}) Roles: {string.Join(", ", rolesToReturn)} " +
                    $"ExtraChannels: {string.Join(", ", overrides.Select(o => $"{o.channelOverride.ChannelId}|{o.channelOverride.AllowValue}|{o.channelOverride.DenyValue}"))}");

                user.AddRolesAsync(roles).GetAwaiter().GetResult();

                foreach(var channelOverride in overrides)
                {
                    channelOverride.channel
                        .AddPermissionOverwriteAsync(user, channelOverride.channelOverride.GetPermissions())
                        .GetAwaiter()
                        .GetResult();
                }

                FindAndToggleMutedRole(user, guild, false).GetAwaiter().GetResult();
                RemoveOverwritesForPreprocessedChannels(user, guild, overrides.Select(o => o.channelOverride).ToList()).GetAwaiter().GetResult();

                using (var repository = new GrillBotRepository(Config))
                {
                    repository.TempUnverify.RemoveItem(unverify.ID);
                }

                unverify.Dispose();
                Data.RemoveAll(o => o.ID == unverify.ID);
            }
        }

        public async Task<string> ReturnAccessAsync(int id, SocketUser fromUser)
        {
            using (var repository = new GrillBotRepository(Config))
            {
                var item = await repository.TempUnverify.FindItemByIDAsync(id).ConfigureAwait(false);

                if (item == null)
                    throw new ArgumentException($"Odebrání přístupu s ID {id} nebylo v databázi nalezeno.");

                var guild = Client.GetGuild(Convert.ToUInt64(item.GuildID));
                var user = await guild.GetUserFromGuildAsync(item.UserID).ConfigureAwait(false);

                if (user == null)
                    throw new ArgumentException($"Uživatel s ID **{item.UserID}** nebyl na serveru **{guild.Name}** nalezen.");

                var data = new UnverifyLogRemove()
                {
                    Overrides = item.DeserializedChannelOverrides,
                    Roles = item.DeserializedRolesToReturn
                };
                data.SetUser(user);

                await repository.TempUnverify.LogOperationAsync(UnverifyLogOperation.Remove, fromUser, guild, data).ConfigureAwait(false);

                ReturnAccess(item);
                return $"Předčasné vrácení přístupu pro uživatele **{user.GetFullName()}** bylo dokončeno.";
            }
        }
    }
}

﻿using Discord;
using Discord.WebSocket;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Logger.LoggerMethods;
using Grillbot.Services.MessageCache;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Grillbot.Services.Logger
{
    public class Logger : IDisposable
    {
        private HttpClient HttpClient { get; }
        private DiscordSocketClient Client { get; }
        private Configuration Config { get; }
        private IMessageCache MessageCache { get; }
        private BotLoggingService LoggingService { get; }

        public Dictionary<string, uint> Counters { get; }

        public Logger(DiscordSocketClient client, IOptions<Configuration> config, IMessageCache messageCache, BotLoggingService loggingService)
        {
            Client = client;
            Config = config.Value;
            MessageCache = messageCache;
            LoggingService = loggingService;

            Counters = new Dictionary<string, uint>();

            HttpClient = new HttpClient();
        }

        public async Task OnGuildMemberUpdatedAsync(SocketGuildUser guildUserBefore, SocketGuildUser guildUserAfter)
        {
            var method = new GuildMemberUpdated(Client, Config);
            var result = await method.ProcessAsync(guildUserBefore, guildUserAfter).ConfigureAwait(false);

            if (result)
                IncrementEventHandle("GuildMemberUpdated");
        }

        public async Task OnMessageDelete(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            var method = new MessageDeleted(Client, Config, MessageCache, HttpClient, LoggingService);
            await method.ProcessAsync(message, channel).ConfigureAwait(false);

            IncrementEventHandle("MessageDeleted");
        }

        public async Task OnMessageEdited(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
        {
            var method = new MessageEdited(Client, Config, MessageCache);
            var result = await method.ProcessAsync(messageBefore, messageAfter, channel).ConfigureAwait(false);

            if (result)
                IncrementEventHandle("MessageEdited");
        }

        public async Task OnUserJoined(SocketGuildUser user)
        {
            var method = new UserJoined(Client, Config);
            await method.ProcessAsync(user).ConfigureAwait(false);

            IncrementEventHandle("UserJoined");
        }

        public async Task OnUserLeft(SocketGuildUser user)
        {
            var method = new UserLeft(Client, Config);
            await method.ProcessAsync(user).ConfigureAwait(false);

            IncrementEventHandle("UserLeft");
        }

        private void IncrementEventHandle(string name)
        {
            if (!Counters.ContainsKey(name))
                Counters.Add(name, 1);
            else
                Counters[name]++;
        }

        public void Dispose()
        {
            HttpClient.Dispose();
            Counters.Clear();
        }
    }
}

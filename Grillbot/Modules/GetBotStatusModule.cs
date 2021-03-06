﻿using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Helpers;
using Grillbot.Services.Statistics;
using Grillbot.Services.Preconditions;
using Grillbot.Services;
using Grillbot.Extensions;
using Grillbot.Models.Embed;

namespace Grillbot.Modules
{
    [Group("status")]
    [Name("Stav bota")]
    [RequirePermissions]
    public class GetBotStatusModule : BotModuleBase
    {
        private CalledEventStats CalledEventStats { get; }
        private BotStatusService BotStatusService { get; }

        public GetBotStatusModule(CalledEventStats calledEventStats, BotStatusService botStatusService)
        {
            CalledEventStats = calledEventStats;
            BotStatusService = botStatusService;
        }

        [Command("")]
        [Summary("Vytiskne diagnostické informace o botovi.")]
        public async Task StatusAsync()
        {
            var data = BotStatusService.GetSimpleStatus();

            var embed = new BotEmbed(Context.Message.Author, title: "Stav bota")
                .WithFields(
                    new EmbedFieldBuilder().WithName("Využití RAM").WithValue(data.RamUsage).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Běží od").WithValue(data.StartTime.ToLocaleDatetime()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Počet vláken").WithValue(data.ThreadStatus).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Průměrná doba reakce").WithValue(data.AvgReactTime).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Aktivní CPU čas").WithValue(data.ActiveCpuTime).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Instance").WithValue(data.InstanceType).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Počet akt. tokenů").WithValue(data.ActiveWebTokensCount).WithIsInline(true)
                );

            await ReplyAsync("", embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("call")]
        public async Task PrintCallStatsAsync()
        {
            var data = BotStatusService.GetCallStats();

            if (data.Count == 0)
                return;

            var embed = new BotEmbed(Context.Message.Author, title: "Statistika příkazů")
                .WithFields(
                    new EmbedFieldBuilder().WithName("Příkaz").WithValue(string.Join("\n", data.Select(o => o.Command))).WithIsInline(true),

                    new EmbedFieldBuilder()
                        .WithName("Počet volání")
                        .WithValue(string.Join("\n", data.Select(o => FormatHelper.FormatWithSpaces(o.CallsCount))))
                        .WithIsInline(true),

                    new EmbedFieldBuilder().WithName("Průměrná doba").WithValue(string.Join("\n", data.Select(o => $"{o.AverageTime}ms"))).WithIsInline(true)
                );

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("logger")]
        public async Task PrintLoggerStatistics()
        {
            await DoAsync(async () =>
            {
                var data = BotStatusService.GetLoggerStats();

                if (data.Count == 0)
                    throw new ArgumentException("Ještě jsem nic nezalogoval.");

                var embed = new BotEmbed(Context.Message.Author, title: "Statistiky logování")
                    .WithFields(
                        new EmbedFieldBuilder().WithName("Událost").WithValue(string.Join("\n", data.Select(o => o.Key))).WithIsInline(true),
                        new EmbedFieldBuilder().WithName("Počet").WithValue(string.Join("\n", data.Select(o => o.Value))).WithIsInline(true)
                    );

                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Command("events")]
        public async Task PrintEventStatistics()
        {
            await DoAsync(async () =>
            {
                var data = CalledEventStats.ToFormatedDictionary();

                if (data.Count == 0)
                    throw new ArgumentException("Ještě nebyla zavolána žádná událost.");

                var embed = new BotEmbed(Context.Message.Author, title: "Statistika zavolaných událostí")
                    .WithFields(
                        new EmbedFieldBuilder().WithName("Událost").WithValue(string.Join("\n", data.Select(o => o.Key))).WithIsInline(true),
                        new EmbedFieldBuilder().WithName("Počet").WithValue(string.Join("\n", data.Select(o => o.Value))).WithIsInline(true)
                    );

                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Command("db")]
        [Summary("Počty záznamů v databázi")]
        public async Task GetDbStatus()
        {
            await DoAsync(async () =>
            {
                var data = await BotStatusService.GetDbReport().ConfigureAwait(false);

                var embed = new BotEmbed(Context.Message.Author)
                    .WithFields(data.Select(o => new EmbedFieldBuilder().WithName(o.Key).WithValue(FormatHelper.FormatWithSpaces(o.Value))));

                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}

﻿using Discord;
using Discord.Commands;
using Grillbot.Models.Embed;
using Grillbot.Services.Math;
using Grillbot.Services.Preconditions;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Name("Počítání")]
    [RequirePermissions]
    public class MathModule : BotModuleBase
    {
        private MathService Calculator { get; }

        public MathModule(MathService calculator)
        {
            Calculator = calculator;
        }

        [Command("solve")]
        public async Task SolveAsync([Remainder] string expression)
        {
            var result = Calculator.Solve(expression, Context.Message);

            var embed = new BotEmbed(Context.Message.Author, Color.Green)
                .AddField("Výraz", $"`{expression}`", false);

            if (result == null)
            {
                embed
                    .SetColor(Color.Red)
                    .WithTitle("Při zpracování výrazu došlo k neznámé chybě.");

                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                return;
            }

            if (!result.IsValid)
            {
                embed.SetColor(Color.Red);

                if (result.IsTimeout)
                {
                    embed
                        .WithTitle("Vypršel časový limit pro výpočet výrazu.")
                        .AddField(o => o.WithName("Maximální doba zpracování").WithValue(result.GetAssignedComputingTime()));
                }
                else
                {
                    embed
                        .WithTitle("Výpočet nebyl úspěšně proveden.")
                        .AddField("Chybové hlášení", result.ErrorMessage.Trim(), false);
                }

                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                return;
            }

            embed
                .AddField("Výsledek", result.Result.ToString(), true)
                .AddField(o => o.WithName("Doba zpracování").WithValue(result.GetComputingTime()).WithIsInline(true));

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }
    }
}

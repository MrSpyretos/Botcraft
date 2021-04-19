using Botcraft.Common;
using Botcraft.Models;
using Botcraft.Services;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Botcraft.Modules
{
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        private readonly ServerServices _serverServices;
        public FunModule(ServerServices serverServices)
        {
            _serverServices = serverServices;
        }
        #region gamble
        private readonly Random _random = new Random();
        [Command("roll"), Summary("Rolls a 100 sided dice.")]
        [Alias("r20")]
        public async Task Roll()
        {
            int value = _random.RandomNumber(1, 100);
            await ReplyAsync("A 20-sided dice was rolled, and landed on: " + value);
        }
        [Command("flipcoin"), Summary("Flips a two sided coin.")]
        [Alias("flip", "tosscoin", "coinflip")]
        public async Task FlipCoin()
        {
            var value = _random.RandomNumber(1, 2);

            switch (value)
            {
                case 1:
                    await ReplyAsync("A coin has been flipped, and landed on: **Heads**");
                    break;
                case 2:
                    await ReplyAsync("A coin has been flipped, and landed on: **Tails**");
                    break;
                default:
                    await ReplyAsync("A coin had been flipped, but got lost while landing.");
                    break;
            }
        }
        [Command("rolldice"), Summary("Rolls a x sided dice.")]
        public async Task RollDice(int numberOfDice = 1)
        {
            if (numberOfDice > 10)
            {
                await ReplyAsync("You can not roll more than 10 dice at one time, " + Context.User.Mention);
                numberOfDice = 10;
            }
            else if (numberOfDice < 1)
            {
                await ReplyAsync("You need to roll at least 1 dice, " + Context.User.Mention);
                return;
            }

            EmbedBuilder eb = new EmbedBuilder()
                .WithDescription(Context.User.Mention + " rolled " + numberOfDice + " 6-sided dice.");

            int totalOfRoll = 0;
            for (int i = 0; i < numberOfDice; i++)
            {
                var roll = _random.RandomNumber(1, 6);
                totalOfRoll += roll;

                eb.AddField("Dice " + (i + 1), roll.ToString(), true);
            }

            eb.AddField("Sum of roll", totalOfRoll);
            eb.WithFooter("Did you know? You can roll more dice by doing \"" +
                _serverServices.GetGuildPrefix(Context.Guild.Id)  + "rolldice [number of dice]\"!");
            
            await ReplyAsync("", false, eb.Build());
        }
        #endregion

    }
}

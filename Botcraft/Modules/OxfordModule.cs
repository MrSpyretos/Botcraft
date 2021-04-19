using Botcraft.Models;
using Botcraft.Services;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Botcraft.Modules
{
    public class OxfordModule : ModuleBase<SocketCommandContext>
    {
        private readonly OxfordApi _oxApi;
        private readonly ChannelServices _channelServices;
        public OxfordModule(OxfordApi oxApi, ChannelServices channelServices)
        {
            _oxApi = oxApi;
            _channelServices = channelServices;
        }
        [Command("define", RunMode = RunMode.Async)]
        [Summary("Get the definition of a word")]
        public async Task DefineWord([Remainder] string args)
        {
            StringBuilder sb = new StringBuilder();
            var embed = new EmbedBuilder();
            embed.WithColor(new Color(0, 255, 0));
            embed.ThumbnailUrl = Context.User.GetAvatarUrl();
            var result = _oxApi.SearchOxford(args);
            OxfordResponses.OxfordDefinition definition = null;
            int limit = 2;
            if (result.metadata.total > 0)
            {
                definition = _oxApi.DefineOxford(result.results[0].id);
            }
            if (definition != null)
            {
                Console.WriteLine(definition.results[0].lexicalEntries.Count());
                embed.Title = $"Definition for: **{definition.results[0].id}**";
                for (int i = 0; i <= definition.results[0].lexicalEntries.Count() - 1 && i < limit; i++)
                {
                    var entries = definition.results[0].lexicalEntries[i].entries.FirstOrDefault();
                    if (entries.senses != null)
                    {
                        var senses = definition.results[0].lexicalEntries[i].entries[0].senses.FirstOrDefault();
                        sb.AppendLine($"**{definition.results[0].lexicalEntries[i].lexicalCategory}**");
                        if (senses.definitions != null)
                        {
                            sb.AppendLine($"{senses.definitions[0]}\n");
                        }
                        else if (senses.crossReferenceMarkers != null)
                        {
                            sb.AppendLine($"{senses.crossReferenceMarkers[0]}");
                        }
                        else
                        {
                            sb.AppendLine($"No definition found :(\n");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"{definition.results[0].lexicalEntries[0].derivativeOf[0].text}");
                        break;
                    }
                }
                var lexicalEntries = definition.results[0].lexicalEntries.FirstOrDefault();
                if (lexicalEntries.pronunciations != null)
                {
                    sb.AppendLine($"[Pronunciation]({lexicalEntries.pronunciations[0].audioFile})");
                }
                //await ReplyAsync(, isTTS: true);
            }
            else
            {
                embed.Title = $"Definition for: **{args}**";
                sb.AppendLine($"No definition found :(");
            }

            embed.Description = sb.ToString();
            await _channelServices.Reply(Context, embed);
        }
    }
}

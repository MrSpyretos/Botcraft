using Botcraft.Services;
using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Botcraft.Modules
{
    public class YoutubeModule : ModuleBase<SocketCommandContext>
    {
        private static ChannelServices _channelServices = null;
        private static YoutubeApi _youtubeApi = null;

        public YoutubeModule(ChannelServices channelServices, YoutubeApi youtubeApi)
        {
            if (_channelServices == null)
            {
                _channelServices = channelServices;
            }
            if (_youtubeApi == null)
            {
                _youtubeApi = youtubeApi;
            }
        }
        //Command that links just one video normally so it has play button
        [Command("ysearch", RunMode = RunMode.Async)]
        [Summary("Search YouTube for a specific keyword")]
        public async Task SearchYouTube([Remainder] string args = "")
        {
            string searchFor = string.Empty;
            var embed = new EmbedBuilder();
            var embedThumb = Context.User.GetAvatarUrl();
            StringBuilder sb = new StringBuilder();
            List<Google.Apis.YouTube.v3.Data.SearchResult> results = null;

            embed.ThumbnailUrl = embedThumb;

            if (string.IsNullOrEmpty(args))
            {

                embed.Title = $"No search term provided!";
                embed.WithColor(new Color(255, 0, 0));
                sb.AppendLine("Please provide a term to search for!");
                embed.Description = sb.ToString();
                await _channelServices.Reply(Context, embed);
                return;
            }
            else
            {
                searchFor = args;
                embed.WithColor(new Color(0, 255, 0));
                results = await _youtubeApi.SearchChannelsAsync(searchFor);
            }

            if (results != null)
            {
                string videoUrlPrefix = $"https://www.youtube.com/watch?v=";
                embed.Title = $"YouTube Search For (**{searchFor}**)";
                var thumbFromVideo = results.Where(r => r.Id.Kind == "youtube#video").Take(1).FirstOrDefault();
                if (thumbFromVideo != null)
                {
                    embed.ThumbnailUrl = thumbFromVideo.Snippet.Thumbnails.Default__.Url;
                }
                foreach (var result in results.Where(r => r.Id.Kind == "youtube#video").Take(3))
                {
                    string fullVideoUrl = string.Empty;
                    string videoId = string.Empty;
                    string description = string.Empty;
                    if (string.IsNullOrEmpty(result.Snippet.Description))
                    {
                        description = "No description available.";
                    }
                    else
                    {
                        description = result.Snippet.Description;
                    }
                    if (result.Id.VideoId != null)
                    {
                        fullVideoUrl = $"{videoUrlPrefix}{result.Id.VideoId.ToString()}";
                    }
                    sb.AppendLine($":video_camera: **__{result.Snippet.ChannelTitle}__** -> [**{result.Snippet.Title}**]({fullVideoUrl})\n\n *{description}*\n");
                }
                embed.Description = sb.ToString();
                await _channelServices.Reply(Context, embed);
            }
        }
    }
}

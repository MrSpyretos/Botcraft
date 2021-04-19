using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Botcraft.Common
{
    public static class Extensions
    {
        #region Success-Error
        public static async Task<IMessage> SendSuccessAsync(this ISocketMessageChannel channel, string title , string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(40, 240, 60))
                //.WithTitle(title)
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                    .WithIconUrl("https://mijnoldtimerverkopen.nl/wp-content/uploads/2019/11/vinkje-alt.png")
                    .WithName(title);
                })
                //.WithThumbnailUrl("https://mijnoldtimerverkopen.nl/wp-content/uploads/2019/11/vinkje-alt.png")
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }
        public static async Task<IMessage> SendErrorAsync(this ISocketMessageChannel channel, string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(240, 80, 60))
                //.WithTitle(title)
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                    .WithIconUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/8/8e/Antu_dialog-error.svg/1024px-Antu_dialog-error.svg.png")
                    .WithName(title);
                })
                //.WithThumbnailUrl("https://mijnoldtimerverkopen.nl/wp-content/uploads/2019/11/vinkje-alt.png")
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }
        public static async Task<IMessage> SendLogAsync(this ITextChannel channel, string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(25, 155, 225))
                //.WithTitle(title)
                .WithDescription(description)
                .WithAuthor(author =>
                {
                    author
                    .WithIconUrl("https://cdn.shortpixel.ai/client/q_lossy,ret_img,w_300/https://www.curiosfera.com/wp-content/uploads/2016/08/remedio-casero-para-la-tos-regaliz.jpg")
                    .WithName(title);
                })
                .Build();

            var message = await channel.SendMessageAsync(embed: embed);
            return message;
        }
        #endregion
        #region randoms
        public static int RandomNumber(this Random source, int minValue, int maxValue)
        {
            return source.Next(minValue, maxValue + 1);
        }
        #endregion
    }
}

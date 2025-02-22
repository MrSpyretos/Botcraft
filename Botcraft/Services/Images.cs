﻿using Discord.WebSocket;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Threading.Tasks;

namespace Botcraft.Utilities
{
    public class Images
    {
        public async Task<string> CreateImageAsync(SocketGuildUser user,string url= "https://images.unsplash.com/photo-1610573515515-f3d9d85c2f88?ixid=MXwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHw%3D&ixlib=rb-1.2.1&auto=format&fit=crop&w=1050&q=80")
        {
            var avatar = await FetchImageAsync(user.GetAvatarUrl(size: 2048, format: Discord.ImageFormat.Png)?? user.GetDefaultAvatarUrl());
            var background = await FetchImageAsync(url);
            background = CropToBanner(background);
            avatar = ClipImageToCircle(avatar);

            var bitmap = avatar as Bitmap;
            bitmap?.MakeTransparent();

            var banner = CopyRegionIntoImage(bitmap, background);
            banner = DrawTextToImage(banner, $"{user.Username}#{user.Discriminator} joined the server", $"Member #{user.Guild.MemberCount}");

            string path = $"{Guid.NewGuid()}.png";
            banner.Save(path);
            return await Task.FromResult(path);
        }
        private static Bitmap CropToBanner(Image image)
        {
            var originalWidth = image.Width;
            var originalHeight = image.Height;
            var destinationSize = new Size(1100, 450);

            var heightRatio = (float)originalHeight / destinationSize.Height;
            var widtchRatio = (float)originalWidth / destinationSize.Width;

            var ratio = Math.Min(heightRatio, widtchRatio);

            var heightScale = Convert.ToInt32(destinationSize.Height * ratio);
            var widthScale = Convert.ToInt32(destinationSize.Width * ratio);

            var startY = (originalHeight - heightScale)/2;
            var startX = (originalWidth - widthScale)/2;

            var sourceRectangle = new Rectangle(startX, startY, widthScale, heightScale);
            var bitmap = new Bitmap(destinationSize.Width, destinationSize.Height);
            var destinationRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            using var g = Graphics.FromImage(bitmap);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);

            return bitmap;
        }
        private Image ClipImageToCircle(Image image)
        {
            Image destination = new Bitmap(image.Width, image.Height, image.PixelFormat);
            var radius = image.Width / 2;
            var x = image.Width / 2;
            var y = image.Height / 2;

            using Graphics g = Graphics.FromImage(destination);
            var r = new Rectangle(x - radius, y - radius, radius * 2, radius * 2);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (Brush brush = new SolidBrush(Color.Transparent))
            {
                g.FillRectangle(brush, 0, 0, destination.Width, destination.Height);
            }
            var path = new GraphicsPath();
            path.AddEllipse(r);
            g.SetClip(path);
            g.DrawImage(image, 0, 0);
            return destination;

        }
        private Image CopyRegionIntoImage(Image source , Image destination)
        {
            using var grD = Graphics.FromImage(destination);
            var x = (destination.Width / 2) - 110;
            var y = (destination.Height / 2) - 155;

            grD.DrawImage(source, x, y, 220, 220);
            return destination;
        }
        private Image DrawTextToImage(Image image,string header , string subheader)
        {
            var font = new Font("Times New Roman", 30, FontStyle.Regular);
            var fontsmall = new Font("Times new Roman", 20, FontStyle.Regular);

            var brushWhite = new SolidBrush(Color.White);
            var brushGrey = new SolidBrush(ColorTranslator.FromHtml("#838383"));

            var headerX = image.Width / 2;
            var headerY = (image.Height / 2) + 115;

            var subheaderX = image.Width / 2;
            var subheaderY = (image.Height / 2) + 160;

            var drawFormat = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };
            using var GrD = Graphics.FromImage(image);
            GrD.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            GrD.DrawString(header, font, brushWhite, headerX, headerY , drawFormat);
            GrD.DrawString(subheader, fontsmall, brushGrey, subheaderX, subheaderY , drawFormat);

            var img = new Bitmap(image);
            return img;
        }
        private async Task<Image> FetchImageAsync(string url)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var backupResponse = await client.GetAsync("https://images.unsplash.com/photo-1610573515515-f3d9d85c2f88?ixid=MXwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHw%3D&ixlib=rb-1.2.1&auto=format&fit=crop&w=1050&q=80");
                var backupStream = await backupResponse.Content.ReadAsStreamAsync();
                return Image.FromStream(backupStream);
            }

            var stream = await response.Content.ReadAsStreamAsync();
            return Image.FromStream(stream);
        }
    }
}

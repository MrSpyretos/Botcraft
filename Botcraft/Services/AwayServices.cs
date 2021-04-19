using Botcraft.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Botcraft.Services
{
    public class AwayServices
    {
        public void SetAwayUser(AwaySystem awayInfo)
        {

            var awayUser = new AwaySystem();

            using (var db = new DiscbotContext())
            {
                awayUser = db.AwaySystem.AsEnumerable().Where(a => a.UserName == awayInfo.UserName).FirstOrDefault();
                if (awayUser == null)
                {
                    db.AwaySystem.Add(awayInfo);
                }
                else
                {
                    awayUser.Status = awayInfo.Status;
                    awayUser.Message = awayInfo.Message;
                    awayUser.TimeAway = awayInfo.TimeAway;
                }
                db.SaveChanges();
            }

        }

        public AwaySystem GetAwayUser(string discordUserName)
        {
            var awayUser = new AwaySystem();
            using (var db = new DiscbotContext())
            {
                awayUser = db.AwaySystem.AsEnumerable().Where(a => a.UserName == discordUserName).FirstOrDefault();
            }
            return awayUser;
        }
    }
}

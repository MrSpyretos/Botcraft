using Botcraft.Database.Entities;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Botcraft.Services
{
    public class ConfigurationServices
    {
        private string _prefix;
        public ConfigurationServices(ServerServices serverServices, Server server)
        {
            _prefix = serverServices.GetGuildPrefix(server.Id).Result;
        }
        public async void AddWarning(ICommandContext context, IGuildUser userWarned)
        {
            using (var db = new DiscbotContext())
            {
                var warnings = db.Warnings.AsEnumerable().Where(w => w.ServerId == (long)context.Guild.Id && w.UserWarnedId == (long)userWarned.Id).FirstOrDefault();
                if (warnings != null)
                {
                    warnings.NumWarnings = warnings.NumWarnings + 1;
                }
                else
                {
                    db.Warnings.Add(new Warnings
                    {
                        ServerId = (long)context.Guild.Id,
                        ServerName = context.Guild.Name,
                        UserWarnedId = (long)userWarned.Id,
                        UserWarnedName = userWarned.Username,
                        IssuerId = (long)context.User.Id,
                        IssuerName = context.User.Username,
                        TimeIssued = DateTime.Now,
                        NumWarnings = 1
                    });
                }
                await db.SaveChangesAsync();
            }
        }

        public async void ResetWarnings(Warnings warning)
        {
            using (var db = new DiscbotContext())
            {
                var currentWarning = db.Warnings.AsEnumerable().Where(w => w.Id == warning.Id).FirstOrDefault();
                if (currentWarning != null)
                {
                    db.Warnings.Remove(currentWarning);
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task<Warnings> GetWarning(ICommandContext context, IGuildUser userWarned)
        {
            Warnings warning = null;
            using (var db = new DiscbotContext())
            {
                warning = db.Warnings.AsEnumerable().Where(w => w.ServerId == (long)context.Guild.Id && w.UserWarnedId == (long)userWarned.Id).FirstOrDefault();
            }
            return warning;
        }

        public async Task<string> SetNoteInfo(ICommandContext Context, string noteText)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                using (var db = new DiscbotContext())
                {
                    var currentNote = db.Notes.FirstOrDefault(c => c.ServerId == (long)Context.Guild.Id);
                    if (currentNote == null)
                    {
                        Note n = new Note()
                        {
                            Note1 = noteText,
                            ServerId = (long)Context.Guild.Id,
                            ServerName = Context.Guild.Name,
                            SetBy = Context.User.Username,
                            SetById = (long)Context.User.Id,
                            TimeSet = DateTime.Now
                        };
                        db.Notes.Add(n);
                    }
                    else
                    {
                        currentNote.Note1 = noteText;
                        currentNote.SetBy = Context.User.Username;
                        currentNote.SetById = (long)Context.User.Id;
                        currentNote.TimeSet = DateTime.Now;
                    }
                    await db.SaveChangesAsync();
                }
                sb.AppendLine($"Note successfully added for server [**{Context.Guild.Name}**] by [**{Context.User.Username}**]!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting note {ex.Message}");
                sb.AppendLine($"Something went wrong adding a note for server [**{Context.Guild.Name}**] :(");
            }
            return sb.ToString();
        }

        public async Task<string> GetNoteInfo(ICommandContext Context)
        {
            StringBuilder sb = new StringBuilder();
            using (var db = new DiscbotContext())
            {
                var note = db.Notes.FirstOrDefault(n => n.ServerId == (long)Context.Guild.Id);
                if (note == null)
                {
                    sb.AppendLine($"Unable to find a note for server [{Context.Guild.Name}], perhaps try adding one by using {_prefix}set-note \"Note goes here!\"");
                }
                else
                {
                    sb.AppendLine(note.Note1);
                    sb.AppendLine();
                    sb.Append($"*Note set by [**{note.SetBy}**] on [**{note.TimeSet}**]*");
                }
            }
            return sb.ToString();
        }
    }
}

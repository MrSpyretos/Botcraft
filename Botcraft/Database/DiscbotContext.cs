
using Botcraft.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Botcraft
{
    public class DiscbotContext :DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<AutoRole> AutoRoles { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<ChannelOutput> ChannelOutputs { get; set; }
        public DbSet<AwaySystem> AwaySystem { get; set; }
        public DbSet<Giphy> Giphy { get; set; }
        public DbSet<WordList> WordList { get; set; }
        public DbSet<Warnings> Warnings { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<ServerGreeting> ServerGreetings { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(connectionString);

        public readonly static string connectionString = "Server =localhost,1433; " +
               "Database =disc; " +
               "User Id =sa; " +
               "Password =Admin!@#;";
    }
}

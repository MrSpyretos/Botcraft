using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Botcraft.Migrations
{
    public partial class v2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutoRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    ServerId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AwaySystem",
                columns: table => new
                {
                    AwayId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true),
                    TimeAway = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AwaySystem", x => x.AwayId);
                });

            migrationBuilder.CreateTable(
                name: "ChannelOutputs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerId = table.Column<long>(type: "bigint", nullable: true),
                    ChannelName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChannelId = table.Column<long>(type: "bigint", nullable: true),
                    SetByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetById = table.Column<long>(type: "bigint", nullable: true),
                    SetTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelOutputs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Giphy",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerId = table.Column<long>(type: "bigint", nullable: true),
                    GiphyEnabled = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Giphy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Note1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerId = table.Column<long>(type: "bigint", nullable: true),
                    SetBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetById = table.Column<long>(type: "bigint", nullable: true),
                    TimeSet = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ranks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    ServerId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ranks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerGreetings",
                columns: table => new
                {
                    DiscordGuildId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GreetUsers = table.Column<bool>(type: "bit", nullable: true),
                    Greeting = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetById = table.Column<long>(type: "bigint", nullable: true),
                    SetByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeSet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PartingMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GreetingChannelId = table.Column<long>(type: "bigint", nullable: true),
                    GreetingChannelName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerGreetings", x => x.DiscordGuildId);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Welcome = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    Background = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Logs = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warnings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServerId = table.Column<long>(type: "bigint", nullable: false),
                    ServerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserWarnedId = table.Column<long>(type: "bigint", nullable: false),
                    UserWarnedName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssuerId = table.Column<long>(type: "bigint", nullable: false),
                    IssuerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeIssued = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumWarnings = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warnings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WordList",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServerId = table.Column<long>(type: "bigint", nullable: false),
                    ServerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Word = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetById = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordList", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutoRoles");

            migrationBuilder.DropTable(
                name: "AwaySystem");

            migrationBuilder.DropTable(
                name: "ChannelOutputs");

            migrationBuilder.DropTable(
                name: "Giphy");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "Ranks");

            migrationBuilder.DropTable(
                name: "ServerGreetings");

            migrationBuilder.DropTable(
                name: "Servers");

            migrationBuilder.DropTable(
                name: "Warnings");

            migrationBuilder.DropTable(
                name: "WordList");
        }
    }
}

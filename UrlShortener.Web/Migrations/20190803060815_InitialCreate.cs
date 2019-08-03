using Microsoft.EntityFrameworkCore.Migrations;

namespace UrlShortener.Web.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShortenedUrl",
                columns: table => new
                {
                    Alias = table.Column<string>(nullable: false),
                    ShortUrl = table.Column<string>(nullable: true),
                    LongUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortenedUrl", x => x.Alias);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShortenedUrl");
        }
    }
}

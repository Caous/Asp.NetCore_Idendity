using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApplication.Identity.Migrations
{
    public partial class AdicaoMembers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Member",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Member",
                table: "AspNetUsers");
        }
    }
}

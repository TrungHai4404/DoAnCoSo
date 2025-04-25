using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyTruyenThong_TuVan.Migrations
{
    /// <inheritdoc />
    public partial class updatemodel1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResidentName",
                table: "VoteResults");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResidentName",
                table: "VoteResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

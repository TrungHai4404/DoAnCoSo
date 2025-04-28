using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyTruyenThong_TuVan.Migrations
{
    /// <inheritdoc />
    public partial class updateVVote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Votes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Votes");
        }
    }
}

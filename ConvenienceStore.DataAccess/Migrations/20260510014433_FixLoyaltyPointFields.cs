using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConvenienceStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixLoyaltyPointFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HoaDonId",
                table: "LichSuDiems",
                newName: "DonHangId");

            migrationBuilder.AlterColumn<string>(
                name: "NguoiDungId",
                table: "LichSuDiems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "DiemTichLuy",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiemTichLuy",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "DonHangId",
                table: "LichSuDiems",
                newName: "HoaDonId");

            migrationBuilder.AlterColumn<int>(
                name: "NguoiDungId",
                table: "LichSuDiems",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}

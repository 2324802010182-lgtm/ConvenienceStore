using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConvenienceStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ThemKhuyenMaiChoSanPham : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TenSanPham",
                table: "SanPhams",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayBatDauKhuyenMai",
                table: "SanPhams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayKetThucKhuyenMai",
                table: "SanPhams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PhanTramGiam",
                table: "SanPhams",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayBatDauKhuyenMai",
                table: "SanPhams");

            migrationBuilder.DropColumn(
                name: "NgayKetThucKhuyenMai",
                table: "SanPhams");

            migrationBuilder.DropColumn(
                name: "PhanTramGiam",
                table: "SanPhams");

            migrationBuilder.AlterColumn<string>(
                name: "TenSanPham",
                table: "SanPhams",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}

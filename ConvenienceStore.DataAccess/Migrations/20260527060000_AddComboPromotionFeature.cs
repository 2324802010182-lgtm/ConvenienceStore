using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConvenienceStore.DataAccess.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260527060000_AddComboPromotionFeature")]
    public partial class AddComboPromotionFeature : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Combos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenCombo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GiaCombo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComboSanPhams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComboId = table.Column<int>(type: "int", nullable: false),
                    SanPhamId = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboSanPhams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComboSanPhams_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComboSanPhams_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddColumn<int>(
                name: "ComboId",
                table: "ChiTietDonHangs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HinhAnhHienThi",
                table: "ChiTietDonHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LaCombo",
                table: "ChiTietDonHangs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MoTaCombo",
                table: "ChiTietDonHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenHienThi",
                table: "ChiTietDonHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComboSanPhams_ComboId",
                table: "ComboSanPhams",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboSanPhams_SanPhamId",
                table: "ComboSanPhams",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_ComboId",
                table: "ChiTietDonHangs",
                column: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietDonHangs_Combos_ComboId",
                table: "ChiTietDonHangs",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietDonHangs_Combos_ComboId",
                table: "ChiTietDonHangs");

            migrationBuilder.DropTable(
                name: "ComboSanPhams");

            migrationBuilder.DropTable(
                name: "Combos");

            migrationBuilder.DropIndex(
                name: "IX_ChiTietDonHangs_ComboId",
                table: "ChiTietDonHangs");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "ChiTietDonHangs");

            migrationBuilder.DropColumn(
                name: "HinhAnhHienThi",
                table: "ChiTietDonHangs");

            migrationBuilder.DropColumn(
                name: "LaCombo",
                table: "ChiTietDonHangs");

            migrationBuilder.DropColumn(
                name: "MoTaCombo",
                table: "ChiTietDonHangs");

            migrationBuilder.DropColumn(
                name: "TenHienThi",
                table: "ChiTietDonHangs");
        }
    }
}

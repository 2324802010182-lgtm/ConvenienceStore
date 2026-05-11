using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConvenienceStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddGopYAndChatFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GopYs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiGuiId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoaiGopY = table.Column<int>(type: "int", nullable: false),
                    SanPhamId = table.Column<int>(type: "int", nullable: true),
                    NhanVienId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TieuDe = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaDong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GopYs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GopYs_AspNetUsers_NguoiGuiId",
                        column: x => x.NguoiGuiId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GopYs_AspNetUsers_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GopYs_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TinNhanGopYs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GopYId = table.Column<int>(type: "int", nullable: false),
                    NguoiGuiId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinNhanGopYs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TinNhanGopYs_AspNetUsers_NguoiGuiId",
                        column: x => x.NguoiGuiId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TinNhanGopYs_GopYs_GopYId",
                        column: x => x.GopYId,
                        principalTable: "GopYs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GopYs_NguoiGuiId",
                table: "GopYs",
                column: "NguoiGuiId");

            migrationBuilder.CreateIndex(
                name: "IX_GopYs_NhanVienId",
                table: "GopYs",
                column: "NhanVienId");

            migrationBuilder.CreateIndex(
                name: "IX_GopYs_SanPhamId",
                table: "GopYs",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhanGopYs_GopYId",
                table: "TinNhanGopYs",
                column: "GopYId");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhanGopYs_NguoiGuiId",
                table: "TinNhanGopYs",
                column: "NguoiGuiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TinNhanGopYs");

            migrationBuilder.DropTable(
                name: "GopYs");
        }
    }
}

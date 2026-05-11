using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConvenienceStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddChatOnlineFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HoiThoaiChats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KhachHangId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NhanVienPhuTrachId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LanHoatDongCuoi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KhachHangDaDoc = table.Column<bool>(type: "bit", nullable: false),
                    QuanTriDaDoc = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoiThoaiChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoiThoaiChats_AspNetUsers_KhachHangId",
                        column: x => x.KhachHangId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoiThoaiChats_AspNetUsers_NhanVienPhuTrachId",
                        column: x => x.NhanVienPhuTrachId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TinNhanChats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoiThoaiChatId = table.Column<int>(type: "int", nullable: false),
                    NguoiGuiId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinNhanChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TinNhanChats_AspNetUsers_NguoiGuiId",
                        column: x => x.NguoiGuiId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TinNhanChats_HoiThoaiChats_HoiThoaiChatId",
                        column: x => x.HoiThoaiChatId,
                        principalTable: "HoiThoaiChats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HoiThoaiChats_KhachHangId",
                table: "HoiThoaiChats",
                column: "KhachHangId");

            migrationBuilder.CreateIndex(
                name: "IX_HoiThoaiChats_NhanVienPhuTrachId",
                table: "HoiThoaiChats",
                column: "NhanVienPhuTrachId");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhanChats_HoiThoaiChatId",
                table: "TinNhanChats",
                column: "HoiThoaiChatId");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhanChats_NguoiGuiId",
                table: "TinNhanChats",
                column: "NguoiGuiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TinNhanChats");

            migrationBuilder.DropTable(
                name: "HoiThoaiChats");
        }
    }
}

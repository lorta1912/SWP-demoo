using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWP391.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitDb2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CycleLength",
                table: "User",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MenstrualLength",
                table: "User",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MenstrualCycle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenstrualCycle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenstrualCycle_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PillIntakeCycle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PackSize = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PillIntakeCycle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PillIntakeCycle_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenstrualCycle_UserId",
                table: "MenstrualCycle",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PillIntakeCycle_UserId",
                table: "PillIntakeCycle",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenstrualCycle");

            migrationBuilder.DropTable(
                name: "PillIntakeCycle");

            migrationBuilder.DropColumn(
                name: "CycleLength",
                table: "User");

            migrationBuilder.DropColumn(
                name: "MenstrualLength",
                table: "User");
        }
    }
}

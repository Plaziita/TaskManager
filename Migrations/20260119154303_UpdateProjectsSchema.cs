using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TaskIds",
                table: "Projects");

            migrationBuilder.RenameTable(
                name: "Tasks",
                newName: "Issues");

            migrationBuilder.RenameColumn(
                name: "UserIds",
                table: "Projects",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "IsCompleted",
                table: "Issues",
                newName: "ProjectId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Issues",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Issues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Issues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Issues",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Issues",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Issues",
                table: "Issues",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ProjectUsers",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectUsers", x => new { x.ProjectId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ProjectUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectUsers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ProjectId",
                table: "Issues",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectUsers_UserId",
                table: "ProjectUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Projects_ProjectId",
                table: "Issues",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Projects_ProjectId",
                table: "Issues");

            migrationBuilder.DropTable(
                name: "ProjectUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Issues",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_ProjectId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Issues");

            migrationBuilder.RenameTable(
                name: "Issues",
                newName: "Tasks");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Projects",
                newName: "UserIds");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "Tasks",
                newName: "IsCompleted");

            migrationBuilder.AddColumn<string>(
                name: "TaskIds",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks",
                column: "Id");
        }
    }
}

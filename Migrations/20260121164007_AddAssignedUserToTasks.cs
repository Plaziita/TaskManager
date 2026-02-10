using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedUserToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedUserId",
                table: "Issues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Issues_AssignedUserId",
                table: "Issues",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_AspNetUsers_AssignedUserId",
                table: "Issues",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_AspNetUsers_AssignedUserId",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_AssignedUserId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "Issues");
        }
    }
}

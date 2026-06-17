using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseOperations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationTaskIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OperationTasks_CreatedAt",
                table: "OperationTasks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OperationTasks_IsCompleted",
                table: "OperationTasks",
                column: "IsCompleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OperationTasks_CreatedAt",
                table: "OperationTasks");

            migrationBuilder.DropIndex(
                name: "IX_OperationTasks_IsCompleted",
                table: "OperationTasks");
        }
    }
}

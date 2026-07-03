using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseOperations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationTaskRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "OperationTasks",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "OperationTasks");
        }
    }
}

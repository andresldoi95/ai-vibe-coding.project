using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletePaymentPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add payments.complete permission
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name", "Description", "Resource", "Action" },
                values: new object[] { Guid.NewGuid(), "payments.complete", "Complete payments", "payments", "complete" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove payments.complete permission
            migrationBuilder.Sql(@"
                DELETE FROM ""Permissions"" WHERE ""Name"" = 'payments.complete'
            ");
        }
    }
}

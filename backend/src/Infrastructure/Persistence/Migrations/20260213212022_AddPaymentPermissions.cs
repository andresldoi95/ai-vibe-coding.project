using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Payment permissions
            var permissions = new[]
            {
                (Guid.NewGuid(), "payments.read", "View payments", "payments", "read"),
                (Guid.NewGuid(), "payments.create", "Create payments", "payments", "create"),
                (Guid.NewGuid(), "payments.update", "Update payments", "payments", "update"),
                (Guid.NewGuid(), "payments.void", "Void payments", "payments", "void"),
                (Guid.NewGuid(), "payments.delete", "Delete payments", "payments", "delete")
            };

            foreach (var (id, name, description, resource, action) in permissions)
            {
                migrationBuilder.InsertData(
                    table: "Permissions",
                    columns: new[] { "Id", "Name", "Description", "Resource", "Action" },
                    values: new object[] { id, name, description, resource, action });
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove Payment permissions
            migrationBuilder.Sql(@"
                DELETE FROM ""Permissions"" WHERE ""Resource"" = 'payments'
            ");
        }
    }
}

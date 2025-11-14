using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EbayClone.API.Migrations
{
    /// <inheritdoc />
    public partial class edit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "b159dcaa-1524-4605-848b-9c7dc8256d19");

            migrationBuilder.AlterColumn<string>(
                name: "OrderId",
                table: "OrderItems",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "PasswordHash", "Rating", "Role", "TotalSales", "TrustLevel", "UserName" },
                values: new object[] { "92a2ef13-725b-4549-9ab2-10bd090ddecc", "admin@ebayclone.com", "admin123", 5.0m, "Admin", 0, 0, "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "92a2ef13-725b-4549-9ab2-10bd090ddecc");

            migrationBuilder.AlterColumn<string>(
                name: "OrderId",
                table: "OrderItems",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "PasswordHash", "Rating", "Role", "TotalSales", "TrustLevel", "UserName" },
                values: new object[] { "b159dcaa-1524-4605-848b-9c7dc8256d19", "admin@ebayclone.com", "admin123", 5.0m, "Admin", 0, 0, "Admin" });
        }
    }
}

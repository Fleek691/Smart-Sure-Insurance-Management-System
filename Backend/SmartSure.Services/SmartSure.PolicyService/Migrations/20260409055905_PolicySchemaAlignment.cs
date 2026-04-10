using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartSure.PolicyService.Migrations
{
    /// <inheritdoc />
    public partial class PolicySchemaAlignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BasePremium",
                table: "InsuranceSubTypes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.InsertData(
                table: "InsuranceTypes",
                columns: new[] { "TypeId", "TypeName" },
                values: new object[,]
                {
                    { 1, "Vehicle" },
                    { 2, "Home" }
                });

            migrationBuilder.InsertData(
                table: "InsuranceSubTypes",
                columns: new[] { "SubTypeId", "BasePremium", "SubTypeName", "TypeId" },
                values: new object[,]
                {
                    { 101, 15000m, "Mahindra", 1 },
                    { 102, 12000m, "Maruti Suzuki", 1 },
                    { 103, 13000m, "Hyundai", 1 },
                    { 104, 14000m, "Honda", 1 },
                    { 105, 13500m, "Tata Motors", 1 },
                    { 106, 16000m, "Toyota", 1 },
                    { 107, 14500m, "Kia", 1 },
                    { 108, 17000m, "Volkswagen", 1 },
                    { 109, 16500m, "Skoda", 1 },
                    { 110, 13000m, "Renault", 1 },
                    { 111, 14000m, "Nissan", 1 },
                    { 112, 15500m, "Ford", 1 },
                    { 113, 15000m, "MG Motor", 1 },
                    { 114, 18000m, "Jeep", 1 },
                    { 115, 25000m, "BMW", 1 },
                    { 116, 28000m, "Mercedes-Benz", 1 },
                    { 117, 26000m, "Audi", 1 },
                    { 118, 24000m, "Volvo", 1 },
                    { 201, 8000m, "Apartment", 2 },
                    { 202, 12000m, "Independent House", 2 },
                    { 203, 18000m, "Villa", 2 },
                    { 204, 15000m, "Bungalow", 2 },
                    { 205, 20000m, "Penthouse", 2 },
                    { 206, 6000m, "Studio Apartment", 2 },
                    { 207, 14000m, "Duplex", 2 },
                    { 208, 16000m, "Farmhouse", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 110);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 113);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 114);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 115);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 116);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 117);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 118);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 201);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 202);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 203);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 204);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 205);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 206);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 207);

            migrationBuilder.DeleteData(
                table: "InsuranceSubTypes",
                keyColumn: "SubTypeId",
                keyValue: 208);

            migrationBuilder.DeleteData(
                table: "InsuranceTypes",
                keyColumn: "TypeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "InsuranceTypes",
                keyColumn: "TypeId",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "BasePremium",
                table: "InsuranceSubTypes");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class D1CheckoutOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShippingApartment",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingBuildingNumber",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCity",
                table: "Users",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingDistrict",
                table: "Users",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingFullName",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShippingGovernorateId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingInstructions",
                table: "Users",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingPhone",
                table: "Users",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingStreet",
                table: "Users",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "Products",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE [Products] SET [Sku] = CONCAT('SKU-', CONVERT(nvarchar(20), [Id])) WHERE [Sku] IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "Sku",
                table: "Products",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Governorates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShippingFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Governorates", x => x.Id);
                    table.CheckConstraint("CK_Governorates_ShippingFee_NonNegative", "[ShippingFee] >= 0");
                });

            migrationBuilder.InsertData(
                table: "Governorates",
                columns: new[] { "Id", "Name", "ShippingFee" },
                values: new object[,]
                {
                    { 1, "Alexandria", 50m },
                    { 2, "Aswan", 75m },
                    { 3, "Asyut", 50m },
                    { 4, "Beheira", 50m },
                    { 5, "Beni Suef", 50m },
                    { 6, "Cairo", 50m },
                    { 7, "Dakahlia", 50m },
                    { 8, "Damietta", 50m },
                    { 9, "Faiyum", 50m },
                    { 10, "Gharbia", 50m },
                    { 11, "Giza", 50m },
                    { 12, "Ismailia", 50m },
                    { 13, "Kafr El Sheikh", 50m },
                    { 14, "Luxor", 50m },
                    { 15, "Matrouh", 75m },
                    { 16, "Minya", 50m },
                    { 17, "Monufia", 50m },
                    { 18, "New Valley", 75m },
                    { 19, "North Sinai", 75m },
                    { 20, "Port Said", 50m },
                    { 21, "Qalyubia", 50m },
                    { 22, "Qena", 50m },
                    { 23, "Red Sea", 75m },
                    { 24, "Sharqia", 50m },
                    { 25, "Sohag", 50m },
                    { 26, "South Sinai", 75m },
                    { 27, "Suez", 50m },
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ClientRequestId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingFullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShippingPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ShippingGovernorateId = table.Column<int>(type: "int", nullable: false),
                    ShippingGovernorateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShippingCity = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ShippingDistrict = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ShippingStreet = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ShippingBuildingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShippingApartment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShippingInstructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.CheckConstraint("CK_Orders_Money_NonNegative", "[Subtotal] >= 0 AND [ShippingFee] >= 0 AND [Total] >= 0");
                    table.CheckConstraint("CK_Orders_PaymentMethod", "[PaymentMethod] = 'COD'");
                    table.CheckConstraint("CK_Orders_PaymentStatus", "[PaymentStatus] IN ('Pending', 'Paid', 'Failed')");
                    table.CheckConstraint("CK_Orders_Status", "[Status] IN ('Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    SkuSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UnitPriceSnapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.CheckConstraint("CK_OrderItems_Money_NonNegative", "[UnitPriceSnapshot] >= 0 AND [LineTotal] >= 0");
                    table.CheckConstraint("CK_OrderItems_Quantity_Positive", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistory_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ShippingGovernorateId",
                table: "Users",
                column: "ShippingGovernorateId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku",
                table: "Products",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Governorates_Name",
                table: "Governorates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId_ClientRequestId",
                table: "Orders",
                columns: new[] { "UserId", "ClientRequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId_CreatedAt",
                table: "Orders",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_OrderId_ChangedAt",
                table: "OrderStatusHistory",
                columns: new[] { "OrderId", "ChangedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Governorates_ShippingGovernorateId",
                table: "Users",
                column: "ShippingGovernorateId",
                principalTable: "Governorates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Governorates_ShippingGovernorateId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Governorates");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "OrderStatusHistory");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Users_ShippingGovernorateId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Products_Sku",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShippingApartment",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShippingBuildingNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShippingCity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShippingDistrict",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShippingFullName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShippingGovernorateId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShippingInstructions",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShippingPhone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShippingStreet",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "Products");
        }
    }
}

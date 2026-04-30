using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyTech.Migrations
{
    /// <inheritdoc />
    public partial class ProposeOfferModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProposeItems",
                columns: table => new
                {
                    PropItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProdName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProdCategory = table.Column<int>(type: "int", nullable: false),
                    ProdCondition = table.Column<int>(type: "int", nullable: false),
                    ProdDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProdStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposeItems", x => x.PropItemId);
                });

            migrationBuilder.CreateTable(
                name: "AcceptOffers",
                columns: table => new
                {
                    OfferId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropItemId = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcceptOffers", x => x.OfferId);
                    table.ForeignKey(
                        name: "FK_AcceptOffers_ProposeItems_PropItemId",
                        column: x => x.PropItemId,
                        principalTable: "ProposeItems",
                        principalColumn: "PropItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProposedProdPic",
                columns: table => new
                {
                    ProposedId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SavedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProposeItemPropItemId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposedProdPic", x => x.ProposedId);
                    table.ForeignKey(
                        name: "FK_ProposedProdPic_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProposedProdPic_ProposeItems_ProposeItemPropItemId",
                        column: x => x.ProposeItemPropItemId,
                        principalTable: "ProposeItems",
                        principalColumn: "PropItemId");
                });

            migrationBuilder.CreateTable(
                name: "ReceiptPics",
                columns: table => new
                {
                    ReceiptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SavedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProposeItemPropItemId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptPics", x => x.ReceiptId);
                    table.ForeignKey(
                        name: "FK_ReceiptPics_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceiptPics_ProposeItems_ProposeItemPropItemId",
                        column: x => x.ProposeItemPropItemId,
                        principalTable: "ProposeItems",
                        principalColumn: "PropItemId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcceptOffers_PropItemId",
                table: "AcceptOffers",
                column: "PropItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposedProdPic_CustomerId",
                table: "ProposedProdPic",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposedProdPic_ProposeItemPropItemId",
                table: "ProposedProdPic",
                column: "ProposeItemPropItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPics_CustomerId",
                table: "ReceiptPics",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPics_ProposeItemPropItemId",
                table: "ReceiptPics",
                column: "ProposeItemPropItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcceptOffers");

            migrationBuilder.DropTable(
                name: "ProposedProdPic");

            migrationBuilder.DropTable(
                name: "ReceiptPics");

            migrationBuilder.DropTable(
                name: "ProposeItems");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyTech.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "Customers",
            //    columns: table => new
            //    {
            //        CustomerId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
            //        Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            //        Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        RegisteredOn = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Customers", x => x.CustomerId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "DeliveryDrivers",
            //    columns: table => new
            //    {
            //        DriverId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
            //        DriverName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        DriverSurname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        ResidentialAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
            //        LicenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        VehicleType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        VehicleNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        BankType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        IsAvailable = table.Column<bool>(type: "bit", nullable: false),
            //        ApplicationStatus = table.Column<int>(type: "int", nullable: false),
            //        Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        RegisteredOn = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_DeliveryDrivers", x => x.DriverId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "RepairItemCategories",
            //    columns: table => new
            //    {
            //        CategoryId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_RepairItemCategories", x => x.CategoryId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "shopowners",
            //    columns: table => new
            //    {
            //        OwnerId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        IdBookNumber = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
            //        Names = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        Surname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ResidentialAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            //        AccountNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        BankType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ShopAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            //        Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ApplicationStatus = table.Column<int>(type: "int", nullable: false),
            //        RegisteredOn = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_shopowners", x => x.OwnerId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "SystemAdmins",
            //    columns: table => new
            //    {
            //        AdminId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        RegisteredOn = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_SystemAdmins", x => x.AdminId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Carts",
            //    columns: table => new
            //    {
            //        CartId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        CustomerId = table.Column<int>(type: "int", nullable: false),
            //        DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        IsCheckedOut = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Carts", x => x.CartId);
            //        table.ForeignKey(
            //            name: "FK_Carts_Customers_CustomerId",
            //            column: x => x.CustomerId,
            //            principalTable: "Customers",
            //            principalColumn: "CustomerId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "DriverIdentityBooks",
            //    columns: table => new
            //    {
            //        DriverIdentityBookId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DriverId = table.Column<int>(type: "int", nullable: false),
            //        FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        SavedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_DriverIdentityBooks", x => x.DriverIdentityBookId);
            //        table.ForeignKey(
            //            name: "FK_DriverIdentityBooks_DeliveryDrivers_DriverId",
            //            column: x => x.DriverId,
            //            principalTable: "DeliveryDrivers",
            //            principalColumn: "DriverId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "DriversLicences",
            //    columns: table => new
            //    {
            //        LicenceId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DriverId = table.Column<int>(type: "int", nullable: false),
            //        FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        SavedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_DriversLicences", x => x.LicenceId);
            //        table.ForeignKey(
            //            name: "FK_DriversLicences_DeliveryDrivers_DriverId",
            //            column: x => x.DriverId,
            //            principalTable: "DeliveryDrivers",
            //            principalColumn: "DriverId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ProposeItems",
            //    columns: table => new
            //    {
            //        PropItemId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        CustomerId = table.Column<int>(type: "int", nullable: false),
            //        AssignedDriverId = table.Column<int>(type: "int", nullable: true),
            //        ProdName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ProdCategory = table.Column<int>(type: "int", nullable: false),
            //        ProdCondition = table.Column<int>(type: "int", nullable: false),
            //        ProdDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ProdStatus = table.Column<int>(type: "int", nullable: false),
            //        AmountProposed = table.Column<double>(type: "float", nullable: false),
            //        StoreAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        CustomerAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        SavedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CollectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        IsCollected = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ProposeItems", x => x.PropItemId);
            //        table.ForeignKey(
            //            name: "FK_ProposeItems_Customers_CustomerId",
            //            column: x => x.CustomerId,
            //            principalTable: "Customers",
            //            principalColumn: "CustomerId");
            //        table.ForeignKey(
            //            name: "FK_ProposeItems_DeliveryDrivers_AssignedDriverId",
            //            column: x => x.AssignedDriverId,
            //            principalTable: "DeliveryDrivers",
            //            principalColumn: "DriverId");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "RepairRequests",
            //    columns: table => new
            //    {
            //        RequestId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        CustomerId = table.Column<int>(type: "int", nullable: true),
            //        ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ItemDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        IssueDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Status = table.Column<int>(type: "int", nullable: false),
            //        CategoryId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_RepairRequests", x => x.RequestId);
            //        table.ForeignKey(
            //            name: "FK_RepairRequests_Customers_CustomerId",
            //            column: x => x.CustomerId,
            //            principalTable: "Customers",
            //            principalColumn: "CustomerId");
            //        table.ForeignKey(
            //            name: "FK_RepairRequests_RepairItemCategories_CategoryId",
            //            column: x => x.CategoryId,
            //            principalTable: "RepairItemCategories",
            //            principalColumn: "CategoryId");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Technicians",
            //    columns: table => new
            //    {
            //        TechnicianId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        TechnicianFirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        TechnicianLastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        RegisteredOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Status = table.Column<int>(type: "int", nullable: false),
            //        CategoryId = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Technicians", x => x.TechnicianId);
            //        table.ForeignKey(
            //            name: "FK_Technicians_RepairItemCategories_CategoryId",
            //            column: x => x.CategoryId,
            //            principalTable: "RepairItemCategories",
            //            principalColumn: "CategoryId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AddProducts",
            //    columns: table => new
            //    {
            //        ProdId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        OwnerId = table.Column<int>(type: "int", nullable: false),
            //        ProdName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ProdCategory = table.Column<int>(type: "int", nullable: false),
            //        ProdDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ProdPrice = table.Column<double>(type: "float", nullable: false),
            //        ProdSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ProdQuantity = table.Column<int>(type: "int", nullable: false),
            //        ProdType = table.Column<int>(type: "int", nullable: false),
            //        Status = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AddProducts", x => x.ProdId);
            //        table.ForeignKey(
            //            name: "FK_AddProducts_shopowners_OwnerId",
            //            column: x => x.OwnerId,
            //            principalTable: "shopowners",
            //            principalColumn: "OwnerId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "BusRegCertificates",
            //    columns: table => new
            //    {
            //        CertId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        OwnerId = table.Column<int>(type: "int", nullable: false),
            //        FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        SavedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_BusRegCertificates", x => x.CertId);
            //        table.ForeignKey(
            //            name: "FK_BusRegCertificates_shopowners_OwnerId",
            //            column: x => x.OwnerId,
            //            principalTable: "shopowners",
            //            principalColumn: "OwnerId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Orders",
            //    columns: table => new
            //    {
            //        OrderId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        CustomerId = table.Column<int>(type: "int", nullable: true),
            //        OrderedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        PaymentExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        Status = table.Column<int>(type: "int", nullable: false),
            //        DriverId = table.Column<int>(type: "int", nullable: true),
            //        DeliveryAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        TotalAmount = table.Column<double>(type: "float", nullable: false),
            //        PaymentReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        IsPaid = table.Column<bool>(type: "bit", nullable: false),
            //        DeliveredOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        IsArchived = table.Column<bool>(type: "bit", nullable: false),
            //        OwnerId = table.Column<int>(type: "int", nullable: false),
            //        DeliveryPin = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ScheduledDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        OrderType = table.Column<int>(type: "int", nullable: false),
            //        PickupOrderId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Orders", x => x.OrderId);
            //        table.ForeignKey(
            //            name: "FK_Orders_Customers_CustomerId",
            //            column: x => x.CustomerId,
            //            principalTable: "Customers",
            //            principalColumn: "CustomerId");
            //        table.ForeignKey(
            //            name: "FK_Orders_DeliveryDrivers_DriverId",
            //            column: x => x.DriverId,
            //            principalTable: "DeliveryDrivers",
            //            principalColumn: "DriverId");
            //        table.ForeignKey(
            //            name: "FK_Orders_shopowners_OwnerId",
            //            column: x => x.OwnerId,
            //            principalTable: "shopowners",
            //            principalColumn: "OwnerId");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "OwnerIdentityBooks",
            //    columns: table => new
            //    {
            //        OwnerIdentityBookId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        OwnerId = table.Column<int>(type: "int", nullable: false),
            //        FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        SavedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_OwnerIdentityBooks", x => x.OwnerIdentityBookId);
            //        table.ForeignKey(
            //            name: "FK_OwnerIdentityBooks_shopowners_OwnerId",
            //            column: x => x.OwnerId,
            //            principalTable: "shopowners",
            //            principalColumn: "OwnerId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Staffs",
            //    columns: table => new
            //    {
            //        StaffId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        OwnerId = table.Column<int>(type: "int", nullable: false),
            //        FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
            //        Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        RegisteredOn = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Staffs", x => x.StaffId);
            //        table.ForeignKey(
            //            name: "FK_Staffs_shopowners_OwnerId",
            //            column: x => x.OwnerId,
            //            principalTable: "shopowners",
            //            principalColumn: "OwnerId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ProposalCheckouts",
            //    columns: table => new
            //    {
            //        CheckoutId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        PropItemId = table.Column<int>(type: "int", nullable: false),
            //        CustomerId = table.Column<int>(type: "int", nullable: false),
            //        SelectDelivery = table.Column<int>(type: "int", nullable: false),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        BankCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AccountHolderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        BranchCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AccountType = table.Column<int>(type: "int", nullable: false),
            //        SubmittedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ProposalCheckouts", x => x.CheckoutId);
            //        table.ForeignKey(
            //            name: "FK_ProposalCheckouts_Customers_CustomerId",
            //            column: x => x.CustomerId,
            //            principalTable: "Customers",
            //            principalColumn: "CustomerId");
            //        table.ForeignKey(
            //            name: "FK_ProposalCheckouts_ProposeItems_PropItemId",
            //            column: x => x.PropItemId,
            //            principalTable: "ProposeItems",
            //            principalColumn: "PropItemId");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ProposedProdPic",
            //    columns: table => new
            //    {
            //        ProposedId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        CustomerId = table.Column<int>(type: "int", nullable: false),
            //        FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        SavedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        ProposeItemPropItemId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ProposedProdPic", x => x.ProposedId);
            //        table.ForeignKey(
            //            name: "FK_ProposedProdPic_Customers_CustomerId",
            //            column: x => x.CustomerId,
            //            principalTable: "Customers",
            //            principalColumn: "CustomerId",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_ProposedProdPic_ProposeItems_ProposeItemPropItemId",
            //            column: x => x.ProposeItemPropItemId,
            //            principalTable: "ProposeItems",
            //            principalColumn: "PropItemId");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ReceiptPics",
            //    columns: table => new
            //    {
            //        ReceiptId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        CustomerId = table.Column<int>(type: "int", nullable: false),
            //        FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        SavedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        ProposeItemPropItemId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ReceiptPics", x => x.ReceiptId);
            //        table.ForeignKey(
            //            name: "FK_ReceiptPics_Customers_CustomerId",
            //            column: x => x.CustomerId,
            //            principalTable: "Customers",
            //            principalColumn: "CustomerId",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_ReceiptPics_ProposeItems_ProposeItemPropItemId",
            //            column: x => x.ProposeItemPropItemId,
            //            principalTable: "ProposeItems",
            //            principalColumn: "PropItemId");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "RepairTrackings",
            //    columns: table => new
            //    {
            //        TrackingId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        RepairRequestId = table.Column<int>(type: "int", nullable: false),
            //        Status = table.Column<int>(type: "int", nullable: false),
            //        UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        StatusMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_RepairTrackings", x => x.TrackingId);
            //        table.ForeignKey(
            //            name: "FK_RepairTrackings_RepairRequests_RepairRequestId",
            //            column: x => x.RepairRequestId,
            //            principalTable: "RepairRequests",
            //            principalColumn: "RequestId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Quotations",
            //    columns: table => new
            //    {
            //        QuotationId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        RepairRequestId = table.Column<int>(type: "int", nullable: false),
            //        TechnicianId = table.Column<int>(type: "int", nullable: true),
            //        CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Status = table.Column<int>(type: "int", nullable: false),
            //        TotalCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
            //        TechnicianNotes = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Quotations", x => x.QuotationId);
            //        table.ForeignKey(
            //            name: "FK_Quotations_RepairRequests_RepairRequestId",
            //            column: x => x.RepairRequestId,
            //            principalTable: "RepairRequests",
            //            principalColumn: "RequestId",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_Quotations_Technicians_TechnicianId",
            //            column: x => x.TechnicianId,
            //            principalTable: "Technicians",
            //            principalColumn: "TechnicianId");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "RepairAssignments",
            //    columns: table => new
            //    {
            //        AssignmentId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        RepairRequestId = table.Column<int>(type: "int", nullable: false),
            //        TechnicianId = table.Column<int>(type: "int", nullable: false),
            //        AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        AssignmentNotes = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_RepairAssignments", x => x.AssignmentId);
            //        table.ForeignKey(
            //            name: "FK_RepairAssignments_RepairRequests_RepairRequestId",
            //            column: x => x.RepairRequestId,
            //            principalTable: "RepairRequests",
            //            principalColumn: "RequestId",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_RepairAssignments_Technicians_TechnicianId",
            //            column: x => x.TechnicianId,
            //            principalTable: "Technicians",
            //            principalColumn: "TechnicianId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CartItems",
            //    columns: table => new
            //    {
            //        CartItemId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        CartId = table.Column<int>(type: "int", nullable: false),
            //        ProdId = table.Column<int>(type: "int", nullable: false),
            //        ProdName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ProdQuantity = table.Column<int>(type: "int", nullable: false),
            //        PriceAtTimeAdded = table.Column<double>(type: "float", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CartItems", x => x.CartItemId);
            //        table.ForeignKey(
            //            name: "FK_CartItems_AddProducts_ProdId",
            //            column: x => x.ProdId,
            //            principalTable: "AddProducts",
            //            principalColumn: "ProdId",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_CartItems_Carts_CartId",
            //            column: x => x.CartId,
            //            principalTable: "Carts",
            //            principalColumn: "CartId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ProductPicture",
            //    columns: table => new
            //    {
            //        ProdPictureId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        OwnerId = table.Column<int>(type: "int", nullable: false),
            //        FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        SavedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        AddProductProdId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ProductPicture", x => x.ProdPictureId);
            //        table.ForeignKey(
            //            name: "FK_ProductPicture_AddProducts_AddProductProdId",
            //            column: x => x.AddProductProdId,
            //            principalTable: "AddProducts",
            //            principalColumn: "ProdId");
            //        table.ForeignKey(
            //            name: "FK_ProductPicture_shopowners_OwnerId",
            //            column: x => x.OwnerId,
            //            principalTable: "shopowners",
            //            principalColumn: "OwnerId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "OrderItems",
            //    columns: table => new
            //    {
            //        OrderItemId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        OrderId = table.Column<int>(type: "int", nullable: false),
            //        ProdId = table.Column<int>(type: "int", nullable: false),
            //        ProdName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Quantity = table.Column<int>(type: "int", nullable: false),
            //        PriceAtTimeOfPurchase = table.Column<double>(type: "float", nullable: false),
            //        ReturnPStatus = table.Column<int>(type: "int", nullable: false),
            //        RetunReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        QuantityReturned = table.Column<int>(type: "int", nullable: false),
            //        RefundOrExchange = table.Column<int>(type: "int", nullable: false),
            //        IsAssingedForCollection = table.Column<bool>(type: "bit", nullable: false),
            //        IsAssingedForSecDelivery = table.Column<bool>(type: "bit", nullable: false),
            //        DriverId = table.Column<int>(type: "int", nullable: true),
            //        CollectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        NewDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_OrderItems", x => x.OrderItemId);
            //        table.ForeignKey(
            //            name: "FK_OrderItems_AddProducts_ProdId",
            //            column: x => x.ProdId,
            //            principalTable: "AddProducts",
            //            principalColumn: "ProdId",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_OrderItems_DeliveryDrivers_DriverId",
            //            column: x => x.DriverId,
            //            principalTable: "DeliveryDrivers",
            //            principalColumn: "DriverId");
            //        table.ForeignKey(
            //            name: "FK_OrderItems_Orders_OrderId",
            //            column: x => x.OrderId,
            //            principalTable: "Orders",
            //            principalColumn: "OrderId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "PickupOrders",
            //    columns: table => new
            //    {
            //        PickupOrderId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        OrderId = table.Column<int>(type: "int", nullable: false),
            //        PickupBranchId = table.Column<int>(type: "int", nullable: false),
            //        PickupReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PickupFee = table.Column<double>(type: "float", nullable: false),
            //        Status = table.Column<int>(type: "int", nullable: false),
            //        CollectedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        ScheduledPickupDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_PickupOrders", x => x.PickupOrderId);
            //        table.ForeignKey(
            //            name: "FK_PickupOrders_Orders_OrderId",
            //            column: x => x.OrderId,
            //            principalTable: "Orders",
            //            principalColumn: "OrderId");
            //        table.ForeignKey(
            //            name: "FK_PickupOrders_shopowners_PickupBranchId",
            //            column: x => x.PickupBranchId,
            //            principalTable: "shopowners",
            //            principalColumn: "OwnerId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "QuotationItems",
            //    columns: table => new
            //    {
            //        QuotationItemId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        QuotationId = table.Column<int>(type: "int", nullable: false),
            //        ItemDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        UnitCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
            //        Quantity = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_QuotationItems", x => x.QuotationItemId);
            //        table.ForeignKey(
            //            name: "FK_QuotationItems_Quotations_QuotationId",
            //            column: x => x.QuotationId,
            //            principalTable: "Quotations",
            //            principalColumn: "QuotationId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Receipts",
            //    columns: table => new
            //    {
            //        ReceiptId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        RepairRequestId = table.Column<int>(type: "int", nullable: false),
            //        QuotationId = table.Column<int>(type: "int", nullable: true),
            //        TechnicianId = table.Column<int>(type: "int", nullable: true),
            //        IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        FinalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
            //        ReceiptNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Receipts", x => x.ReceiptId);
            //        table.ForeignKey(
            //            name: "FK_Receipts_Quotations_QuotationId",
            //            column: x => x.QuotationId,
            //            principalTable: "Quotations",
            //            principalColumn: "QuotationId");
            //        table.ForeignKey(
            //            name: "FK_Receipts_RepairRequests_RepairRequestId",
            //            column: x => x.RepairRequestId,
            //            principalTable: "RepairRequests",
            //            principalColumn: "RequestId",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_Receipts_Technicians_TechnicianId",
            //            column: x => x.TechnicianId,
            //            principalTable: "Technicians",
            //            principalColumn: "TechnicianId");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "InterBranchTransfers",
            //    columns: table => new
            //    {
            //        TransferId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        OrderItemId = table.Column<int>(type: "int", nullable: false),
            //        SourceBranchId = table.Column<int>(type: "int", nullable: false),
            //        TargetBranchId = table.Column<int>(type: "int", nullable: false),
            //        DriverId = table.Column<int>(type: "int", nullable: true),
            //        TransferCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        TransferStatus = table.Column<int>(type: "int", nullable: false),
            //        ScheduledTransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        DeliveredToTargetOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        IsDelivered = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_InterBranchTransfers", x => x.TransferId);
            //        table.ForeignKey(
            //            name: "FK_InterBranchTransfers_DeliveryDrivers_DriverId",
            //            column: x => x.DriverId,
            //            principalTable: "DeliveryDrivers",
            //            principalColumn: "DriverId");
            //        table.ForeignKey(
            //            name: "FK_InterBranchTransfers_OrderItems_OrderItemId",
            //            column: x => x.OrderItemId,
            //            principalTable: "OrderItems",
            //            principalColumn: "OrderItemId");
            //        table.ForeignKey(
            //            name: "FK_InterBranchTransfers_shopowners_SourceBranchId",
            //            column: x => x.SourceBranchId,
            //            principalTable: "shopowners",
            //            principalColumn: "OwnerId");
            //        table.ForeignKey(
            //            name: "FK_InterBranchTransfers_shopowners_TargetBranchId",
            //            column: x => x.TargetBranchId,
            //            principalTable: "shopowners",
            //            principalColumn: "OwnerId");
            //    });

        //    migrationBuilder.CreateIndex(
        //        name: "IX_AddProducts_OwnerId",
        //        table: "AddProducts",
        //        column: "OwnerId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_BusRegCertificates_OwnerId",
        //        table: "BusRegCertificates",
        //        column: "OwnerId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_CartItems_CartId",
        //        table: "CartItems",
        //        column: "CartId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_CartItems_ProdId",
        //        table: "CartItems",
        //        column: "ProdId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Carts_CustomerId",
        //        table: "Carts",
        //        column: "CustomerId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_DriverIdentityBooks_DriverId",
        //        table: "DriverIdentityBooks",
        //        column: "DriverId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_DriversLicences_DriverId",
        //        table: "DriversLicences",
        //        column: "DriverId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_InterBranchTransfers_DriverId",
        //        table: "InterBranchTransfers",
        //        column: "DriverId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_InterBranchTransfers_OrderItemId",
        //        table: "InterBranchTransfers",
        //        column: "OrderItemId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_InterBranchTransfers_SourceBranchId",
        //        table: "InterBranchTransfers",
        //        column: "SourceBranchId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_InterBranchTransfers_TargetBranchId",
        //        table: "InterBranchTransfers",
        //        column: "TargetBranchId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_OrderItems_DriverId",
        //        table: "OrderItems",
        //        column: "DriverId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_OrderItems_OrderId",
        //        table: "OrderItems",
        //        column: "OrderId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_OrderItems_ProdId",
        //        table: "OrderItems",
        //        column: "ProdId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Orders_CustomerId",
        //        table: "Orders",
        //        column: "CustomerId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Orders_DriverId",
        //        table: "Orders",
        //        column: "DriverId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Orders_OwnerId",
        //        table: "Orders",
        //        column: "OwnerId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_OwnerIdentityBooks_OwnerId",
        //        table: "OwnerIdentityBooks",
        //        column: "OwnerId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_PickupOrders_OrderId",
        //        table: "PickupOrders",
        //        column: "OrderId",
        //        unique: true);

        //    migrationBuilder.CreateIndex(
        //        name: "IX_PickupOrders_PickupBranchId",
        //        table: "PickupOrders",
        //        column: "PickupBranchId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_ProductPicture_AddProductProdId",
        //        table: "ProductPicture",
        //        column: "AddProductProdId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_ProductPicture_OwnerId",
        //        table: "ProductPicture",
        //        column: "OwnerId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_ProposalCheckouts_CustomerId",
        //        table: "ProposalCheckouts",
        //        column: "CustomerId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_ProposalCheckouts_PropItemId",
        //        table: "ProposalCheckouts",
        //        column: "PropItemId",
        //        unique: true);

        //    migrationBuilder.CreateIndex(
        //        name: "IX_ProposedProdPic_CustomerId",
        //        table: "ProposedProdPic",
        //        column: "CustomerId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_ProposedProdPic_ProposeItemPropItemId",
        //        table: "ProposedProdPic",
        //        column: "ProposeItemPropItemId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_ProposeItems_AssignedDriverId",
        //        table: "ProposeItems",
        //        column: "AssignedDriverId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_ProposeItems_CustomerId",
        //        table: "ProposeItems",
        //        column: "CustomerId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_QuotationItems_QuotationId",
        //        table: "QuotationItems",
        //        column: "QuotationId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Quotations_RepairRequestId",
        //        table: "Quotations",
        //        column: "RepairRequestId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Quotations_TechnicianId",
        //        table: "Quotations",
        //        column: "TechnicianId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_ReceiptPics_CustomerId",
        //        table: "ReceiptPics",
        //        column: "CustomerId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_ReceiptPics_ProposeItemPropItemId",
        //        table: "ReceiptPics",
        //        column: "ProposeItemPropItemId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Receipts_QuotationId",
        //        table: "Receipts",
        //        column: "QuotationId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Receipts_RepairRequestId",
        //        table: "Receipts",
        //        column: "RepairRequestId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Receipts_TechnicianId",
        //        table: "Receipts",
        //        column: "TechnicianId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_RepairAssignments_RepairRequestId",
        //        table: "RepairAssignments",
        //        column: "RepairRequestId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_RepairAssignments_TechnicianId",
        //        table: "RepairAssignments",
        //        column: "TechnicianId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_RepairRequests_CategoryId",
        //        table: "RepairRequests",
        //        column: "CategoryId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_RepairRequests_CustomerId",
        //        table: "RepairRequests",
        //        column: "CustomerId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_RepairTrackings_RepairRequestId",
        //        table: "RepairTrackings",
        //        column: "RepairRequestId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Staffs_OwnerId",
        //        table: "Staffs",
        //        column: "OwnerId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Technicians_CategoryId",
        //        table: "Technicians",
        //        column: "CategoryId");
        //}

        ///// <inheritdoc />
        //protected override void Down(MigrationBuilder migrationBuilder)
        //{
        //    migrationBuilder.DropTable(
        //        name: "BusRegCertificates");

        //    migrationBuilder.DropTable(
        //        name: "CartItems");

        //    migrationBuilder.DropTable(
        //        name: "DriverIdentityBooks");

        //    migrationBuilder.DropTable(
        //        name: "DriversLicences");

        //    migrationBuilder.DropTable(
        //        name: "InterBranchTransfers");

        //    migrationBuilder.DropTable(
        //        name: "OwnerIdentityBooks");

        //    migrationBuilder.DropTable(
        //        name: "PickupOrders");

        //    migrationBuilder.DropTable(
        //        name: "ProductPicture");

        //    migrationBuilder.DropTable(
        //        name: "ProposalCheckouts");

        //    migrationBuilder.DropTable(
        //        name: "ProposedProdPic");

        //    migrationBuilder.DropTable(
        //        name: "QuotationItems");

        //    migrationBuilder.DropTable(
        //        name: "ReceiptPics");

        //    migrationBuilder.DropTable(
        //        name: "Receipts");

        //    migrationBuilder.DropTable(
        //        name: "RepairAssignments");

        //    migrationBuilder.DropTable(
        //        name: "RepairTrackings");

        //    migrationBuilder.DropTable(
        //        name: "Staffs");

        //    migrationBuilder.DropTable(
        //        name: "SystemAdmins");

        //    migrationBuilder.DropTable(
        //        name: "Carts");

        //    migrationBuilder.DropTable(
        //        name: "OrderItems");

        //    migrationBuilder.DropTable(
        //        name: "ProposeItems");

        //    migrationBuilder.DropTable(
        //        name: "Quotations");

        //    migrationBuilder.DropTable(
        //        name: "AddProducts");

        //    migrationBuilder.DropTable(
        //        name: "Orders");

        //    migrationBuilder.DropTable(
        //        name: "RepairRequests");

        //    migrationBuilder.DropTable(
        //        name: "Technicians");

        //    migrationBuilder.DropTable(
        //        name: "DeliveryDrivers");

        //    migrationBuilder.DropTable(
        //        name: "shopowners");

        //    migrationBuilder.DropTable(
        //        name: "Customers");

        //    migrationBuilder.DropTable(
        //        name: "RepairItemCategories");
        }
    }
}

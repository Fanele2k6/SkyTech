using CM_Project.Models.Documents;
using Microsoft.EntityFrameworkCore;
using SkyTech.Models;


namespace CM_Project.Models
{
    public class ApplicationDbContext : DbContext
    {
        // EF Core: receives DbContextOptions via constructor injection
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // ── User tables ──────────────────────────────────────
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<ShopOwner> shopowners { get; set; }
        public DbSet<SystemAdmin> SystemAdmins { get; set; }
        public DbSet<DeliveryDriver> DeliveryDrivers { get; set; }

        // ── Document tables ──────────────────────────────────
        public DbSet<BusRegCertificate> BusRegCertificates { get; set; }
        public DbSet<DriverIdentityBook> DriverIdentityBooks { get; set; }
        public DbSet<OwnerIdentityBook> OwnerIdentityBooks { get; set; }
        public DbSet<DriversLicence> DriversLicences { get; set; }

        // ── Product / order tables ───────────────────────────
        public DbSet<AddProduct> AddProducts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // ── Cart tables ──────────────────────────────────────
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        //public DbSet<Return> Return { get; set; }
        //public DbSet<ReturnItem> ReturnItem { get; set; }


        //New tables for pickup
        public DbSet<PickupOrder> PickupOrders { get; set; }
        public DbSet<InterBranchTransfer> InterBranchTransfers { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DbSet<ProposeItem> ProposeItems { get; set; }
        public DbSet<AcceptOffer> AcceptOffers { get; set; }
        public DbSet<ReceiptPic> ReceiptPics { get; set; }
        public DbSet<ProposedProdPic> ProposedProdPic { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Order → OrderItems (cascade is fine)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Customer → Orders (no cascade)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customers)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);  // ← stops the cycle

            // ========== FIXED: PickupOrder configuration (NO CASCADE) ==========
            modelBuilder.Entity<PickupOrder>()
                .HasOne(p => p.Order)
                .WithOne()
                .HasForeignKey<PickupOrder>(p => p.OrderId)
                .OnDelete(DeleteBehavior.NoAction);  // ← FIXED: NoAction instead of Cascade

            // ========== FIXED: InterBranchTransfer configuration ==========
            modelBuilder.Entity<InterBranchTransfer>()
                .HasOne(t => t.OrderItem)
                .WithMany()
                .HasForeignKey(t => t.OrderItemId)
                .OnDelete(DeleteBehavior.NoAction);  // ← FIXED: NoAction

            modelBuilder.Entity<InterBranchTransfer>()
                .HasOne(t => t.SourceBranch)
                .WithMany()
                .HasForeignKey(t => t.SourceBranchId)
                .OnDelete(DeleteBehavior.NoAction);  // ← FIXED: NoAction

            modelBuilder.Entity<InterBranchTransfer>()
                .HasOne(t => t.TargetBranch)
                .WithMany()
                .HasForeignKey(t => t.TargetBranchId)
                .OnDelete(DeleteBehavior.NoAction);  // ← FIXED: NoAction

            modelBuilder.Entity<InterBranchTransfer>()
                .HasOne(t => t.Driver)
                .WithMany()
                .HasForeignKey(t => t.DriverId)
                // ← FIXED: NoAction
                .OnDelete(DeleteBehavior.NoAction);

            // ShopOwner → Orders (no cascade)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.ShopOwner)
                .WithMany()
                .HasForeignKey(o => o.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.DeliveryDriver)
            .WithMany()
            .HasForeignKey(i => i.DriverId)
            .OnDelete(DeleteBehavior.NoAction);
        }

    }
}

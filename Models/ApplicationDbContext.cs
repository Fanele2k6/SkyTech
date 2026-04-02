using CM_Project.Models.Documents;
using Microsoft.EntityFrameworkCore;


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



        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    // Keep cascade on OrderItems (deleting Order also deletes its items) ✅
        //    modelBuilder.Entity<OrderItem>()
        //        .HasOne(oi => oi.Order)
        //        .WithMany(o => o.OrderItems)
        //        .HasForeignKey(oi => oi.OrderId)
        //        .OnDelete(DeleteBehavior.Cascade);  // ← delete order = delete its items

        //    // Break the cycle HERE instead — on the Customer → Order path
        //    modelBuilder.Entity<Order>()
        //        .HasOne(o => o.Customers)
        //        .WithMany()
        //        .HasForeignKey(o => o.CustomerId)
        //        .OnDelete(DeleteBehavior.NoAction);  // ← stops the cycle
        //}
    }
}

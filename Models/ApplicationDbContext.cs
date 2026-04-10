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
        //public DbSet<Return> Return { get; set; }
        //public DbSet<ReturnItem> ReturnItem { get; set; }



        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    // ── Fix cascade cycle ─────────────────────────────────────────

        //    // Keep cascade: deleting Order also deletes its OrderItems ✅
        //    modelBuilder.Entity<OrderItem>()
        //        .HasOne(oi => oi.Order)
        //        .WithMany(o => o.OrderItems)
        //        .HasForeignKey(oi => oi.OrderId)
        //        .OnDelete(DeleteBehavior.Cascade);

        //    // Break the cycle here: Customer → Order (no auto delete)
        //    modelBuilder.Entity<Order>()
        //        .HasOne(o => o.Customers)
        //        .WithMany()
        //        .HasForeignKey(o => o.CustomerId)
        //        .OnDelete(DeleteBehavior.NoAction);

        //    // Also break: ShopOwner → Order (you added OwnerId FK)
        //    modelBuilder.Entity<Order>()
        //        .HasOne(o => o.ShopOwner)
        //        .WithMany()
        //        .HasForeignKey(o => o.OwnerId)
        //        .OnDelete(DeleteBehavior.NoAction);

        //    // ── Seed admin to database
        //    modelBuilder.Entity<SystemAdmin>().HasData(new SystemAdmin
        //    {
        //        AdminId = 1,
        //        FirstName = "Nhlakanipho",
        //        LastName = "Dladla",
        //        Email = "admin@skytech.com",
        //        Password = "Fanele.23",
        //        UserType = "SystemAdmin",
        //        RegisteredOn = new DateTime(2024, 1, 1)
        //    });
        //}

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

            //modelBuilder.Entity<OrderItem>()
            //    .HasOne(i => i.ExchangeDriver)
            //    .WithMany()
            //    .HasForeignKey(i => i.ExchangeDriverId)
            //    .OnDelete(DeleteBehavior.NoAction);
        }

    }
}

using Microsoft.EntityFrameworkCore;
using SkyTech.Models;
using SkyTech.Models.Documents;
using SkyTech.Models.JobPostServices;
using SkyTech.Models.Repair_Item_Services;
using SkyTech.Models.Repair_Services;
using SkyTech.Models.SkytechWorkers;


namespace SkyTech.Models
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
       
        public DbSet<ReceiptPic> ReceiptPics { get; set; }
        public DbSet<ProposedProdPic> ProposedProdPic { get; set; }
        public DbSet<ProposalCheckout> ProposalCheckouts { get; set; }


        // ── Repair Items Services tables ──────────────────────────────────────

        public DbSet<Repair_Item_Category> RepairItemCategories { get; set; }
        public DbSet<Repair_Request> RepairRequests { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<RepairAssignment> RepairAssignments { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationItem> QuotationItems { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<RepairTracking> RepairTrackings { get; set; }

        //--------POST TABLE----
        public DbSet<JobPost> JobPosts { get; set; }
        public DbSet<JobRoleKnowledge> JobRoleKnowledges { get; set; }
        public DbSet<ApplicationForm> ApplicationForms { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<StaffPosition> StaffPositions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<HR> HRs { get; set; }

        //========ROSTER TABLES=====
        public DbSet<Roster> Rosters { get; set; }
        public DbSet<RosterShift> RosterShifts { get; set; }
        public DbSet<StaffRotationOrder> StaffRotationOrders { get; set; }


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

            // Fix cascade conflict on ProposeItems → Customer
            modelBuilder.Entity<ProposeItem>()
                .HasOne(p => p.Customer)
                .WithMany(c => c.ProposeItems)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.NoAction); // ← fixes the error

            // Fix cascade conflict on ProposalCheckout → ProposeItem
            modelBuilder.Entity<ProposalCheckout>()
                .HasOne(c => c.ProposeItem)
                .WithOne(p => p.Checkout)
                .HasForeignKey<ProposalCheckout>(c => c.PropItemId)
                .OnDelete(DeleteBehavior.NoAction);

            // Fix cascade conflict on ProposalCheckout → Customer
            modelBuilder.Entity<ProposalCheckout>()
                .HasOne(c => c.Customer)
                .WithMany()
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RosterShift>()
                .HasOne(rs => rs.Staff).WithMany()
                .HasForeignKey(rs => rs.StaffId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RosterShift>()
                .HasOne(rs => rs.Position).WithMany()
                .HasForeignKey(rs => rs.PositionId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RosterShift>()
                .HasOne(rs => rs.Branch).WithMany()
                .HasForeignKey(rs => rs.BranchId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Roster>()
                .HasOne(r => r.Branch).WithMany()
                .HasForeignKey(r => r.BranchId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StaffRotationOrder>()
                .HasOne(o => o.Staff).WithMany()
                .HasForeignKey(o => o.StaffId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StaffRotationOrder>()
                .HasOne(o => o.Branch).WithMany()
                .HasForeignKey(o => o.BranchId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StaffRotationOrder>()
                .HasOne(o => o.Position).WithMany()
                .HasForeignKey(o => o.PositionId).OnDelete(DeleteBehavior.Restrict);
        }


    }
}

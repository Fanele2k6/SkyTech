using Microsoft.EntityFrameworkCore;
using SkyTech.Models;
using SkyTech.Models.Documents;
using SkyTech.Models.JobPostServices;
using SkyTech.Models.JobPostServices.By_TRENCH;
using SkyTech.Models.Repair_Item_Services;
using SkyTech.Models.Repair_Services;


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


        // ── Org structure ────────────────────────────────────────
        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }

        // ── Leave tracking ───────────────────────────────────────
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        // ── Staffing analysis (UC1) ───────────────────────────
        public DbSet<StaffingAnalysisSchedule> StaffingAnalysisSchedules { get; set; }
        public DbSet<StaffingAnalysisRun> StaffingAnalysisRuns { get; set; }
        public DbSet<DepartmentStaffingSnapshot> DepartmentStaffingSnapshots { get; set; }
        public DbSet<JobVacancyRecommendation> JobVacancyRecommendations { get; set; }
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


            // Position → Department
            modelBuilder.Entity<Position>()
                .HasOne(p => p.Department)
                .WithMany(d => d.Positions)
                .HasForeignKey(p => p.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Position → Staff (HR audit: who set MaxHeadcount)
            modelBuilder.Entity<Position>()
                .HasOne(p => p.MaxHeadcountSetByStaff)
                .WithMany()
                .HasForeignKey(p => p.MaxHeadcountSetByStaffId)
                .OnDelete(DeleteBehavior.NoAction);

            // Staff → Department
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.Department)
                .WithMany(d => d.Staffs)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Staff → Position
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.Position)
                .WithMany(p => p.Staffs)
                .HasForeignKey(s => s.PositionId)
                .OnDelete(DeleteBehavior.NoAction);

            // LeaveRequest → Staff (applicant)
            modelBuilder.Entity<LeaveRequest>()
                .HasOne(l => l.Staff)
                .WithMany(s => s.LeaveRequests)
                .HasForeignKey(l => l.StaffId)
                .OnDelete(DeleteBehavior.NoAction);

            // LeaveRequest → Staff (approver)
            modelBuilder.Entity<LeaveRequest>()
                .HasOne(l => l.ApprovedByStaff)
                .WithMany()
                .HasForeignKey(l => l.ApprovedByStaffId)
                .OnDelete(DeleteBehavior.NoAction);

            // LeaveRequest → LeaveType
            modelBuilder.Entity<LeaveRequest>()
                .HasOne(l => l.LeaveType)
                .WithMany(t => t.LeaveRequests)
                .HasForeignKey(l => l.LeaveTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            // StaffingAnalysisSchedule → SystemAdmin
            modelBuilder.Entity<StaffingAnalysisSchedule>()
                .HasOne(s => s.SystemAdmin)
                .WithMany()
                .HasForeignKey(s => s.SystemAdminId)
                .OnDelete(DeleteBehavior.NoAction);

            // StaffingAnalysisRun → StaffingAnalysisSchedule
            modelBuilder.Entity<StaffingAnalysisRun>()
                .HasOne(r => r.Schedule)
                .WithMany(s => s.Runs)
                .HasForeignKey(r => r.ScheduleId)
                .OnDelete(DeleteBehavior.NoAction);

            // StaffingAnalysisRun → SystemAdmin (manual trigger)
            modelBuilder.Entity<StaffingAnalysisRun>()
                .HasOne(r => r.TriggeredByAdmin)
                .WithMany()
                .HasForeignKey(r => r.TriggeredByAdminId)
                .OnDelete(DeleteBehavior.NoAction);

            // DepartmentStaffingSnapshot → StaffingAnalysisRun (CAN cascade)
            modelBuilder.Entity<DepartmentStaffingSnapshot>()
                .HasOne(sn => sn.AnalysisRun)
                .WithMany(r => r.Snapshots)
                .HasForeignKey(sn => sn.AnalysisRunId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DepartmentStaffingSnapshot>()
                .HasOne(sn => sn.Department)
                .WithMany()
                .HasForeignKey(sn => sn.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DepartmentStaffingSnapshot>()
                .HasOne(sn => sn.Position)
                .WithMany()
                .HasForeignKey(sn => sn.PositionId)
                .OnDelete(DeleteBehavior.NoAction);

            // JobVacancyRecommendation → StaffingAnalysisRun
            modelBuilder.Entity<JobVacancyRecommendation>()
                .HasOne(rec => rec.AnalysisRun)
                .WithMany(r => r.Recommendations)
                .HasForeignKey(rec => rec.AnalysisRunId)
                .OnDelete(DeleteBehavior.NoAction);

            // JobVacancyRecommendation → DepartmentStaffingSnapshot
            modelBuilder.Entity<JobVacancyRecommendation>()
                .HasOne(rec => rec.Snapshot)
                .WithMany(sn => sn.Recommendations)
                .HasForeignKey(rec => rec.SnapshotId)
                .OnDelete(DeleteBehavior.NoAction);

            // JobVacancyRecommendation → Department
            modelBuilder.Entity<JobVacancyRecommendation>()
                .HasOne(rec => rec.Department)
                .WithMany()
                .HasForeignKey(rec => rec.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction);

            // JobVacancyRecommendation → Position
            modelBuilder.Entity<JobVacancyRecommendation>()
                .HasOne(rec => rec.Position)
                .WithMany()
                .HasForeignKey(rec => rec.PositionId)
                .OnDelete(DeleteBehavior.NoAction);

            // JobVacancyRecommendation → LeaveRequest
            modelBuilder.Entity<JobVacancyRecommendation>()
                .HasOne(rec => rec.LeaveRequest)
                .WithMany(l => l.TemporaryCoverRecommendations)
                .HasForeignKey(rec => rec.LeaveRequestId)
                .OnDelete(DeleteBehavior.NoAction);

            // JobVacancyRecommendation → Staff (HR reviewer)
            modelBuilder.Entity<JobVacancyRecommendation>()
                .HasOne(rec => rec.ReviewedByStaff)
                .WithMany()
                .HasForeignKey(rec => rec.ReviewedByStaffId)
                .OnDelete(DeleteBehavior.NoAction);

            // JobRoleKnowledge → Position
            modelBuilder.Entity<JobRoleKnowledge>()
                .HasOne(jrk => jrk.Position)
                .WithMany(p => p.JobRoleKnowledges)
                .HasForeignKey(jrk => jrk.PositionId)
                .OnDelete(DeleteBehavior.NoAction);

        }


    }
}

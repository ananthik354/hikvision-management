using HikvisionBackend1.Models;
using Microsoft.EntityFrameworkCore;

namespace HikvisionBackend1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }

        public DbSet<Camera> Cameras { get; set; }

        public DbSet<VehicleDetection> VehicleDetections { get; set; }

        public DbSet<AuthorizedVehicle> AuthorizedVehicles { get; set; }

        public DbSet<RegisteredVehicle> RegisteredVehicles { get; set; }

        public DbSet<VehicleHistory> VehicleHistory { get; set; }

        public DbSet<RelayAction> RelayActions { get; set; }


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // ==========================================
            // TABLE NAMES
            // ==========================================

            modelBuilder.Entity<UserModel>()
                .ToTable("Users");

            modelBuilder.Entity<Camera>()
                .ToTable("Camera");

            modelBuilder.Entity<VehicleDetection>()
                .ToTable("VehicleDetections");

            modelBuilder.Entity<AuthorizedVehicle>()
                .ToTable("AuthorizedVehicles");

            modelBuilder.Entity<RegisteredVehicle>()
                .ToTable("RegisteredVehicles");

            modelBuilder.Entity<VehicleHistory>()
                .ToTable("VehicleHistory");

            modelBuilder.Entity<RelayAction>()
                .ToTable("RelayActions");


            // ==========================================
            // Camera → VehicleDetection
            // ==========================================

            modelBuilder.Entity<VehicleDetection>()
                .HasOne(v => v.Camera)
                .WithMany(c => c.VehicleDetections)
                .HasForeignKey(v => v.CameraId)
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // Camera → RegisteredVehicle
            // ==========================================

            modelBuilder.Entity<RegisteredVehicle>()
                .HasOne(v => v.Camera)
                .WithMany()
                .HasForeignKey(v => v.CameraId)
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // RegisteredVehicle → VehicleHistory
            // ==========================================

            modelBuilder.Entity<VehicleHistory>()
                .HasOne(h => h.RegisteredVehicle)
                .WithMany()
                .HasForeignKey(h => h.RegisteredVehicleId)
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // VehicleDetection → VehicleHistory
            // ==========================================

            modelBuilder.Entity<VehicleHistory>()
                .HasOne(h => h.VehicleDetection)
                .WithMany()
                .HasForeignKey(h => h.VehicleDetectionId)
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // VehicleDetection → RelayAction
            // ==========================================

            modelBuilder.Entity<RelayAction>()
                .HasOne(r => r.VehicleDetection)
                .WithMany()
                .HasForeignKey(r => r.VehicleDetectionId)
                .OnDelete(DeleteBehavior.Cascade);
            // ==========================================
            // Camera → RelayAction
            // ==========================================

            modelBuilder.Entity<RelayAction>()
                .HasOne(r => r.Camera)
                .WithMany()
                .HasForeignKey(r => r.CameraId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
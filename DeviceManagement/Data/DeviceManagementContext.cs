using Microsoft.EntityFrameworkCore;
using DeviceManagement.Models;

namespace DeviceManagement.Data
{
    /// <summary>
    /// Entity Framework Core database context for the Device Management application.
    /// Manages database sets and maps POCO entities to SQL Server tables.
    /// </summary>
    public class DeviceManagementContext : DbContext
    {
        public DeviceManagementContext(DbContextOptions<DeviceManagementContext> options)
            : base(options)
        {
        }

        public DbSet<DeviceCategory> Categories => Set<DeviceCategory>();
        public DbSet<Device> Devices => Set<Device>();
        public DbSet<Employee> Employees => Set<Employee>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // DeviceCategory configuration
            modelBuilder.Entity<DeviceCategory>(entity =>
            {
                entity.ToTable("DeviceCategories");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Description).HasMaxLength(500);

                // Prevent duplicate category names
                entity.HasIndex(c => c.Name).IsUnique();
            });

            // Device configuration
            modelBuilder.Entity<Device>(entity =>
            {
                entity.ToTable("Devices");
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
                entity.Property(d => d.DeviceCode).IsRequired().HasMaxLength(30);
                entity.Property(d => d.Status).IsRequired();
                entity.Property(d => d.DateOfEntry).HasColumnType("date").IsRequired();

                // Prevent duplicate device codes
                entity.HasIndex(d => d.DeviceCode).IsUnique();

                // One Category to Many Devices relationship
                // DeleteBehavior.Restrict prevents deleting a category that still contains active devices
                entity.HasOne(d => d.DeviceCategory)
                      .WithMany(c => c.Devices)
                      .HasForeignKey(d => d.DeviceCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Employee configuration
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);

                // Prevent duplicate employee email addresses
                entity.HasIndex(e => e.Email).IsUnique();
            });
        }
    }
}

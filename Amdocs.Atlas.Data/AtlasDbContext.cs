using System;
using Amdocs.Atlas.Core.Entities;
using Microsoft.EntityFrameworkCore;

// Alias to avoid confusion with System.Environment
using EnvironmentEntity = Amdocs.Atlas.Core.Entities.Environment;

namespace Amdocs.Atlas.Data
{
    public class AtlasDbContext : DbContext
    {
        public AtlasDbContext(DbContextOptions<AtlasDbContext> options)
            : base(options)
        {
        }

        public DbSet<EnvironmentEntity> Environments => Set<EnvironmentEntity>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Vcenter> Vcenters => Set<Vcenter>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Owner> Owners => Set<Owner>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Server> Servers => Set<Server>();
        public DbSet<ServerTag> ServerTags => Set<ServerTag>();
        public DbSet<HealthCheck> HealthChecks => Set<HealthCheck>();
        public DbSet<ChangeLog> ChangeLogs => Set<ChangeLog>();
        public DbSet<Note> Notes => Set<Note>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Indexes
            modelBuilder.Entity<Server>()
                .HasIndex(s => new { s.Hostname, s.EnvironmentId })
                .IsUnique(false);

            modelBuilder.Entity<ServerTag>()
                .HasIndex(st => new { st.ServerId, st.Tag })
                .IsUnique();

            modelBuilder.Entity<HealthCheck>()
                .HasIndex(h => new { h.ServerId, h.CheckTime });

            // Relationships with explicit generic types

            modelBuilder.Entity<Server>()
                .HasOne<EnvironmentEntity>(s => s.Environment)
                .WithMany(e => e.Servers)
                .HasForeignKey(s => s.EnvironmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Server>()
                .HasOne<Role>(s => s.Role)
                .WithMany(r => r.Servers)
                .HasForeignKey(s => s.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Server>()
                .HasOne<Owner>(s => s.Owner)
                .WithMany(o => o.Servers)
                .HasForeignKey(s => s.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Server>()
                .HasOne<Location>(s => s.Location)
                .WithMany(l => l.Servers)
                .HasForeignKey(s => s.LocationId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Server>()
                .HasOne<Customer>(s => s.Customer)
                .WithMany(c => c.Servers)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Server>()
                .HasOne<Vcenter>(s => s.Vcenter)
                .WithMany(v => v.Servers)
                .HasForeignKey(s => s.VcenterId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Server>()
                .HasOne<Project>(s => s.Project)
                .WithMany(p => p.Servers)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

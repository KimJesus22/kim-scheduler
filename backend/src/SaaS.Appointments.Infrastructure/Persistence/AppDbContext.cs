using Microsoft.EntityFrameworkCore;
using SaaS.Appointments.Domain.Entities;

namespace SaaS.Appointments.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Business> Businesses => Set<Business>();

    public DbSet<Service> Services => Set<Service>();

    public DbSet<BusinessHour> BusinessHours => Set<BusinessHour>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureBusiness(modelBuilder);
        ConfigureService(modelBuilder);
        ConfigureBusinessHour(modelBuilder);
        ConfigureAppointment(modelBuilder);
    }

    private static void ConfigureBusiness(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Business>(entity =>
        {
            entity.ToTable("businesses");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(180);

            entity.HasIndex(x => x.Slug)
                .IsUnique();

            entity.Property(x => x.Phone)
                .HasMaxLength(30);

            entity.Property(x => x.Email)
                .HasMaxLength(180);

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();
        });
    }

    private static void ConfigureService(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Service>(entity =>
        {
            entity.ToTable("services");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.DurationMinutes)
                .IsRequired();

            entity.Property(x => x.Price)
                .HasPrecision(10, 2);

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.HasOne(x => x.Business)
                .WithMany(x => x.Services)
                .HasForeignKey(x => x.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureBusinessHour(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BusinessHour>(entity =>
        {
            entity.ToTable("business_hours");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DayOfWeek)
                .IsRequired();

            entity.Property(x => x.OpenTime)
                .IsRequired();

            entity.Property(x => x.CloseTime)
                .IsRequired();

            entity.Property(x => x.IsClosed)
                .IsRequired();

            entity.HasOne(x => x.Business)
                .WithMany(x => x.BusinessHours)
                .HasForeignKey(x => x.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAppointment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("appointments");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CustomerName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.CustomerPhone)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(x => x.CustomerEmail)
                .HasMaxLength(180);

            entity.Property(x => x.StartAtUtc)
                .IsRequired();

            entity.Property(x => x.EndAtUtc)
                .IsRequired();

            entity.Property(x => x.Status)
                .IsRequired();

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.HasOne(x => x.Business)
                .WithMany(x => x.Appointments)
                .HasForeignKey(x => x.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Service)
                .WithMany(x => x.Appointments)
                .HasForeignKey(x => x.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.BusinessId,
                x.StartAtUtc
            });
        });
    }
}
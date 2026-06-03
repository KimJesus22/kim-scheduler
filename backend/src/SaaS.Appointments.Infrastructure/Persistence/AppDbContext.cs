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

    // Representa la tabla de usuarios del sistema.
    // Aquí vivirán administradores y staff que podrán iniciar sesión.
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureBusiness(modelBuilder);
        ConfigureService(modelBuilder);
        ConfigureBusinessHour(modelBuilder);
        ConfigureAppointment(modelBuilder);

        // Configura la tabla users y sus reglas en MariaDB.
        ConfigureUser(modelBuilder);
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

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            // Nombre real de la tabla en MariaDB.
            entity.ToTable("users");

            // Llave primaria de la tabla.
            entity.HasKey(x => x.Id);

            // El nombre completo es obligatorio para identificar al usuario en el panel.
            entity.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(150);

            // El email será usado para login, por eso es obligatorio.
            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(180);

            // Evita que existan dos usuarios con el mismo email.
            // Esto es importante porque el email será el identificador de login.
            entity.HasIndex(x => x.Email)
                .IsUnique();

            // Aquí guardamos el hash, nunca la contraseña en texto plano.
            entity.Property(x => x.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            // Guardamos el rol como número:
            // Admin = 1, Staff = 2.
            entity.Property(x => x.Role)
                .IsRequired();

            // Permite desactivar usuarios sin borrarlos físicamente.
            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            // Relación opcional:
            // un usuario puede pertenecer a un negocio,
            // pero también puede existir sin negocio asignado todavía.
            entity.HasOne(x => x.Business)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.BusinessId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
using LitackaApi.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LitackaApi.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext(options)
{
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<CardStatus> CardStatuses => Set<CardStatus>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // CardStatus
        builder.Entity<CardStatus>(e =>
        {
            e.ToTable("CardStatuses");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            e.HasIndex(x => x.Order).IsUnique(false);
            e.HasIndex(x => x.Name).IsUnique(true);
        });

        // Card
        builder.Entity<Card>(e =>
        {
            e.ToTable("Cards");
            e.HasKey(x => x.Id);

            e.HasIndex(x => x.UserId).IsUnique();

            e.Property(x => x.ExpirationDate)
                .HasConversion(
                    v => v.ToString("yyyy-MM-dd"),
                    v => DateOnly.Parse(v));

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Status)
                .WithMany()
                .HasForeignKey(x => x.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

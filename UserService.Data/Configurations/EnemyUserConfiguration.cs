using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Model.Entities;

namespace UserService.Data.Configurations;

public class EnemyUserConfiguration : IEntityTypeConfiguration<EnemyUser>
{
    public void Configure(EntityTypeBuilder<EnemyUser> builder)
    {
        builder.ToTable("enemies");
        builder.HasKey(e => new { e.UserId, e.EnemyId });

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.EnemyId)
            .HasColumnName("enemy_id")
            .IsRequired();
        
        builder.HasOne(e => e.User)
            .WithMany(u => u.Enemies)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Enemy)
            .WithMany(u => u.Enemies)
            .HasForeignKey(e => e.EnemyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
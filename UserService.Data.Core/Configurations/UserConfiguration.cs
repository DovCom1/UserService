using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Model.Entities;
using UserService.Model.Enums;

namespace UserService.Data.Core.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User> 
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .ValueGeneratedOnAdd();
        
        builder.Property(u => u.Uid)
            .HasColumnName("uid")
            .HasMaxLength(10)
            .IsRequired();
        
        builder.Property(u => u.Nickname)
            .HasColumnName("nickname")
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(128)
            .IsRequired();
        
        builder.Property(u => u.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasMaxLength(255);

        builder.Property(u => u.Gender)
            .HasColumnName("gender")
            .HasColumnType("SMALLINT")
            .IsRequired();
        
        builder.Property(u => u.Status)
            .HasColumnName("status")
            .HasColumnType("SMALLINT")
            .IsRequired();
        
        builder.Property(u => u.DateOfBirth)
            .HasColumnName("date_of_birth")
            .IsRequired();
        
        builder.Property(u => u.AccountCreationTime)
            .HasColumnName("account_creation_time")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();
        
        builder.HasIndex(u => u.Uid)
            .IsUnique()
            .HasDatabaseName("idx_users_uid");
        
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("idx_users_email");
        
        builder.HasIndex(u => u.Nickname)
            .HasDatabaseName("idx_users_nickname");
        
        builder.HasIndex(u => new {u.Nickname, u.Uid})
            .HasDatabaseName("idx_users_nickname_uid");
        
        builder.HasMany(u => u.Friends)
            .WithOne(f => f.User)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Enemies)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
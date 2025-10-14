using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Model.Entities;

namespace UserService.Data.Core.Configurations;

public class FriendUserConfiguration : IEntityTypeConfiguration<FriendUser>
{
    public void Configure(EntityTypeBuilder<FriendUser> builder)
    {
        builder.ToTable("friends");
        builder.HasKey(f => new { f.UserId, f.FriendId });
        
        builder.Property(f => f.UserId)
            .HasColumnName("user_id")
            .IsRequired();
        
        builder.Property(f => f.FriendId)
            .HasColumnName("friend_id")
            .IsRequired();

        builder.Property(f => f.Status)
            .HasColumnName("status")
            .HasColumnType("SMALLINT")
            .HasDefaultValue(1)
            .IsRequired();
        
        builder.HasIndex(f => new {f.UserId, f.Status})
            .HasDatabaseName("idx_friends_user_id_status");
        
        builder.HasIndex(f => new {f.FriendId, f.Status})
            .HasDatabaseName("idx_friends_friend_id_status");
        
        builder.HasOne(f => f.User)
            .WithMany(u => u.Friends)
            .HasForeignKey(f => f.UserId) 
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(f => f.Friend)
            .WithMany(u => u.Friends)
            .HasForeignKey(f => f.FriendId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
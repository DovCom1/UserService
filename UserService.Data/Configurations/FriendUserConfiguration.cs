using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Model.Entities;

namespace UserService.Data.Configurations;

public class FriendUserConfiguration : IEntityTypeConfiguration<FriendUser>
{
    public void Configure(EntityTypeBuilder<FriendUser> builder)
    {
        builder.ToTable("friends");
        builder.HasKey(fu => new { fu.UserId, fu.FriendId });
        
        builder.Property(fu => fu.UserId)
            .HasColumnName("user_id")
            .IsRequired();
        
        builder.Property(fu => fu.FriendId)
            .HasColumnName("friend_id")
            .IsRequired();

        builder.Property(fu => fu.Status)
            .HasColumnName("status")
            .HasDefaultValue(1)
            .IsRequired();
        
        builder.HasOne(f => f.User)
            .WithMany(u => u.Friends)
            .HasForeignKey(f => f.UserId) 
            .OnDelete(DeleteBehavior.Cascade);
    }
}
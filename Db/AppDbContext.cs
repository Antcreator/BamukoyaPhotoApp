using BamukoyaPhotoApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace BamukoyaPhotoApp.Db
{
    public class AppDbContext : IdentityDbContext<UserModel, IdentityRole<long>, long>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                optionsBuilder.UseNpgsql(configuration.GetConnectionString("AppDatabase"));
            }
        }

        public DbSet<PhotoModel> Photos { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<FriendRequest>()
                .HasOne(fr => fr.Sender)
                .WithMany(u => u.FriendRequestsSent)
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FriendRequest>()
                .HasOne(fr => fr.Receiver)
                .WithMany(u => u.FriendRequestsReceived)
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserModel>()
                .HasMany(u => u.Friends)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "UserFriends",
                    j => j
                        .HasOne<UserModel>()
                        .WithMany()
                        .HasForeignKey("FriendId")
                        .OnDelete(DeleteBehavior.Restrict),
                    j => j
                        .HasOne<UserModel>()
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict));
        }
    }
}

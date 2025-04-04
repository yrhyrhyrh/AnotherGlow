﻿using appBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace appBackend.Repositories // Adjust namespace as needed
{
    public class SocialMediaDbContext : DbContext
    {
        // Define DbSets for each entity (representing tables)
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<Follow> Follows { get; set; } = null!;
        public DbSet<Like> Likes { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<GroupMember> GroupMembers { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Attachment> Attachments { get; set; } = null!;

        public SocialMediaDbContext(DbContextOptions<SocialMediaDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- User Configuration ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users"); // Explicitly map to the table name

                entity.HasKey(u => u.UserId); // Primary Key
                entity.Property(u => u.UserId)
                      .HasColumnName("user_id") // Map to specific column name
                      .ValueGeneratedOnAdd(); // Let DB generate UUID (using default function)

                // Map properties to specific column names and configure constraints
                entity.Property(u => u.Username)
                      .HasColumnName("username")
                      .IsRequired()
                      .HasMaxLength(50);
                entity.HasIndex(u => u.Username, "users_username_unique").IsUnique(); // Named unique index

                entity.Property(u => u.Email)
                      .HasColumnName("email")
                      .IsRequired()
                      .HasMaxLength(255);
                entity.HasIndex(u => u.Email, "users_email_unique").IsUnique(); // Named unique index

                entity.Property(u => u.PasswordHash)
                      .HasColumnName("password_hash")
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(u => u.FullName)
                      .HasColumnName("full_name")
                      .HasMaxLength(100);

                entity.Property(u => u.Bio)
                      .HasColumnName("bio")
                      .HasColumnType("text"); // Explicitly map to TEXT

                entity.Property(u => u.ProfilePictureUrl)
                      .HasColumnName("profile_picture_url")
                      .HasMaxLength(255);

                entity.Property(u => u.CreatedAt)
                      .HasColumnName("created_at")
                      .IsRequired()
                      .HasDefaultValueSql("now()"); // Use DB function for default

                entity.Property(u => u.UpdatedAt)
                      .HasColumnName("updated_at")
                      .HasDefaultValueSql("now()"); // Use DB function for default

                entity.HasIndex(u => u.CreatedAt, "users_created_at_idx"); // Named index

                // Relationships defined after other entities are configured
            });

            // --- Post Configuration ---
            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("posts"); // Explicitly map to the table name

                entity.HasKey(p => p.PostId); // Primary Key
                entity.Property(p => p.PostId)
                      .HasColumnName("post_id")
                      .ValueGeneratedOnAdd(); // Let DB generate UUID

                entity.Property(p => p.UserId)
                      .HasColumnName("user_id")
                      .IsRequired();

                entity.Property(p => p.Content)
                      .HasColumnName("content")
                      .IsRequired()
                      .HasColumnType("text"); // Explicitly map to TEXT

                entity.Property(p => p.CreatedAt)
                      .HasColumnName("created_at")
                      .IsRequired()
                      .HasDefaultValueSql("now()");

                entity.Property(p => p.UpdatedAt)
                      .HasColumnName("updated_at")
                      .HasDefaultValueSql("now()");

                // Indexes (matches SQL)
                entity.HasIndex(p => p.UserId, "posts_user_id_idx");
                entity.HasIndex(p => p.CreatedAt, "posts_created_at_idx");

                // Configure relationships
                // One Post belongs to one Author (User)
                entity.HasOne(p => p.Author)
                      .WithMany(u => u.Posts) // The collection navigation property in User
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // Matches SQL ON DELETE CASCADE

                // One Post has Many Likes (Relationship defined in Like configuration)
            });

            // --- Follow Configuration (Many-to-Many Join Table for Users) ---
            modelBuilder.Entity<Follow>(entity =>
            {
                entity.ToTable("follows"); // Explicitly map to the table name

                // Composite Primary Key
                entity.HasKey(f => f.FollowId); // Primary Key
                entity.Property(f => f.FollowId)
                      .HasColumnName("follow_id")
                      .ValueGeneratedOnAdd(); // Let DB generate UUID

                entity.Property(f => f.FollowerUserId).HasColumnName("follower_user_id");
                entity.Property(f => f.FollowingUserId).HasColumnName("following_user_id");

                entity.Property(f => f.CreatedAt)
                      .HasColumnName("created_at")
                      .IsRequired()
                      .HasDefaultValueSql("now()");

                // Define the relationships to the User table for both foreign keys
                entity.HasOne(f => f.Follower) // Navigation property back to the follower User
                      .WithMany(u => u.Following) // Navigation property in User for who they follow
                      .HasForeignKey(f => f.FollowerUserId)
                      .OnDelete(DeleteBehavior.Cascade); // Matches SQL ON DELETE CASCADE

                entity.HasOne(f => f.Following) // Navigation property back to the followed User
                      .WithMany(u => u.Followers) // Navigation property in User for their followers
                      .HasForeignKey(f => f.FollowingUserId)
                      .OnDelete(DeleteBehavior.Cascade); // Matches SQL ON DELETE CASCADE

                // Indexes (matches SQL) - EF Core often creates these automatically for FKs,
                // but explicit naming ensures consistency with the SQL script.
                entity.HasIndex(f => f.FollowerUserId, "follows_follower_user_id_idx");
                entity.HasIndex(f => f.FollowingUserId, "follows_following_user_id_idx");
            });

            // --- Like Configuration (Many-to-Many Join Table for Users and Posts) ---
            modelBuilder.Entity<Like>(entity =>
            {
                entity.ToTable("likes"); // Explicitly map to the table name

                // Primary Key (matches SQL)
                entity.HasKey(l => l.LikeId);
                entity.Property(l => l.LikeId)
                      .HasColumnName("like_id")
                      .ValueGeneratedOnAdd(); // Let DB generate UUID

                entity.Property(l => l.UserId)
                      .HasColumnName("user_id")
                      .IsRequired();

                entity.Property(l => l.PostId)
                      .HasColumnName("post_id")
                      .IsRequired();

                entity.Property(l => l.CreatedAt)
                      .HasColumnName("created_at")
                      .IsRequired()
                      .HasDefaultValueSql("now()");

                // Unique constraint on User/Post combination (matches SQL)
                entity.HasIndex(l => new { l.UserId, l.PostId }, "likes_user_id_post_id_unique")
                      .IsUnique();

                // Indexes (matches SQL)
                entity.HasIndex(l => l.PostId, "likes_post_id_idx");
                entity.HasIndex(l => l.UserId, "likes_user_id_idx"); // Redundant with unique index but matches SQL

                // Define the relationships
                entity.HasOne(l => l.User) // Navigation property back to the User
                      .WithMany(u => u.Likes) // Navigation property in User for their likes
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // Matches SQL ON DELETE CASCADE

                entity.HasOne(l => l.Post) // Navigation property back to the Post
                      .WithMany(p => p.Likes) // Navigation property in Post for its likes
                      .HasForeignKey(l => l.PostId)
                      .OnDelete(DeleteBehavior.Cascade); // Matches SQL ON DELETE CASCADE
            });

            // --- Comment Configuration ---
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("comments"); // Table name

                entity.HasKey(c => c.CommentId); // Primary Key
                entity.Property(c => c.CommentId)
                      .HasColumnName("comment_id")
                      .ValueGeneratedOnAdd();

                entity.Property(c => c.PostId)
                      .HasColumnName("post_id")
                      .IsRequired();

                entity.Property(c => c.UserId)
                      .HasColumnName("user_id")
                      .IsRequired();

                entity.Property(c => c.Content)
                      .HasColumnName("content")
                      .IsRequired()
                      .HasColumnType("text");

                entity.Property(c => c.CreatedAt)
                      .HasColumnName("created_at")
                      .IsRequired()
                      .HasDefaultValueSql("now()");

                entity.Property(c => c.UpdatedAt)
                      .HasColumnName("updated_at")
                      .HasDefaultValueSql("now()");

                // Indexes
                entity.HasIndex(c => c.PostId, "comments_post_id_idx");
                entity.HasIndex(c => c.UserId, "comments_user_id_idx");
                entity.HasIndex(c => c.CreatedAt, "comments_created_at_idx");

                // Relationships
                entity.HasOne(l => l.User) // Navigation property back to the User
                      .WithMany(u => u.Comments) // Navigation property in User for their likes
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // Matches SQL ON DELETE CASCADE

                entity.HasOne(l => l.Post) // Navigation property back to the Post
                      .WithMany(p => p.Comments) // Navigation property in Post for its likes
                      .HasForeignKey(l => l.PostId)
                      .OnDelete(DeleteBehavior.Cascade); // Matches SQL ON DELETE CASCADE
            });

            // --- Attachment Configuration ---
            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.ToTable("attachments"); // Table name

                entity.HasKey(a => a.AttachmentId); // Primary Key
                entity.Property(a => a.AttachmentId)
                      .HasColumnName("attachment_id")
                      .ValueGeneratedOnAdd();

                entity.Property(a => a.PostId)
                      .HasColumnName("post_id")
                      .IsRequired();

                entity.Property(a => a.FileName)
                      .HasColumnName("file_name")
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(a => a.FilePath) // Or FileUrl
                      .HasColumnName("file_path") // Or file_url
                      .IsRequired()
                      .HasMaxLength(255); // Adjust max length as needed

                entity.Property(a => a.ContentType)
                      .HasColumnName("content_type")
                      .HasMaxLength(100);

                entity.Property(a => a.FileSize)
                      .HasColumnName("file_size");

                entity.Property(a => a.CreatedAt)
                      .HasColumnName("created_at")
                      .IsRequired()
                      .HasDefaultValueSql("now()");

                // Indexes
                entity.HasIndex(a => a.PostId, "attachments_post_id_idx");
                entity.HasIndex(a => a.CreatedAt, "attachments_created_at_idx");

                // Relationships
                entity.HasOne(a => a.Post) // Attachment for Post
                      .WithMany(p => p.Attachments) // Post can have many attachments
                      .HasForeignKey(a => a.PostId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        

        // --- Group Configuration ---
        modelBuilder.Entity<Group>(entity =>
            {
                entity.ToTable("groups");
                entity.HasKey(g => g.GroupId);
                entity.Property(g => g.GroupId)
                      .HasColumnName("group_id")
                      .ValueGeneratedOnAdd();
                entity.Property(g => g.Name)
                      .HasColumnName("name")
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(g => g.CreatedAt)
                      .HasColumnName("created_at")
                      .IsRequired()
                      .HasDefaultValueSql("now()");
                entity.Property(g => g.UpdatedAt)
                      .HasColumnName("updated_at")
                      .HasDefaultValueSql("now()");
            });

            // --- GroupMember Configuration ---
            modelBuilder.Entity<GroupMember>(entity =>
            {
                entity.ToTable("group_members");
                entity.HasKey(gm => gm.GroupMemberId);
                entity.Property(gm => gm.GroupMemberId)
                      .HasColumnName("group_member_id")
                      .ValueGeneratedOnAdd();
                entity.Property(gm => gm.UserId)
                      .HasColumnName("user_id")
                      .IsRequired();
                entity.Property(gm => gm.GroupId)
                      .HasColumnName("group_id")
                      .IsRequired();
                entity.Property(gm => gm.IsAdmin)
                      .HasColumnName("is_admin")
                      .IsRequired().HasDefaultValue(false);
                entity.Property(gm => gm.CreatedAt)
                      .HasColumnName("created_at")
                      .IsRequired()
                      .HasDefaultValueSql("now()");
                entity.HasIndex(gm => new { gm.UserId, gm.GroupId })
                      .IsUnique();
                entity.HasIndex(gm => gm.GroupId);
                entity.HasIndex(gm => gm.UserId);
                entity.HasOne(gm => gm.User)
                      .WithMany(u => u.GroupMemberships)
                      .HasForeignKey(gm => gm.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(gm => gm.Group)
                      .WithMany(g => g.Members)
                      .HasForeignKey(gm => gm.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        // Optional: Override OnConfiguring if you're not using dependency injection
        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     if (!optionsBuilder.IsConfigured)
        //     {
        //         // WARNING: Don't store connection strings directly in code for production
        //         optionsBuilder.UseNpgsql("Host=your_rds_endpoint;Database=your_db;Username=your_user;Password=your_password");
        //     }
        // }
    }
}
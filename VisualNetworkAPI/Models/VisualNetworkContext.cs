using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VisualNetworkAPI.Models;

public partial class VisualNetworkContext : DbContext
{
    public VisualNetworkContext()
    {
    }

    public VisualNetworkContext(DbContextOptions<VisualNetworkContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Board> Boards { get; set; }

    public virtual DbSet<BoardPost> BoardPosts { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Follower> Followers { get; set; }

    public virtual DbSet<LikePost> LikePosts { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostTag> PostTags { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Board>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Board__3213E83F1704428E");

            entity.ToTable("Board");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.Decoration).HasColumnName("decoration");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.LastUpdate)
                .HasColumnType("datetime")
                .HasColumnName("Last_update");
        });

        modelBuilder.Entity<BoardPost>(entity =>
        {
            entity.HasKey(e => new { e.BoardId, e.PostId }).HasName("PK__board_po__F0520DA994AF1A14");

            entity.ToTable("board_post");

            entity.Property(e => e.BoardId).HasColumnName("boardId");
            entity.Property(e => e.PostId).HasColumnName("postId");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.LastUpdate)
                .HasColumnType("datetime")
                .HasColumnName("Last_update");

            entity.HasOne(d => d.Board).WithMany(p => p.BoardPosts)
                .HasForeignKey(d => d.BoardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__board_pos__board__4F7CD00D");

            entity.HasOne(d => d.Post).WithMany(p => p.BoardPosts)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__board_pos__postI__5070F446");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comments__3213E83F261FFB70");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.LastUpdate)
                .HasColumnType("datetime")
                .HasColumnName("Last_update");
            entity.Property(e => e.PostId).HasColumnName("postId");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comments__postId__44FF419A");
        });

        modelBuilder.Entity<Follower>(entity =>
        {
            entity.HasKey(e => new { e.FollowerId, e.FollowedId }).HasName("PK__Follower__950CECCBC964DD5D");

            entity.Property(e => e.FollowerId).HasColumnName("Follower_id");
            entity.Property(e => e.FollowedId).HasColumnName("Followed_id");
            entity.Property(e => e.FollowDate)
                .HasColumnType("datetime")
                .HasColumnName("Follow_date");

            entity.HasOne(d => d.Followed).WithMany(p => p.FollowerFolloweds)
                .HasForeignKey(d => d.FollowedId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Followers__Follo__3B75D760");

            entity.HasOne(d => d.FollowerNavigation).WithMany(p => p.FollowerFollowerNavigations)
                .HasForeignKey(d => d.FollowerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Followers__Follo__3A81B327");
        });

        modelBuilder.Entity<LikePost>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.PostId }).HasName("PK__like_pos__764ADBC6056A024B");

            entity.ToTable("like_post");

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.PostId).HasColumnName("postId");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.LastUpdate)
                .HasColumnType("datetime")
                .HasColumnName("Last_update");

            entity.HasOne(d => d.Post).WithMany(p => p.LikePosts)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__like_post__postI__4222D4EF");

            entity.HasOne(d => d.User).WithMany(p => p.LikePosts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__like_post__userI__412EB0B6");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Post__3213E83F3D1A0E2F");

            entity.ToTable("Post");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedBy).HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImageUrls).HasColumnName("imageUrls");
            entity.Property(e => e.JsonPersonalizacion).HasColumnName("Json_personalizacion");
            entity.Property(e => e.LastUpdate)
                .HasColumnType("datetime")
                .HasColumnName("Last_update");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Posts)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Post__Created_by__3E52440B");
        });

        modelBuilder.Entity<PostTag>(entity =>
        {
            entity.HasKey(e => new { e.PostId, e.TagId }).HasName("PK__post_tag__B803B38F98462B94");

            entity.ToTable("post_tag");

            entity.Property(e => e.PostId).HasColumnName("postId");
            entity.Property(e => e.TagId).HasColumnName("tagId");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.LastUpdate)
                .HasColumnType("datetime")
                .HasColumnName("Last_update");

            entity.HasOne(d => d.Post).WithMany(p => p.PostTags)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__post_tag__postId__49C3F6B7");

            entity.HasOne(d => d.Tag).WithMany(p => p.PostTags)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__post_tag__tagId__4AB81AF0");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3213E83F58BFF9A9");

            entity.HasIndex(e => e.ExpiryDate, "IX_RefreshTokens_ExpiryDate");

            entity.HasIndex(e => e.Token, "IX_RefreshTokens_Token");

            entity.HasIndex(e => e.UserId, "IX_RefreshTokens_UserId");

            entity.HasIndex(e => e.Token, "UQ__RefreshT__CA90DA7A7577ACF1").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceInfo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("device_info");
            entity.Property(e => e.ExpiryDate)
                .HasColumnType("datetime")
                .HasColumnName("expiry_date");
            entity.Property(e => e.ReplacedByToken)
                .IsUnicode(false)
                .HasColumnName("replaced_by_token");
            entity.Property(e => e.RevokedAt)
                .HasColumnType("datetime")
                .HasColumnName("revoked_at");
            entity.Property(e => e.SessionId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("session_id");
            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__RefreshTo__user___5629CD9C");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tag__3213E83F8BB67BCC");

            entity.ToTable("Tag");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.LastUpdate)
                .HasColumnType("datetime")
                .HasColumnName("Last_update");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user__3213E83FDA174AEF");

            entity.ToTable("user");

            entity.HasIndex(e => e.Email, "IX_User_Email").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__user__AB6E616479AEDB59").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.DateBirth).HasColumnName("date_birth");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .HasColumnName("first_name");
            entity.Property(e => e.Genre)
                .HasMaxLength(255)
                .HasColumnName("genre");
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .HasColumnName("last_name");
            entity.Property(e => e.LastUpdate)
                .HasColumnType("datetime")
                .HasColumnName("last_update");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(255)
                .HasColumnName("phone");
            entity.Property(e => e.User1)
                .HasMaxLength(50)
                .HasColumnName("user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CommonLibrary.Auth.EFModels;

public partial class UserDbContext : DbContext
{
    public UserDbContext()
    {
    }

    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Action> Actions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<UserInfo> UserInfos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=TMWeb;Trusted_Connection=True; trustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Action>(entity =>
        {
            entity.HasKey(e => e.Code);

            entity.ToTable("Action");

            entity.Property(e => e.Code).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Code);

            entity.Property(e => e.Code).ValueGeneratedNever();
            entity.Property(e => e.RoleName)
                .HasMaxLength(10)
                .IsFixedLength();

            entity.HasMany(d => d.ActionCodes).WithMany(p => p.RoleCodes)
                .UsingEntity<Dictionary<string, object>>(
                    "RoleAction",
                    r => r.HasOne<Action>().WithMany()
                        .HasForeignKey("ActionCode")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RoleActions_Action"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleCode")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RoleActions_Roles"),
                    j =>
                    {
                        j.HasKey("RoleCode", "ActionCode");
                        j.ToTable("RoleActions");
                    });
        });

        modelBuilder.Entity<UserInfo>(entity =>
        {
            entity.ToTable("UserInfo");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("EMail");
            entity.Property(e => e.UserName).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.UserInfos)
                .HasForeignKey(d => d.RoleCode)
                .HasConstraintName("FK_UserInfo_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

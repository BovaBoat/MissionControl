using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MissionControlDatabase.Models;

public partial class MissionControlDbContext : DbContext
{
    DatabaseConfig _dbConfig;

    public MissionControlDbContext(DatabaseConfig dbConfig)
    {
        _dbConfig = dbConfig;
    }


    public MissionControlDbContext(DbContextOptions<MissionControlDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Node> Nodes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer($"Server={_dbConfig.ServerName};Database={_dbConfig.DatabaseName};Trusted_Connection=True;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__messages__0BBF6EE61ADFA430");

            entity.ToTable("messages");

            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.CommandCode).HasColumnName("command_code");
            entity.Property(e => e.NodeId).HasColumnName("node_id");
            entity.Property(e => e.Payload).HasColumnName("payload");
            entity.Property(e => e.Timestamp)
                .HasColumnType("datetime")
                .HasColumnName("timestamp");

            entity.HasOne(d => d.Node).WithMany(p => p.Messages)
                .HasForeignKey(d => d.NodeId)
                .HasConstraintName("FK__messages__node_i__4BAC3F29");
        });

        modelBuilder.Entity<Node>(entity =>
        {
            entity.HasKey(e => e.NodeId).HasName("PK__nodes__5F19EF169C000337");

            entity.ToTable("nodes");

            entity.Property(e => e.NodeId).HasColumnName("node_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("type");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

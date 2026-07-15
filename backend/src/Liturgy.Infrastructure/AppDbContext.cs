using Liturgy.Application.Abstractions;
using Liturgy.Domain;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Infrastructure;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Phase> Phases => Set<Phase>();
    public DbSet<Gate> Gates => Set<Gate>();
    public DbSet<Requirement> Requirements => Set<Requirement>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<RMovement> RMovements => Set<RMovement>();
    public DbSet<Decision> Decisions => Set<Decision>();
    public DbSet<ImpactMetric> ImpactMetrics => Set<ImpactMetric>();
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<Gratitude> Gratitudes => Set<Gratitude>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.Email).HasMaxLength(254).IsRequired();
            b.HasIndex(u => u.Email).IsUnique();
            b.Property(u => u.FirstName).HasMaxLength(64).IsRequired();
            b.Property(u => u.LastName).HasMaxLength(64).IsRequired();
            b.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            b.Property(u => u.Role).HasMaxLength(32).IsRequired();
        });

        modelBuilder.Entity<Workspace>(b =>
        {
            b.HasKey(w => w.Id);
            b.Property(w => w.Name).HasMaxLength(120).IsRequired();
            b.Property(w => w.Slug).HasMaxLength(120).IsRequired();
            b.HasIndex(w => w.Slug).IsUnique();
        });

        modelBuilder.Entity<Membership>(b =>
        {
            b.HasKey(m => m.Id);
            b.Property(m => m.Role).HasMaxLength(32).IsRequired();
            b.Property(m => m.Initials).HasMaxLength(4).IsRequired();
            b.HasIndex(m => new { m.WorkspaceId, m.UserId }).IsUnique();
        });

        modelBuilder.Entity<Project>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).HasMaxLength(120).IsRequired();
            b.Property(p => p.Tag).HasMaxLength(160).IsRequired();
            b.Property(p => p.CurrentPhase).HasConversion<int>();
            b.HasIndex(p => p.WorkspaceId);
        });

        modelBuilder.Entity<Phase>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Kind).HasConversion<int>();
            b.Property(p => p.State).HasConversion<int>();
            b.HasIndex(p => new { p.ProjectId, p.Order }).IsUnique();
        });

        modelBuilder.Entity<Gate>(b =>
        {
            b.HasKey(g => g.Id);
            b.Property(g => g.Title).HasMaxLength(160).IsRequired();
            b.Property(g => g.State).HasConversion<int>();
            b.HasIndex(g => g.PhaseId).IsUnique();
        });

        modelBuilder.Entity<Requirement>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.Label).HasMaxLength(300).IsRequired();
            b.Property(r => r.Meta).HasMaxLength(120);
            b.Property(r => r.State).HasConversion<int>();
            b.HasIndex(r => new { r.GateId, r.Order });
        });

        modelBuilder.Entity<Sprint>(b =>
        {
            b.HasKey(s => s.Id);
            b.HasIndex(s => new { s.ProjectId, s.Number }).IsUnique();
        });

        modelBuilder.Entity<Card>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Code).HasMaxLength(16).IsRequired();
            b.Property(c => c.Title).HasMaxLength(200).IsRequired();
            b.Property(c => c.Column).HasConversion<int>();
            b.Property(c => c.CurrentR).HasConversion<int?>();
            b.HasIndex(c => c.ProjectId);
            b.HasIndex(c => c.Code).IsUnique();
        });

        modelBuilder.Entity<RMovement>(b =>
        {
            b.HasKey(m => m.Id);
            b.Property(m => m.Kind).HasConversion<int>();
            b.Property(m => m.State).HasConversion<int>();
            b.Property(m => m.Ask).HasMaxLength(2000);
            b.Property(m => m.Received).HasMaxLength(2000);
            b.Property(m => m.Synthesis).HasMaxLength(2000);
            b.Property(m => m.ArtifactUrl).HasMaxLength(500);
            b.Property(m => m.WhatChanged).HasMaxLength(2000);
            b.Property(m => m.Thanksgiving).HasMaxLength(2000);
            b.HasIndex(m => new { m.CardId, m.Order }).IsUnique();
        });

        modelBuilder.Entity<Decision>(b =>
        {
            b.HasKey(d => d.Id);
            b.Property(d => d.ChosenPath).HasConversion<int>();
            b.Property(d => d.Rationale).HasMaxLength(2000).IsRequired();
            b.Property(d => d.PrayedOverWith).HasMaxLength(200).IsRequired();
            b.HasIndex(d => d.ProjectId).IsUnique();
        });

        modelBuilder.Entity<ImpactMetric>(b =>
        {
            b.HasKey(m => m.Id);
            b.Property(m => m.Value).HasMaxLength(32).IsRequired();
            b.Property(m => m.Unit).HasMaxLength(16);
            b.Property(m => m.Label).HasMaxLength(300).IsRequired();
            b.HasIndex(m => new { m.ProjectId, m.Order });
        });

        modelBuilder.Entity<Story>(b =>
        {
            b.HasKey(s => s.Id);
            b.Property(s => s.Text).HasMaxLength(1000).IsRequired();
            b.HasIndex(s => new { s.ProjectId, s.Order });
        });

        modelBuilder.Entity<Gratitude>(b =>
        {
            b.HasKey(g => g.Id);
            b.Property(g => g.Quote).HasMaxLength(1000).IsRequired();
            b.Property(g => g.Attribution).HasMaxLength(200).IsRequired();
            b.HasIndex(g => new { g.ProjectId, g.Order });
        });
    }
}

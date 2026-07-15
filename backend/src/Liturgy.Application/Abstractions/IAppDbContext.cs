using Liturgy.Domain;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Workspace> Workspaces { get; }
    DbSet<Membership> Memberships { get; }
    DbSet<Project> Projects { get; }
    DbSet<Phase> Phases { get; }
    DbSet<Gate> Gates { get; }
    DbSet<Requirement> Requirements { get; }
    DbSet<Sprint> Sprints { get; }
    DbSet<Card> Cards { get; }
    DbSet<RMovement> RMovements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

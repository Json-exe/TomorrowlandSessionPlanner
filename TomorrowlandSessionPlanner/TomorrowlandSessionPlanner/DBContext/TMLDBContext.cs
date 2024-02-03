using Microsoft.EntityFrameworkCore;
using TomorrowlandSessionPlanner.Models;

namespace TomorrowlandSessionPlanner.DBContext;

public class TmldbContext : DbContext
{
    public virtual DbSet<Dj> Djs { get; init; } = null!;
    public virtual DbSet<Stage> Stages { get; init; } = null!;
    public virtual DbSet<Session> Sessions { get; init; } = null!;

    public TmldbContext(DbContextOptions<TmldbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Session>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Dj)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.DjId);
            builder.HasOne(x => x.Stage)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.StageId);
        });
    }
}
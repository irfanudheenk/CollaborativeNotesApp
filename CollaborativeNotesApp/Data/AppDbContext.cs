using Microsoft.EntityFrameworkCore;
using CollaborativeNotesApp.Models;

namespace CollaborativeNotesApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Note> Notes { get; set; }
}
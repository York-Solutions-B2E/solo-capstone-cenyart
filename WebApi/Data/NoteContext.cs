using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Data
{
    public class NoteContext(DbContextOptions<NoteContext> options) : DbContext(options)
    {
        public DbSet<Note> Notes { get; set; } = default!;
    }
}

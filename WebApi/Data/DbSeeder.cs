using WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Data
{
    public static class DbSeeder
    {
        public static void Seed(NoteContext context)
        {
            context.Database.Migrate(); // Ensure DB is up to date

            if (!context.Notes.Any())
            {
                context.Notes.AddRange(
                    new Note { Title = "Welcome Note", Content = "This is your first note.", CreatedAt = DateTime.UtcNow },
                    new Note { Title = "Reminder", Content = "Don't forget to test CRUD!", CreatedAt = DateTime.UtcNow }
                );
                context.SaveChanges();
            }
        }
    }
}

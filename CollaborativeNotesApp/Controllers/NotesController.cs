using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CollaborativeNotesApp.Data;
using CollaborativeNotesApp.Models;

namespace CollaborativeNotesApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/notes/{id}
    [HttpGet("{id?}")]
    public async Task<IActionResult> GetNote(string id = "default")
    {
        // Handle the request from Home.razor which might be calling api/notes/ (no id)
        if (string.IsNullOrEmpty(id) || id == "null" || id == "undefined")
        {
            id = "default";
        }

        int noteId;
        if (id == "default" || id == "demo-note")
        {
            noteId = 1;
        }
        else if (!int.TryParse(id, out noteId))
        {
            noteId = 1;
        }

        var note = await _context.Notes.FindAsync(noteId);

        if (note == null)
        {
            note = new Note
            {
                Id = noteId,
                Title = "My Collaborative Note",
                Content = "Welcome to collaborative editing!\n\nStart typing and open another browser window to see real-time updates.",
                LastUpdated = DateTime.UtcNow,
                LastUpdatedBy = "System"
            };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
        }

        return Ok(note);
    }

    // PUT: api/notes/{id}
    [HttpPut("{id?}")]
    public async Task<IActionResult> UpdateNote(string id, [FromBody] Note updatedNote)
    {
        if (string.IsNullOrEmpty(id) || id == "null" || id == "undefined")
        {
            id = "default";
        }

        int noteId;
        if (id == "default" || id == "demo-note")
        {
            noteId = 1;
        }
        else if (!int.TryParse(id, out noteId))
        {
            noteId = 1;
        }

        var note = await _context.Notes.FindAsync(noteId);

        if (note == null)
        {
            note = new Note { Id = noteId };
            _context.Notes.Add(note);
        }

        note.Title = updatedNote?.Title ?? "My Collaborative Note";
        note.Content = updatedNote?.Content ?? "";
        note.LastUpdated = DateTime.UtcNow;
        note.LastUpdatedBy = updatedNote?.LastUpdatedBy ?? "Anonymous";

        await _context.SaveChangesAsync();
        return Ok(note);
    }
}
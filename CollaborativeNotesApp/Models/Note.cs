using System.ComponentModel.DataAnnotations;

namespace CollaborativeNotesApp.Models;

public class Note
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = "Untitled Note";

    public string Content { get; set; } = "";

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public string LastUpdatedBy { get; set; } = "Anonymous";
}
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace CollaborativeNotesApp.Hubs;

public class NotesHub : Hub
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> _noteUsers = new();
    private static readonly ConcurrentDictionary<string, string> _noteContent = new();

    public async Task JoinNote(string noteId, string userName)
    {
        Console.WriteLine($"Join: {userName} joined {noteId}");

        await Groups.AddToGroupAsync(Context.ConnectionId, noteId);

        _noteUsers.AddOrUpdate(noteId,
            new HashSet<string> { userName },
            (_, existing) => { existing.Add(userName); return existing; });

        // Send current content to the new user
        if (_noteContent.TryGetValue(noteId, out var content))
        {
            await Clients.Caller.SendAsync("ReceiveNoteContent", content);
        }

        await Clients.Group(noteId).SendAsync("UserJoined", userName);
        await SendActiveUsers(noteId);
    }

    public async Task UpdateNote(string noteId, string content, string userName)
    {
        Console.WriteLine($"Update from {userName}: {content?.Length} chars");

        _noteContent[noteId] = content;

        await Clients.Group(noteId).SendAsync("NoteUpdated", new
        {
            Content = content,
            UserName = userName,
            Timestamp = DateTime.UtcNow
        });
    }

    // NEW: Typing indicator method
    public async Task Typing(string noteId, string userName, bool isTyping)
    {
        await Clients.Group(noteId).SendAsync("UserTyping", new
        {
            UserName = userName,
            IsTyping = isTyping
        });
    }

    private async Task SendActiveUsers(string noteId)
    {
        if (_noteUsers.TryGetValue(noteId, out var users))
        {
            await Clients.Group(noteId).SendAsync("ActiveUsers", users.ToList());
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        foreach (var kvp in _noteUsers)
        {
            if (kvp.Value.RemoveWhere(u => u.Contains(Context.ConnectionId)) > 0)
            {
                await SendActiveUsers(kvp.Key);
                break;
            }
        }
        await base.OnDisconnectedAsync(exception);
    }
}
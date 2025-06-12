using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class ChatMessage
{
    public int ChatMessageId { get; set; }

    public int ChatBoxId { get; set; }

    public int? UserId { get; set; }
    public int ChatBoxId { get; set; }

    public string? Message { get; set; }

    public DateTime SentAt { get; set; }

    public virtual User? User { get; set; }
}

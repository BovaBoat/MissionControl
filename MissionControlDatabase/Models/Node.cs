using System;
using System.Collections.Generic;

namespace MissionControlDatabase.Models;

public partial class Node
{
    public int NodeId { get; set; }

    public string? Name { get; set; }

    public string? Type { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}

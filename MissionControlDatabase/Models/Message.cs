using System;
using System.Collections.Generic;

namespace MissionControlDatabase.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int? NodeId { get; set; }

    public int CommandCode { get; set; }

    public byte[]? Payload { get; set; }

    public DateTime Timestamp { get; set; }

    public virtual Node? Node { get; set; }
}

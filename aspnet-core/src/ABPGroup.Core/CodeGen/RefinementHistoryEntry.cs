using System;
using System.Collections.Generic;

namespace ABPGroup.CodeGen;

public class RefinementHistoryEntry
{
    public DateTime Timestamp { get; set; }
    public string ChangeRequest { get; set; }
    public List<string> ChangedFiles { get; set; } = new();
    public List<string> DeletedFiles { get; set; } = new();
}

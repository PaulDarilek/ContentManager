namespace WindowsInfo;

public class Partition
{
    public Partition() => LogicalDisks = [];
    public bool Bootable { get; internal set; }
    public bool BootPartition { get; internal set; }
    public uint DiskIndex { get; internal set; }
    public List<LogicalDisk> LogicalDisks { get; internal set; }
    public ulong PartitionBlockSize { get; internal set; }
    public ulong PartitionNumberOfBlocks { get; internal set; }
    public ulong PartitionStartingOffset { get; internal set; }
    public bool PrimaryPartition { get; internal set; }

    public override string ToString()
    {
        var props = new List<string>();
        if (PrimaryPartition) props.Add(nameof(PrimaryPartition));
        if (BootPartition) props.Add(nameof(BootPartition));
        if (Bootable) props.Add(nameof(Bootable));
        var propMsg = props.Count > 0 ? $" ({string.Join(", ", props)})" : string.Empty;

        return $"{GetType().Name}: Disk {DiskIndex} @ {PartitionStartingOffset}({PartitionNumberOfBlocks * PartitionBlockSize}){propMsg}";
    }

}

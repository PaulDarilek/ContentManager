using System.Management;
using System.Runtime.Versioning;

namespace WindowsInfo;

public class DiskDrive
{
    private int m_PartionsCount = 0;
    public DiskDrive() => Partitions = new List<Partition>(1);
    public string? Caption { get; private set; }
    public string? DeviceID { get; private set; }
    public string? FirmwareRevision { get; private set; }
    public ulong FreeSpace { get; private set; }
    public string? InterfaceType { get; private set; }
    public bool MediaLoaded { get; private set; }
    public string? MediaType { get; private set; }
    public string? Model { get; private set; }
    public uint NumberOfPartitions { get; private set; }
    public List<Partition> Partitions { get; private set; }
    public string? PNPDeviceID { get; private set; }
    public string? SerialNumber { get; private set; }
    public ulong Size { get; private set; }
    public string? Status { get; private set; }
    public ulong TotalCylinders { get; private set; }
    public uint TotalHeads { get; private set; }
    public ulong TotalSectors { get; private set; }
    public ulong TotalTracks { get; private set; }
    public uint TracksPerCylinder { get; private set; }

    [SupportedOSPlatform("windows")]
    internal void GetDiskDriveInfo(ManagementBaseObject diskDrive)
    {
        Caption = diskDrive.GetPropertyValue(nameof(Caption))?.ToString();
        DeviceID = diskDrive[nameof(DeviceID)]?.ToString();
        FirmwareRevision = diskDrive[nameof(FirmwareRevision)]?.ToString();
        InterfaceType = diskDrive[nameof(InterfaceType)]?.ToString();
        MediaLoaded = (bool?)diskDrive[nameof(MediaLoaded)] ?? false;
        MediaType = diskDrive[nameof(MediaType)]?.ToString();
        Model = diskDrive[nameof(Model)]?.ToString();
        NumberOfPartitions = (uint?)diskDrive[nameof(Partitions)] ?? 0;
        PNPDeviceID = diskDrive[nameof(PNPDeviceID)]?.ToString();
        SerialNumber = diskDrive[nameof(SerialNumber)]?.ToString()?.Trim();
        Size = (ulong?)diskDrive[nameof(Size)] ?? 0L;
        Status = diskDrive[nameof(Status)]?.ToString();
        TotalCylinders = (ulong?)diskDrive[nameof(TotalCylinders)] ?? 0;
        TotalHeads = (uint?)diskDrive[nameof(TotalHeads)] ?? 0U;
        TotalSectors = (ulong?)diskDrive[nameof(TotalSectors)] ?? 0;
        TotalTracks = (ulong?)diskDrive[nameof(TotalTracks)] ?? 0;
        TracksPerCylinder = (uint?)diskDrive[nameof(TracksPerCylinder)] ?? 0;
    }

    [SupportedOSPlatform("windows")]
    internal void GetDiskPartitionInfo(ManagementBaseObject partitions)
    {
        m_PartionsCount += 1;
        Partitions.Add(new Partition()
        {
            DiskIndex = (uint?)partitions["DiskIndex"] ?? 0,
            PartitionBlockSize = (ulong?)partitions["BlockSize"] ?? 0,
            Bootable = (bool?)partitions["Bootable"] ?? false,
            BootPartition = (bool?)partitions["BootPartition"] ?? false,
            PartitionNumberOfBlocks = (ulong?)partitions["NumberOfBlocks"] ?? 0,
            PrimaryPartition = (bool?)partitions["PrimaryPartition"] ?? false,
            PartitionStartingOffset = (ulong?)partitions["StartingOffset"] ?? 0
        });
    }

    [SupportedOSPlatform("windows")]
    internal void GetLogicalDiskInfo(ManagementBaseObject logicalDisk)
    {
        if (m_PartionsCount == 0) return;
        Partitions[m_PartionsCount - 1].LogicalDisks.Add(new LogicalDisk()
        {
            FileSystem = logicalDisk["FileSystem"]?.ToString(),
            FreeSpace = (ulong?)logicalDisk[nameof(FreeSpace)] ?? 0,
            LogicalDiskVolume = logicalDisk[nameof(DeviceID)]?.ToString(),
            SupportsDiskQuotas = (bool?)logicalDisk["SupportsDiskQuotas"] ?? false,
            VolumeName = logicalDisk["VolumeName"]?.ToString(),
            VolumeSerialNumber = logicalDisk["VolumeSerialNumber"]?.ToString()
        });
        //Linq's Sum() does not sum ulong(s)
        foreach (Partition p in Partitions)
        {
            foreach (LogicalDisk ld in p.LogicalDisks)
            {
                FreeSpace += ld.FreeSpace;
            }
        }
    }

    public override string ToString() => $"{GetType().Name}: #{SerialNumber} ({Size}, '{Model}', {Status}, {NumberOfPartitions})";
}

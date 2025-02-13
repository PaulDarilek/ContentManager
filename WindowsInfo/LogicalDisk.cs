namespace WindowsInfo;

public class LogicalDisk
{
    /// <summary>Drive Letter</summary>
    public string? LogicalDiskVolume { get; internal set; }

    /// <summary>Serial Number created by Formatting the Logical Disk. (does not have '-' like returned from VOL command)</summary>
    public string? VolumeSerialNumber { get; internal set; }

    /// <summary>Volume Label</summary>
    public string? VolumeName { get; internal set; }

    /// <summary>NTFS/FAT32 etc</summary>
    public string? FileSystem { get; internal set; }

    /// <summary>Free Space Available</summary>
    public ulong FreeSpace { get; internal set; }

    /// <summary>Volume Label</summary>
    public bool SupportsDiskQuotas { get; internal set; }

    public override string ToString()
    {
        const int KB = 1024;
        const int MB = KB * KB;
        const int TB = MB * KB;

        string free =
            FreeSpace > TB ? $"{FreeSpace / TB:0.000} TB free" :
            FreeSpace > MB ? $"{FreeSpace / MB:0.000} MB free" :
            FreeSpace > KB ? $"{FreeSpace / KB:0.000} KB free" :
            $"{FreeSpace} free";

        return $"{GetType().Name}: {LogicalDiskVolume}('{VolumeSerialNumber}', '{VolumeName}', {FileSystem}, {free})";
    }
}

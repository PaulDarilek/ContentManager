using System.Management;
using System.Runtime.Versioning;
using EnumerationOptions = System.Management.EnumerationOptions;

namespace WindowsInfo;

public record LogicalDriveExport
    (
    string? DriveLetter,
    string? VolumeSerialNumber,
    string? VolumeLabel,
    string? HardwareSerialNumber,
    string? Model
    );


/// <summary>
/// 
/// </summary>
/// <example>
///     // Simple Example Local Machine, no authentication:
///     var systemDrives = new SystemDrives(null);
///     var driveList = systemDrives.GetUSBDrivesInfo(null, null, null);
///     
///     // Remote Domain Machine...
///     // When [Computer Name] is null or empty, it uses the Local Machine name.
///     var systemDrives = new SystemDrives("[Computer Name]");
/// 
///     // [UserName], [Password], [Domain] are used to connect to a NT Domain. They can be blank or null if not connecting to a domain.
///     var driveList = systemDrives.GetUSBDrivesInfo([UserName], [Password], [Domain]);
/// </example>
[SupportedOSPlatform("windows")]
public class SystemDrives
{
    private string ComputerName { get; }

    public SystemDrives() : this(Environment.MachineName)
    {
    }

    /// <summary>Default Contructor</summary>
    /// <param name="computerName">Remote Computer name. Null uses MachineName</param>
    public SystemDrives(string computerName)
    {
        ComputerName =
            string.IsNullOrWhiteSpace(computerName) ?
            Environment.MachineName :
            computerName;
    }

    public LogicalDriveExport[] GetCurrentSystemDrives()
    {
        var drives = GetDiskDrives();
        var list = new List<LogicalDriveExport>();
        foreach (var drive in drives)
        {
            foreach (var partition in drive.Partitions)
            {
                foreach (var logicalDrive in partition.LogicalDisks)
                {
                    var export =
                        new LogicalDriveExport(
                            DriveLetter: logicalDrive.LogicalDiskVolume,
                            VolumeSerialNumber: logicalDrive.VolumeSerialNumber,
                            VolumeLabel: logicalDrive.VolumeName,
                            HardwareSerialNumber: drive.SerialNumber,
                            Model: drive.Model
                            );
                    list.Add(export);
                }
            }
        }
        return [.. list];
    }

    public List<DiskDrive> GetDiskDrives() => GetDiskDrives(null, null, null, false);

    public List<DiskDrive> GetDiskDrives(string? userName, string? password, string? domain, bool usbOnly = false)
    {
        var wmiQueryResult = new List<DiskDrive>();
        ConnectionOptions connOptions = GetConnectionOptions(userName, password, domain);
        EnumerationOptions mOptions = GetEnumerationOptions(false);
        var mScope = new ManagementScope($@"\\{ComputerName}\root\CIMV2", connOptions);
        string where = usbOnly ? "WHERE InterfaceType='USB'" : string.Empty;
        var selQuery = new SelectQuery($"SELECT * FROM Win32_DiskDrive {where}");
        mScope.Connect();

        using var moSearcher = new ManagementObjectSearcher(mScope, selQuery, mOptions);
        foreach (ManagementBaseObject moDiskDrive in moSearcher.Get())
        {
            var usbInfo = new DiskDrive();
            usbInfo.GetDiskDriveInfo(moDiskDrive);

            var relQuery = new RelatedObjectQuery(
                $"Associators of {{Win32_DiskDrive.DeviceID='{moDiskDrive.Properties["DeviceID"].Value}'}} " +
                $"WHERE AssocClass = Win32_DiskDriveToDiskPartition");
            using (var moAssocPart = new ManagementObjectSearcher(mScope, relQuery, mOptions))
            {
                foreach (ManagementBaseObject moAssocPartition in moAssocPart.Get())
                {

                    usbInfo.GetDiskPartitionInfo(moAssocPartition);
                    relQuery = new RelatedObjectQuery(
                        $"Associators of {{Win32_DiskPartition.DeviceID='{moAssocPartition.Properties["DeviceID"].Value}'}} " +
                        $"WHERE AssocClass = CIM_LogicalDiskBasedOnPartition");

                    using (var moLogDisk = new ManagementObjectSearcher(mScope, relQuery, mOptions))
                    {
                        foreach (ManagementBaseObject moLogDiskEnu in moLogDisk.Get())
                        {
                            usbInfo.GetLogicalDiskInfo(moLogDiskEnu);
                            moLogDiskEnu.Dispose();
                        }
                    }
                    moAssocPartition.Dispose();
                }
                wmiQueryResult.Add(usbInfo);
            }
            moDiskDrive.Dispose();
        }
        return wmiQueryResult;
    }   //GetUSBDrivesInfo()

    private static EnumerationOptions GetEnumerationOptions(bool DeepScan)
    {
        var mOptions = new EnumerationOptions()
        {
            Rewindable = false,        //Forward only query => no caching
            ReturnImmediately = true,  //Pseudo-async result
            DirectRead = true,
            EnumerateDeep = DeepScan
        };
        return mOptions;
    }

    private static ConnectionOptions GetConnectionOptions(string? userName, string? password, string? domain)
    {
        var connOptions = new ConnectionOptions()
        {
            EnablePrivileges = true,
            Timeout = ManagementOptions.InfiniteTimeout,
            Authentication = AuthenticationLevel.PacketPrivacy,
            Impersonation = ImpersonationLevel.Impersonate,
            Username = userName,
            Password = password,
            Authority = !string.IsNullOrEmpty(domain) ? $"NTLMDOMAIN:{domain}" : string.Empty  //Authority = "NTLMDOMAIN:[domain]"
        };
        return connOptions;
    }


} // SystemDrives

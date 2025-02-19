namespace WindowsInfo;

public record LogicalDriveExport
    (
    string? DriveLetter,
    string? VolumeSerialNumber,
    string? VolumeLabel,
    string? HardwareSerialNumber,
    string? Model
    );

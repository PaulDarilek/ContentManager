using ContentManagement.Models;

namespace ContentManager.Tests;

public class DriveTests
{
    [Fact]
    public void ConstructorTest()
    {
        var info = new DriveInfo("c:");
        var drive = new Drive(info);

        Assert.NotNull(drive);
        Assert.Equal(info.IsReady, drive.IsReady);
        Assert.Equal(info.DriveType.ToString(), drive.DriveType);

        Assert.Equal(info.VolumeLabel, drive.VolumeLabel);
        Assert.Equal(info.DriveFormat, drive.DriveFormat);
        Assert.Equal(info.Name.ToUpper(), drive.DriveLetter + '\\');
        Assert.Equal(info.TotalSize, drive.TotalSize);

        long diff = Math.Abs(info.TotalFreeSpace - (drive.TotalFreeSpace ?? 0) );
        const int maxDiff = 1024 * 1024;
        Assert.True(diff < maxDiff);

        Assert.NotNull(drive.VolumeSerialNumber);
        Assert.True(drive.VolumeSerialNumber.Length > 0);
    }
}
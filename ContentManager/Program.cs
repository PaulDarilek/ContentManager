using ContentManagement;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace ContentManager;


public static class Program
{
    private static CommandParser Parser { get; } 
        = new CommandParser([
            new CommandType(nameof(Recurse), Recurse) {Description = "Find Files in subdirectories"},
            new CommandType(nameof(NoRecurse), NoRecurse) {Description = "Do not look in subdirectories"},
            new CommandType(nameof(ComputeHash), ComputeHash) {Description = "Compute Hash Code when scanning Files"},
            new CommandType(nameof(NoComputeHash), NoComputeHash) {Description = "Do not Compute has when scanning Files"},

            new CommandType(nameof(Scan), Scan) { MinParms = 1, MaxParms = 100, Description = "Scan Paths for Files to Add to Datbase" },
            new CommandType(nameof(HashCodesForDrive), HashCodesForDrive) { MinParms = 0, MaxParms = 10, Description = "Compute Hashes Missing from Database for one or more Drive Letters"  },
            new CommandType(nameof(VerifyFilesExist), VerifyFilesExist) { MinParms = 0, MaxParms = 10 , Description = "Verify Files already Scanned for one or more Drive Letters" },
            //new CommandType(nameof(Restore), Restore) { MinParms = 0, MaxParms = 100 , Description = "" },
            new CommandType(nameof(DeleteFile), DeleteFile) { MinParms = 0, MaxParms = 100 , Description = "" },
        ]);

    private static ContentManagement.FileManager? Manager { get; set; }
    private static FileScanner FileScanner { get; } = new FileScanner();
    private static bool RecurseSwitch { get; set; }
    private static bool ComputeHashSwitch { get; set; }


    private static TextWriter StdOut { get; } = System.Console.Out;
    private static TextWriter StdErr { get; } = System.Console.Error;

    public static void Main(string[] args)
    {

        var settings = GetAppSettings();

        var contentDb = new ContentManagement.Storage.Sqlite.ContentStore(settings.ContentMangementConnectionString);
        Manager = new ContentManagement.FileManager(contentDb);

        GetDrives();

        settings.Arguments.AddRange(args);
        Parser.Parse([.. settings.Arguments]);
        Parser.PromptForActions();

        foreach (var command in Parser.ExecuteAll(LogException))
        {
            StdOut.WriteLine($"{command.Name}({string.Join(", ", command.Parameters)})");
        }

        static void LogException(Exception ex) => StdErr.WriteLine(ex.ToString());

    }

    private static void GetDrives()
    {
        if (Manager == null)
            return;
        
        if (OperatingSystem.IsWindows())
        {
            var drives = new WindowsInfo.SystemDrives().GetCurrentSystemDrives();
            foreach (var drive in drives)
            {
                Manager.SetDriveModelAndHardwareSerialNumber(drive.DriveLetter, drive.VolumeSerialNumber, drive.VolumeLabel, drive.HardwareSerialNumber, drive.Model);
            }
            Manager.SaveChanges();
        }

        foreach (var drive in Manager.Drives.Values)
        {
            StdOut.WriteLine($"{drive.DriveLetter} {drive.TotalSize} {drive.VolumeLabel} {drive.VolumeSerialNumber} {drive.HardwareSerialNumber} {drive.Model}");
        }
    }

    public static AppSettings GetAppSettings()
    {
        var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


        IConfiguration config = builder.Build();
        var section = config.GetSection(nameof(AppSettings));
        AppSettings settings = config.GetSection(nameof(AppSettings)).Get<AppSettings>() ?? new AppSettings();
        settings.ContentMangementConnectionString = Environment.ExpandEnvironmentVariables(settings.ContentMangementConnectionString);
        return settings;
    }

    private static void ComputeHash(IEnumerable<string> ignore) => ComputeHashSwitch = true;
    private static void NoComputeHash(IEnumerable<string> ignore) => ComputeHashSwitch = false;
    private static void Recurse(IEnumerable<string> ignore) => RecurseSwitch = true;
    private static void NoRecurse(IEnumerable<string> ignore) => RecurseSwitch = false;

    /// <summary></summary>
    /// <param name="filePaths"></param>
    private static void Scan(IEnumerable<string> filePaths) 
    {
        foreach (var filePath in filePaths)
        {
            var files = GetFiles(filePath);
            Manager?.Scan(files,ComputeHashSwitch);
        }
    }

    
    /// <summary>Compute Hashhes for files on a drive</summary>
    private static void HashCodesForDrive(IEnumerable<string> drives) {
        if(!drives.Any())
        {
            drives = Manager!.Drives.Values.Select(d => d.DriveLetter);
        }
        foreach (var drive in drives)
        {
            Manager!.HashFilesForDrive(drive);
        }
    }

    private static void VerifyFilesExist(IEnumerable<string> driveLetters) {
        foreach (string driveLetter in driveLetters)
        {
            var drive = Manager?.Drives.Values.FirstOrDefault(d => d.DriveLetter.Equals(driveLetter, StringComparison.OrdinalIgnoreCase));
            if (drive != null)
            {
                Manager?.VerifyFilesExist(drive, ComputeHashSwitch);
            }
        }
    }

    private static void DeleteFile(IEnumerable<string> files) 
    {
        Manager?.DeleteFile(GetFiles(files), deleteFromFileSystem: false);
    }

    private static List<FileInfo> GetFiles(IEnumerable<string> filePaths)
    {
        var list = new List<FileInfo>();
        foreach (var path in filePaths)
        {
            StdOut.WriteLine($"\t{path}");

            // Normalize to Full Path.
            var info = new FileInfo(path);

            if (info.Exists)
            {
                //Single File...
                list.Add(info);
                continue;
            }
            if (info.Directory == null)
            {
                // Bad Path
                continue;
            }

            if (FileScanner.DirectoriesToSkip.Count > 0)
            {
                FileScanner.AllowDirectoryAndChildren(info.Directory);
            }

            // Get Files and SubDirectories
            if (RecurseSwitch)
            {
                // Add subdirectories under this one with System, Hidden, or ReparsePoints set.
                FileScanner.SkipDirectoriesWithAttributes(info.Directory.GetDirectories());
            }
            list.AddRange(FileScanner.EnumerateFiles(info.Directory, info.Name, RecurseSwitch));
        }
        return list;
    }

    private static IEnumerable<FileInfo> GetFiles(string filePath)
    {
        StdOut.WriteLine($"\t{filePath}");

        // Normalize to Full Path.
        var info = new FileInfo(filePath);

        if (info.Exists)
        {
            //Single File...
            return [info];
        }
        if (info.Directory == null)
        {
            // Bad Path
            return [];
        }


        if (FileScanner.DirectoriesToSkip.Count > 0)
        {
            FileScanner.AllowDirectoryAndChildren(info.Directory);
        }

        // Get Files and SubDirectories
        if (RecurseSwitch)
        {
            // Add subdirectories under this one with System, Hidden, or ReparsePoints set.
            FileScanner.SkipDirectoriesWithAttributes(info.Directory.GetDirectories());
        }
        return FileScanner.EnumerateFiles(info.Directory, info.Name, RecurseSwitch);
    }

}
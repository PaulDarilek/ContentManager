using Microsoft.EntityFrameworkCore;

namespace FileManagement.Database.SqLite
{
    internal class FileManagementContext : DbContext
    {
        private static bool _migrated;

        public string ConnectionString { get; }

        public virtual DbSet<Drive> Drives { get; set; }
        public virtual DbSet<Directory> Directories { get; set; }
        public virtual DbSet<File> Files { get; set; }
        public virtual DbSet<FileDuplicates> FileDuplicates { get; set; }


        [Obsolete("Used for EF Migrations Only", error: true)]
        public FileManagementContext() 
        {
            var dir = new DirectoryInfo(Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Sqlite\\"));
            if (!dir.Exists) dir.Create();
            ConnectionString = "Data Source=" + Path.Combine(dir.FullName, "FileManagement.sqlite");
        }

        public FileManagementContext(string connectionString) : base()
        {
            ConnectionString =
                string.IsNullOrWhiteSpace(connectionString) ?
                $"Data Source={GetType().Namespace ?? "FileManagement.Sqlite"}" : 
                connectionString ;
             
            MigrateSchemaChanges();
        }

        public void MigrateSchemaChanges()
        {
            if (!_migrated)
            {
                base.Database.Migrate();
                base.SaveChanges(true);
                _migrated = true;
            }
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new ModelHelper().ConfigureModels(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }




    }
}

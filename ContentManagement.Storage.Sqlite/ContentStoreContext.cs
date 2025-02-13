using ContentManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ContentManagement.Storage.Sqlite
{
    public class ContentStoreContext : DbContext
    {
        private static bool _migrated;

        public string ConnectionString { get; }


        /// <summary></summary>
        public virtual DbSet<Document> Documents { get; set; }

        /// <summary></summary>
        public virtual DbSet<DocumentBlob> Blobs { get; set; }

        /// <summary>Logical Disk Drives</summary>
        public virtual DbSet<Drive> Drives { get; set; }

        /// <summary></summary>
        public virtual DbSet<Models.Directory> Directories { get; set; }


        public virtual DbSet<Models.File> Files { get; set; }

        public virtual DbSet<FileDuplicates> FileDuplicates { get; set; }

        [Obsolete("Used for EF Migrations Only", error: true)]
        public ContentStoreContext() : base()
        {
            ConnectionString = "Data Source=%USERPROFILE%\\Sqlite\\ContentManagement.db";
        }

        public ContentStoreContext(string connectionString) : base()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
            ConnectionString = connectionString;
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

        protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
        {
            new ModelHelper().ConfigureModels(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        


    }
}

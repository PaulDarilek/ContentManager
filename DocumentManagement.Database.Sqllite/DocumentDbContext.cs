using DocumentManagement;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagement.Database.Sqllite
{
    public class DocumentDbContext : DbContext
    {
        private static bool _migrated;

        public string ConnectionString { get; }


        /// <summary></summary>
        public virtual DbSet<Document> Documents { get; set; }

        /// <summary></summary>
        public virtual DbSet<DocumentBlob> Blobs { get; set; }

        [Obsolete("Used for EF Migrations Only", error: true)]
        public DocumentDbContext() : base()
        {
            ConnectionString = "Data Source=%USERPROFILE%\\Sqlite\\ContentManagement.db";
        }

        public DocumentDbContext(string connectionString) : base()
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new ModelHelper().ConfigureModels(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }




    }
}

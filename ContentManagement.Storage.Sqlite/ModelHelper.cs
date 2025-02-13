using ContentManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentManagement.Storage.Sqlite
{
    internal class ModelHelper : IEntityTypeConfiguration<Document>, IEntityTypeConfiguration<DocumentBlob>, IEntityTypeConfiguration<Drive>, IEntityTypeConfiguration<Models.File>, IEntityTypeConfiguration<Models.Directory>, IEntityTypeConfiguration<FileDuplicates>
    {
        private const string NewLine = "\n";
        private const string CrLf = "\r\n";
        private const string Comma = ",";
        private const char EqualSign = '=';

        public void ConfigureModels(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("NOCASE");
            Configure(modelBuilder.Entity<Drive>());
            Configure(modelBuilder.Entity<Models.Directory>());
            Configure(modelBuilder.Entity<Models.File>());
            Configure(modelBuilder.Entity<FileDuplicates>());
            Configure(modelBuilder.Entity<Document>());
            Configure(modelBuilder.Entity<DocumentBlob>());
        }

        /// <summary>Drive Depends on Nothing</summary>
        public void Configure(EntityTypeBuilder<Drive> builder)
        {
            string tableName = nameof(IContentStore.Drives);

            builder
                .ToTable(tableName)
                .HasKey(x => x.DriveId)
                .HasName("PK_" + tableName);

            builder.HasIndex(x => x.VolumeSerialNumber).IsUnique();

            builder
                .Property(t => t.MachineNames).HasConversion(
                        names => names.Join(Comma),
                        text => text.ToHashSet(Comma));

            builder
                .Property(t => t.Tags).HasConversion(
                        tags => tags.Join(Comma),
                        text => text.ToHashSet(Comma));

            // serialize with only a newline character, but deserialize with carriage return and newline (either)
            builder.Property(t => t.Properties).HasConversion(
                        props => props.AsText(NewLine, EqualSign),
                        text => text.ToDictionary(CrLf, EqualSign, true));

            builder.Property(t => t.DriveType).IsRequired();
            builder.Property(t => t.DriveLetter).IsRequired();
        }

        /// <summary>Directory Depends on Drive</summary>
        public void Configure(EntityTypeBuilder<Models.Directory> builder)
        {
            string tableName = nameof(IContentStore.Directories);

            builder
                .ToTable(tableName)
                .HasKey(x => x.DirectoryId)
                .HasName("PK_" + tableName);

            // directory dependant on drive
            builder.HasOne(x => x.Drive)
                .WithMany(x => x.Directories)
                .HasForeignKey(x => x.DriveId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(true);

            builder.Navigation(x => x.Drive).IsRequired();

            builder
                .Property(t => t.Tags).HasConversion(
                        tags => tags.Join(Comma),
                        text => text.ToHashSet(Comma));

            // serialize with only a newline character, but deserialize with carriage return and newline (either)
            builder.Property(t => t.Properties).HasConversion(
                        props => props.AsText(NewLine, EqualSign),
                        text => text.ToDictionary(CrLf, EqualSign, true));

        }

        /// <summary>FileStorage Depends on Directory (0..1) that Depends On Drive (1..1)</summary>
        public void Configure(EntityTypeBuilder<Models.File> builder)
        {
            string tableName = nameof(IContentStore.Files);

            builder
                .ToTable(tableName)
                .HasKey(x => x.FileId)
                .HasName("PK_" + tableName);

            builder.HasIndex(x => new { x.DirectoryPath, x.FileName, x.DriveId }).IsUnique(true);
            builder.HasIndex(x => x.FileName).IsUnique(false);
            builder.HasIndex(x => x.HashCode).IsUnique(false);
            builder.HasIndex(x => x.Crc32).IsUnique(false);


            // File Duplicates can link two files together.
            builder.HasMany<FileDuplicates>().WithOne(x => x.First).HasForeignKey(x => x.FirstId);
            builder.HasMany<FileDuplicates>().WithOne(x => x.Second).HasForeignKey(x => x.SecondId);

            // serialize with only a newline character, but deserialize with carriage return and newline (either)
            builder
                .Property(t => t.Tags).HasConversion(
                        tags => tags.Join(Comma),
                        text => text.ToHashSet(Comma));

            builder.Property(t => t.Properties).HasConversion(
                        props => props.AsText(NewLine, EqualSign),
                        text => text.ToDictionary(CrLf, EqualSign, true));
        }

        /// <summary>FileDuplicates Depends on FileStorages (2..2)</summary>
        public void Configure(EntityTypeBuilder<FileDuplicates> builder)
        {
            string tableName = nameof(IContentStore.FileDuplicates);

            builder
                .ToTable(tableName, t=> t.HasCheckConstraint($"Ck_{tableName}_IdOrder", $"{nameof(FileDuplicates.FirstId)} < {nameof(FileDuplicates.SecondId)}"))
                .HasKey(x => new{x.FirstId, x.SecondId})
                .HasName("PK_" + tableName);

            builder.HasIndex(x => x.SecondId);

            builder.Navigation(e => e.First).IsRequired();
            builder.Navigation(e => e.Second).IsRequired();

            builder
                .Property(t => t.Tags).HasConversion(
                        tags => tags.Join(Comma),
                        text => text.ToHashSet(Comma));

            // serialize with only a newline character, but deserialize with carriage return and newline (either)
            builder.Property(t => t.Properties).HasConversion(
                        props => props.AsText(NewLine, EqualSign),
                        text => text.ToDictionary(CrLf, EqualSign, true));
        }

        /// <summary>Document Depends on FilesStorages (0..n)</summary>
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            string tableName = nameof(IContentStore.Documents);
            builder
                .ToTable(tableName)
                .HasKey(x => x.DocumentGuid)
                .HasName("PK_" + tableName);

            builder
                .HasMany(t => t.Blobs)
                .WithOne(t => t.Document)
                .HasPrincipalKey(x => x.DocumentGuid)
                .IsRequired(required: false);

            // serialize with only a newline character, but deserialize with carriage return and newline (either)
            builder
                .Property(t => t.Tags).HasConversion(
                        tags => tags.Join(Comma),
                        text => text.ToHashSet(Comma));

            builder.Property(t => t.Properties).HasConversion(
                        props => props.AsText(NewLine, EqualSign),
                        text => text.ToDictionary(CrLf, EqualSign, true));

        }

        /// <summary>Files for a Document</summary>
        public void Configure(EntityTypeBuilder<DocumentBlob> builder)
        {
            string tableName = nameof(IContentStore.Blobs);
            builder
                .ToTable(tableName)
                .HasKey(x => new { x.DocumentGuid, x.BlobNumber})
                .HasName("PK_" + tableName);

            builder
                .HasOne(t => t.Document)
                .WithMany(t => t.Blobs)
                .HasPrincipalKey(x => x.DocumentGuid)
                .IsRequired(required: false);

        }

    }
}

using ContentManagement;
using DocumentManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentManagement.Database.Sqllite
{
    internal class ModelHelper : IEntityTypeConfiguration<Document>, IEntityTypeConfiguration<DocumentBlob>
    {
        private const string NewLine = "\n";
        private const string CrLf = "\r\n";
        private const string Comma = ",";
        private const char EqualSign = '=';

        public void ConfigureModels(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("NOCASE");
            Configure(modelBuilder.Entity<Document>());
            Configure(modelBuilder.Entity<DocumentBlob>());
        }

        /// <summary>Document Depends on FilesStorages (0..n)</summary>
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            string tableName = nameof(IDocumentDatabase.Documents);
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
            string tableName = nameof(IDocumentDatabase.Blobs);
            builder
                .ToTable(tableName)
                .HasKey(x => new { x.DocumentGuid, x.BlobNumber })
                .HasName("PK_" + tableName);

            builder
                .HasOne(t => t.Document)
                .WithMany(t => t.Blobs)
                .HasPrincipalKey(x => x.DocumentGuid)
                .IsRequired(required: false);

        }

    }
}

﻿// <auto-generated />
using System;
using ContentManagement.Storage.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ContentManagement.Storage.Sqlite.Migrations
{
    [DbContext(typeof(ContentStoreContext))]
    [Migration("20250212015259_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("NOCASE")
                .HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("ContentManagement.Models.Directory", b =>
                {
                    b.Property<int>("DirectoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DirectoryPath")
                        .HasColumnType("TEXT");

                    b.Property<int>("DriveId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("DriveLetter")
                        .HasColumnType("TEXT");

                    b.Property<bool?>("Exists")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("IsBackupLocation")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("ShouldMonitorFiles")
                        .HasColumnType("INTEGER");

                    b.HasKey("DirectoryId")
                        .HasName("PK_Directories");

                    b.HasIndex("DriveId");

                    b.ToTable("Directories", (string)null);
                });

            modelBuilder.Entity("ContentManagement.Models.Document", b =>
                {
                    b.Property<Guid>("DocumentGuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DateCreatedUtc")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DateRetention")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DateUpdatedUtc")
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT");

                    b.Property<string>("Properties")
                        .HasColumnType("TEXT");

                    b.Property<string>("Tags")
                        .HasColumnType("TEXT");

                    b.HasKey("DocumentGuid")
                        .HasName("PK_Documents");

                    b.ToTable("Documents", (string)null);
                });

            modelBuilder.Entity("ContentManagement.Models.DocumentBlob", b =>
                {
                    b.Property<Guid>("DocumentGuid")
                        .HasColumnType("TEXT");

                    b.Property<ushort>("BlobNumber")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("CanReadData")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("CanWriteData")
                        .HasColumnType("INTEGER");

                    b.Property<uint?>("Crc32")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Data")
                        .HasColumnType("BLOB");

                    b.Property<Guid?>("DocumentGuid1")
                        .HasColumnType("TEXT");

                    b.Property<string>("FileExtension")
                        .HasColumnType("TEXT");

                    b.Property<string>("HashCode")
                        .HasColumnType("TEXT");

                    b.Property<int?>("Length")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MimeType")
                        .HasColumnType("TEXT");

                    b.HasKey("DocumentGuid", "BlobNumber")
                        .HasName("PK_Blobs");

                    b.HasIndex("DocumentGuid1");

                    b.ToTable("Blobs", (string)null);
                });

            modelBuilder.Entity("ContentManagement.Models.Drive", b =>
                {
                    b.Property<int>("DriveId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("DateCreatedUtc")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DateUpdatedUtc")
                        .HasColumnType("TEXT");

                    b.Property<string>("DriveFormat")
                        .HasColumnType("TEXT");

                    b.Property<string>("DriveLetter")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("DriveType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("HardwareSerialNumber")
                        .HasColumnType("TEXT");

                    b.Property<string>("MachineNames")
                        .HasColumnType("TEXT");

                    b.Property<string>("Model")
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT");

                    b.Property<string>("Tags")
                        .HasColumnType("TEXT");

                    b.Property<long?>("TotalFreeSpace")
                        .HasColumnType("INTEGER");

                    b.Property<long>("TotalSize")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VolumeLabel")
                        .HasColumnType("TEXT");

                    b.Property<string>("VolumeSerialNumber")
                        .HasColumnType("TEXT");

                    b.HasKey("DriveId")
                        .HasName("PK_Drives");

                    b.HasIndex("VolumeSerialNumber")
                        .IsUnique();

                    b.ToTable("Drives", (string)null);
                });

            modelBuilder.Entity("ContentManagement.Models.File", b =>
                {
                    b.Property<int>("FileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Attributes")
                        .HasColumnType("INTEGER");

                    b.Property<uint?>("Crc32")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("DateCreatedUtc")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DateRetention")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DateUpdatedUtc")
                        .HasColumnType("TEXT");

                    b.Property<string>("DirectoryPath")
                        .HasColumnType("TEXT");

                    b.Property<int?>("DriveId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("DriveLetter")
                        .HasColumnType("TEXT");

                    b.Property<bool?>("Exists")
                        .HasColumnType("INTEGER");

                    b.Property<string>("HashCode")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsReadOnly")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("Length")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT");

                    b.Property<string>("Properties")
                        .HasColumnType("TEXT");

                    b.Property<string>("Tags")
                        .HasColumnType("TEXT");

                    b.HasKey("FileId")
                        .HasName("PK_Files");

                    b.HasIndex("Crc32");

                    b.HasIndex("HashCode");

                    b.HasIndex("Name");

                    b.HasIndex("DirectoryPath", "Name", "DriveId")
                        .IsUnique();

                    b.ToTable("Files", (string)null);
                });

            modelBuilder.Entity("ContentManagement.Models.FileDuplicates", b =>
                {
                    b.Property<int>("FirstId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SecondId")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("AreDuplicates")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("FirstIsABackup")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT");

                    b.Property<bool?>("SecondIsABackup")
                        .HasColumnType("INTEGER");

                    b.HasKey("FirstId", "SecondId")
                        .HasName("PK_FileDuplicates");

                    b.HasIndex("SecondId");

                    b.ToTable("FileDuplicates", null, t =>
                        {
                            t.HasCheckConstraint("Ck_FileDuplicates_IdOrder", "FirstId < SecondId");
                        });
                });

            modelBuilder.Entity("ContentManagement.Models.Directory", b =>
                {
                    b.HasOne("ContentManagement.Models.Drive", "Drive")
                        .WithMany("Directories")
                        .HasForeignKey("DriveId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Drive");
                });

            modelBuilder.Entity("ContentManagement.Models.DocumentBlob", b =>
                {
                    b.HasOne("ContentManagement.Models.Document", "Document")
                        .WithMany("Blobs")
                        .HasForeignKey("DocumentGuid1");

                    b.Navigation("Document");
                });

            modelBuilder.Entity("ContentManagement.Models.FileDuplicates", b =>
                {
                    b.HasOne("ContentManagement.Models.File", "First")
                        .WithMany()
                        .HasForeignKey("FirstId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ContentManagement.Models.File", "Second")
                        .WithMany()
                        .HasForeignKey("SecondId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("First");

                    b.Navigation("Second");
                });

            modelBuilder.Entity("ContentManagement.Models.Document", b =>
                {
                    b.Navigation("Blobs");
                });

            modelBuilder.Entity("ContentManagement.Models.Drive", b =>
                {
                    b.Navigation("Directories");
                });
#pragma warning restore 612, 618
        }
    }
}

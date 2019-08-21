﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ntbs_service.Models;

namespace ntbs_service.Migrations
{
    [DbContext(typeof(NtbsContext))]
    partial class NtbsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ntbs_service.Models.Country", b =>
                {
                    b.Property<int>("CountryId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .HasMaxLength(200);

                    b.HasKey("CountryId");

                    b.ToTable("Country");

                    b.HasData(
                        new
                        {
                            CountryId = 1,
                            Name = "United Kingdom"
                        },
                        new
                        {
                            CountryId = 2,
                            Name = "Unknown"
                        },
                        new
                        {
                            CountryId = 3,
                            Name = "Other"
                        });
                });

            modelBuilder.Entity("ntbs_service.Models.Ethnicity", b =>
                {
                    b.Property<int>("EthnicityId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Code")
                        .HasMaxLength(50);

                    b.Property<string>("Label")
                        .HasMaxLength(200);

                    b.Property<int>("Order");

                    b.HasKey("EthnicityId");

                    b.ToTable("Ethnicity");

                    b.HasData(
                        new
                        {
                            EthnicityId = 1,
                            Code = "A",
                            Label = "White British",
                            Order = 16
                        },
                        new
                        {
                            EthnicityId = 2,
                            Code = "B",
                            Label = "White Irish",
                            Order = 17
                        },
                        new
                        {
                            EthnicityId = 3,
                            Code = "C",
                            Label = "Any other White background",
                            Order = 3
                        },
                        new
                        {
                            EthnicityId = 4,
                            Code = "D",
                            Label = "Mixed - White and Black Caribbean",
                            Order = 14
                        },
                        new
                        {
                            EthnicityId = 5,
                            Code = "E",
                            Label = "Mixed - White and Black African",
                            Order = 13
                        },
                        new
                        {
                            EthnicityId = 6,
                            Code = "F",
                            Label = "Mixed - White and Asian",
                            Order = 12
                        },
                        new
                        {
                            EthnicityId = 7,
                            Code = "G",
                            Label = "Any other mixed background",
                            Order = 9
                        },
                        new
                        {
                            EthnicityId = 8,
                            Code = "H",
                            Label = "Indian",
                            Order = 1
                        },
                        new
                        {
                            EthnicityId = 9,
                            Code = "J",
                            Label = "Pakistani",
                            Order = 2
                        },
                        new
                        {
                            EthnicityId = 10,
                            Code = "K",
                            Label = "Bangladeshi",
                            Order = 10
                        },
                        new
                        {
                            EthnicityId = 11,
                            Code = "L",
                            Label = "Any other Asian background",
                            Order = 6
                        },
                        new
                        {
                            EthnicityId = 12,
                            Code = "M",
                            Label = "Black Caribbean",
                            Order = 11
                        },
                        new
                        {
                            EthnicityId = 13,
                            Code = "N",
                            Label = "Black African",
                            Order = 5
                        },
                        new
                        {
                            EthnicityId = 14,
                            Code = "P",
                            Label = "Any other Black Background",
                            Order = 7
                        },
                        new
                        {
                            EthnicityId = 15,
                            Code = "S",
                            Label = "Any other ethnic group",
                            Order = 8
                        },
                        new
                        {
                            EthnicityId = 16,
                            Code = "R",
                            Label = "Chinese",
                            Order = 4
                        },
                        new
                        {
                            EthnicityId = 17,
                            Code = "Z",
                            Label = "Not stated",
                            Order = 15
                        });
                });

            modelBuilder.Entity("ntbs_service.Models.Hospital", b =>
                {
                    b.Property<int>("HospitalId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Label")
                        .HasMaxLength(200);

                    b.HasKey("HospitalId");

                    b.ToTable("Hospital");
                });

            modelBuilder.Entity("ntbs_service.Models.Notification", b =>
                {
                    b.Property<int>("NotificationId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("HospitalId");

                    b.Property<int?>("PatientId");

                    b.HasKey("NotificationId");

                    b.HasIndex("HospitalId");

                    b.HasIndex("PatientId");

                    b.ToTable("Notification");
                });

            modelBuilder.Entity("ntbs_service.Models.Patient", b =>
                {
                    b.Property<int>("PatientId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("CountryId");

                    b.Property<DateTime?>("Dob")
                        .HasColumnType("date");

                    b.Property<int?>("EthnicityId");

                    b.Property<string>("FamilyName")
                        .HasMaxLength(35);

                    b.Property<string>("GivenName")
                        .HasMaxLength(35);

                    b.Property<string>("NhsNumber")
                        .HasMaxLength(10);

                    b.Property<string>("Postcode")
                        .HasMaxLength(50);

                    b.Property<int?>("SexId");

                    b.Property<bool?>("UkBorn");

                    b.HasKey("PatientId");

                    b.HasIndex("CountryId");

                    b.HasIndex("EthnicityId");

                    b.HasIndex("SexId");

                    b.ToTable("Patient");
                });

            modelBuilder.Entity("ntbs_service.Models.Region", b =>
                {
                    b.Property<int>("RegionId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Label")
                        .HasMaxLength(200);

                    b.HasKey("RegionId");

                    b.ToTable("Region");
                });

            modelBuilder.Entity("ntbs_service.Models.Sex", b =>
                {
                    b.Property<int>("SexId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Label")
                        .HasMaxLength(200);

                    b.HasKey("SexId");

                    b.ToTable("Sex");

                    b.HasData(
                        new
                        {
                            SexId = 1,
                            Label = "Male"
                        },
                        new
                        {
                            SexId = 2,
                            Label = "Female"
                        },
                        new
                        {
                            SexId = 3,
                            Label = "Unknown"
                        });
                });

            modelBuilder.Entity("ntbs_service.Models.Notification", b =>
                {
                    b.HasOne("ntbs_service.Models.Hospital", "Hospital")
                        .WithMany()
                        .HasForeignKey("HospitalId")
                        .HasConstraintName("FK_Notification_Hospital");

                    b.HasOne("ntbs_service.Models.Patient", "Patient")
                        .WithMany("Notifications")
                        .HasForeignKey("PatientId")
                        .HasConstraintName("FK_Notification_Patient");
                });

            modelBuilder.Entity("ntbs_service.Models.Patient", b =>
                {
                    b.HasOne("ntbs_service.Models.Country", "Country")
                        .WithMany()
                        .HasForeignKey("CountryId")
                        .HasConstraintName("FK_Patient_Country");

                    b.HasOne("ntbs_service.Models.Ethnicity", "Ethnicity")
                        .WithMany()
                        .HasForeignKey("EthnicityId")
                        .HasConstraintName("FK_Patient_Ethnicity");

                    b.HasOne("ntbs_service.Models.Sex", "Sex")
                        .WithMany()
                        .HasForeignKey("SexId")
                        .HasConstraintName("FK_Patient_Sex");
                });
#pragma warning restore 612, 618
        }
    }
}

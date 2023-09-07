﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Server.Data;

#nullable disable

namespace Server.Migrations
{
    [DbContext(typeof(AirportContext))]
    [Migration("20230816165729_DropColumn")]
    partial class DropColumn
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("LogicModels.BaseModels.Plane", b =>
                {
                    b.Property<int>("PlaneID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PlaneID"));

                    b.Property<int>("AirportLocation")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DepartTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("EntryTime")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeparting")
                        .HasColumnType("bit");

                    b.Property<string>("LocationFrom")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LocationTo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("MovedLocationAirfield")
                        .HasColumnType("bit");

                    b.Property<string>("PlaneCompany")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PlaneID");

                    b.ToTable("FlightHistory");
                });

            modelBuilder.Entity("Server.Models.FlightAndLocation", b =>
                {
                    b.Property<int>("RecordID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RecordID"));

                    b.Property<int>("CurrPlaneID")
                        .HasColumnType("int");

                    b.Property<int>("LocationID")
                        .HasColumnType("int");

                    b.HasKey("RecordID");

                    b.ToTable("FlightsAndLocations");
                });

            modelBuilder.Entity("Server.Models.GeneratedPlanes", b =>
                {
                    b.Property<int>("RecordID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RecordID"));

                    b.Property<int>("GeneratedPlaneID")
                        .HasColumnType("int");

                    b.HasKey("RecordID");

                    b.ToTable("GeneratedPlanes");
                });

            modelBuilder.Entity("Server.Models.WaitingFlight", b =>
                {
                    b.Property<int>("RecordID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RecordID"));

                    b.Property<int>("CurrPlaneID")
                        .HasColumnType("int");

                    b.HasKey("RecordID");

                    b.ToTable("WaitingFlight");
                });
#pragma warning restore 612, 618
        }
    }
}

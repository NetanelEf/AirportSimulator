using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlightHistory",
                columns: table => new
                {
                    PlaneID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlaneCompany = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocationFrom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocationTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AirportLocation = table.Column<int>(type: "int", nullable: false),
                    MovedLocationAirfield = table.Column<bool>(type: "bit", nullable: false),
                    IsDeparting = table.Column<bool>(type: "bit", nullable: false),
                    EntryTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightHistory", x => x.PlaneID);
                });

            migrationBuilder.CreateTable(
                name: "FlightsAndLocations",
                columns: table => new
                {
                    RecordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrPlaneID = table.Column<int>(type: "int", nullable: false),
                    LocationID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightsAndLocations", x => x.RecordID);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedPlanes",
                columns: table => new
                {
                    RecordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GeneratedPlaneID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedPlanes", x => x.RecordID);
                });

            migrationBuilder.CreateTable(
                name: "WaitingFlight",
                columns: table => new
                {
                    RecordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrPlaneID = table.Column<int>(type: "int", nullable: false),
                    LocationID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaitingFlight", x => x.RecordID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlightHistory");

            migrationBuilder.DropTable(
                name: "FlightsAndLocations");

            migrationBuilder.DropTable(
                name: "GeneratedPlanes");

            migrationBuilder.DropTable(
                name: "WaitingFlight");
        }
    }
}

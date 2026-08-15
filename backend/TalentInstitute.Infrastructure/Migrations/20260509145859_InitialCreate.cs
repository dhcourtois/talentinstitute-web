using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentInstitute.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alumnos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Grado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BalanceMeritos = table.Column<int>(type: "int", nullable: false),
                    Privilegio_Oficina = table.Column<bool>(type: "bit", nullable: false),
                    Privilegio_Comedor = table.Column<bool>(type: "bit", nullable: false),
                    Privilegio_Patio = table.Column<bool>(type: "bit", nullable: false),
                    Privilegio_Biblioteca = table.Column<bool>(type: "bit", nullable: false),
                    Privilegio_Actividades = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alumnos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntrevistasPadres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaEntrevista = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NombrePadre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NumeroHijos = table.Column<int>(type: "int", nullable: false),
                    RiesgoViolencia = table.Column<bool>(type: "bit", nullable: false),
                    RiesgoDivorcio = table.Column<bool>(type: "bit", nullable: false),
                    ConoceADios = table.Column<bool>(type: "bit", nullable: false),
                    Comentarios = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Aceptado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntrevistasPadres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Paces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Materia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    PuntajeMaximo = table.Column<int>(type: "int", nullable: false),
                    PuntajeMinimoAprobacion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlumnoPaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Materia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlumnoPaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlumnoPaces_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlumnoPaces_Paces_PaceId",
                        column: x => x.PaceId,
                        principalTable: "Paces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlumnoPaces_AlumnoId",
                table: "AlumnoPaces",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_AlumnoPaces_PaceId",
                table: "AlumnoPaces",
                column: "PaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlumnoPaces");

            migrationBuilder.DropTable(
                name: "EntrevistasPadres");

            migrationBuilder.DropTable(
                name: "Alumnos");

            migrationBuilder.DropTable(
                name: "Paces");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentInstitute.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPortalPadresFamilia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PadresFamilia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PadresFamilia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PadresAlumnos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PadreFamiliaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaVinculo = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PadresAlumnos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PadresAlumnos_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PadresAlumnos_PadresFamilia_PadreFamiliaId",
                        column: x => x.PadreFamiliaId,
                        principalTable: "PadresFamilia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PadresAlumnos_AlumnoId",
                table: "PadresAlumnos",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_PadresAlumnos_PadreFamiliaId_AlumnoId",
                table: "PadresAlumnos",
                columns: new[] { "PadreFamiliaId", "AlumnoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PadresFamilia_Email",
                table: "PadresFamilia",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PadresAlumnos");

            migrationBuilder.DropTable(
                name: "PadresFamilia");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentInstitute.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoreEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Grado",
                table: "Alumnos",
                newName: "NumeroMatricula");

            migrationBuilder.AddColumn<string>(
                name: "Apellido",
                table: "Alumnos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nivel",
                table: "Alumnos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Alumnos",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCompletado",
                table: "AlumnoPaces",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicio",
                table: "AlumnoPaces",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "PuntajeFinal",
                table: "AlumnoPaces",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AlumnoPaces",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "ConfiguracionPrivilegios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StaffIdActualizo = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UmbralOficina = table.Column<int>(type: "int", nullable: false),
                    UmbralOficinaRevocado = table.Column<int>(type: "int", nullable: false),
                    UmbralComedor = table.Column<int>(type: "int", nullable: false),
                    UmbralComedorRevocado = table.Column<int>(type: "int", nullable: false),
                    UmbralPatio = table.Column<int>(type: "int", nullable: false),
                    UmbralPatioRevocado = table.Column<int>(type: "int", nullable: false),
                    UmbralBiblioteca = table.Column<int>(type: "int", nullable: false),
                    UmbralBibliotecaRevocado = table.Column<int>(type: "int", nullable: false),
                    UmbralActividades = table.Column<int>(type: "int", nullable: false),
                    UmbralActividadesRevocado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionPrivilegios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Metas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlumnoPaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Turno = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaginasObjetivo = table.Column<int>(type: "int", nullable: false),
                    FechaObjetivo = table.Column<DateOnly>(type: "date", nullable: false),
                    PuntajeObtenido = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Metas_AlumnoPaces_AlumnoPaceId",
                        column: x => x.AlumnoPaceId,
                        principalTable: "AlumnoPaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Meritos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Puntos = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    FechaAplicado = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Revocado = table.Column<bool>(type: "bit", nullable: false),
                    StaffIdRevoco = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaRevocacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meritos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Meritos_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Meritos_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Meritos_AlumnoId",
                table: "Meritos",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Meritos_StaffId",
                table: "Meritos",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Metas_AlumnoPaceId",
                table: "Metas",
                column: "AlumnoPaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_Email",
                table: "Staff",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionPrivilegios");

            migrationBuilder.DropTable(
                name: "Meritos");

            migrationBuilder.DropTable(
                name: "Metas");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropColumn(
                name: "Apellido",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "Nivel",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "FechaCompletado",
                table: "AlumnoPaces");

            migrationBuilder.DropColumn(
                name: "FechaInicio",
                table: "AlumnoPaces");

            migrationBuilder.DropColumn(
                name: "PuntajeFinal",
                table: "AlumnoPaces");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AlumnoPaces");

            migrationBuilder.RenameColumn(
                name: "NumeroMatricula",
                table: "Alumnos",
                newName: "Grado");
        }
    }
}

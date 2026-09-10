using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentInstitute.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivilegiosManuales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PrivilegioManual_Actividades",
                table: "Alumnos",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PrivilegioManual_Biblioteca",
                table: "Alumnos",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PrivilegioManual_Comedor",
                table: "Alumnos",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PrivilegioManual_Oficina",
                table: "Alumnos",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PrivilegioManual_Patio",
                table: "Alumnos",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivilegioManual_Actividades",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "PrivilegioManual_Biblioteca",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "PrivilegioManual_Comedor",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "PrivilegioManual_Oficina",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "PrivilegioManual_Patio",
                table: "Alumnos");
        }
    }
}

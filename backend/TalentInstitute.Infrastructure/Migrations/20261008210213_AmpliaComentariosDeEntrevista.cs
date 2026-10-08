using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentInstitute.Infrastructure.Migrations
{
    /// <summary>
    /// Amplía los comentarios de entrevista de 1000 a 4000 caracteres.
    ///
    /// Con el tope anterior, una nota de más de 1000 caracteres pasaba la
    /// validación del dominio —que no miraba el largo— y reventaba en la base
    /// como error 500. Mil caracteres son poco para una entrevista de admisión,
    /// que recoge contexto familiar, factores de riesgo y seguimientos.
    ///
    /// Ampliar una columna de texto es una operación en sitio y sin pérdida.
    /// El `Down` sí puede perder datos: si existen notas de más de 1000
    /// caracteres, volver atrás las trunca.
    /// </summary>
    public partial class AmpliaComentariosDeEntrevista : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Comentarios",
                table: "EntrevistasPadres",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Comentarios",
                table: "EntrevistasPadres",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);
        }
    }
}

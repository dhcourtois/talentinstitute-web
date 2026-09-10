using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentInstitute.Infrastructure.Migrations
{
    /// <summary>
    /// Convierte la meta de "avanzar N páginas" a un rango "de la página X a la Y"
    /// (issue #6), y agrega el total de páginas al PACE del catálogo.
    ///
    /// Escrita a mano. El andamiaje generado renombraba `PaginasObjetivo` a
    /// `PaginaInicial`, lo que habría convertido una meta de 5 páginas en una que
    /// empieza en la página 5 y termina en la 0 — dato corrupto y, además,
    /// inválido según la invariante `PaginaFinal >= PaginaInicial`.
    /// </summary>
    public partial class AddRangoDePaginasEnMeta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalPaginas",
                table: "Paces",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaginaInicial",
                table: "Metas",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PaginaFinal",
                table: "Metas",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // Las metas existentes decían "avanzar N páginas" sin indicar desde
            // dónde. Se reinterpretan como el rango 1..N, que conserva la
            // cantidad de páginas, que es el único dato que de verdad había.
            // El CASE protege contra filas con un valor fuera de rango: sin él
            // quedarían con PaginaFinal 0 y violarían la invariante.
            migrationBuilder.Sql(@"
                UPDATE [Metas]
                SET [PaginaInicial] = 1,
                    [PaginaFinal]   = CASE WHEN [PaginasObjetivo] < 1 THEN 1 ELSE [PaginasObjetivo] END;
            ");

            migrationBuilder.DropColumn(
                name: "PaginasObjetivo",
                table: "Metas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaginasObjetivo",
                table: "Metas",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // Al revertir se conserva la cantidad de páginas del rango, que es
            // lo único que el modelo anterior sabía representar. La página de
            // inicio se pierde: el esquema viejo no tiene dónde guardarla.
            migrationBuilder.Sql(@"
                UPDATE [Metas]
                SET [PaginasObjetivo] = [PaginaFinal] - [PaginaInicial] + 1;
            ");

            migrationBuilder.DropColumn(
                name: "PaginaFinal",
                table: "Metas");

            migrationBuilder.DropColumn(
                name: "PaginaInicial",
                table: "Metas");

            migrationBuilder.DropColumn(
                name: "TotalPaginas",
                table: "Paces");
        }
    }
}

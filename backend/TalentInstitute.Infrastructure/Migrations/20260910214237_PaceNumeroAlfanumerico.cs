using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentInstitute.Infrastructure.Migrations
{
    /// <summary>
    /// El número de PACE pasa de entero a texto: además de los cuadernillos
    /// numerados (1045, 1046) el colegio maneja códigos con letras como RR01.
    ///
    /// Se traduce a un `ALTER COLUMN` en sitio. SQL Server convierte
    /// `int → nvarchar` de forma implícita y sin pérdida, así que no hace falta
    /// backfill: los PACEs existentes quedan como '1025', '1045', etc.
    /// </summary>
    public partial class PaceNumeroAlfanumerico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Numero",
                table: "Paces",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <summary>
        /// Revertir solo es seguro mientras todos los números sigan siendo
        /// dígitos. En cuanto exista un PACE como RR01, esta conversión falla:
        /// no hay entero al que llevarlo, y el esquema anterior no tiene dónde
        /// guardarlo. Sería necesario decidir primero qué hacer con esas filas.
        /// </summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Numero",
                table: "Paces",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Inventario.API.Migrations
{
    /// <inheritdoc />
    public partial class EstructuraFinalConCedula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Marca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarcaVehiculo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marca", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Cedula = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: true),
                    Correo = table.Column<string>(type: "text", nullable: true),
                    Contraseña = table.Column<string>(type: "text", nullable: true),
                    Rol = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Cedula);
                });

            migrationBuilder.CreateTable(
                name: "Parabrisas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarcaId = table.Column<int>(type: "integer", nullable: false),
                    Modelo = table.Column<string>(type: "text", nullable: true),
                    Anio = table.Column<string>(type: "text", nullable: true),
                    Tipo = table.Column<string>(type: "text", nullable: true),
                    Precio = table.Column<double>(type: "double precision", nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    Ubicación = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parabrisas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parabrisas_Marca_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "Marca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parabrisas_MarcaId",
                table: "Parabrisas",
                column: "MarcaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Parabrisas");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Marca");
        }
    }
}

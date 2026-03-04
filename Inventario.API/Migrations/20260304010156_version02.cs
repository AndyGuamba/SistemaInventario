using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventario.API.Migrations
{
    /// <inheritdoc />
    public partial class version02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parabrisas_Marca_MarcaId",
                table: "Parabrisas");

            migrationBuilder.AlterColumn<int>(
                name: "MarcaId",
                table: "Parabrisas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Marca",
                table: "Parabrisas",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Parabrisas_Marca_MarcaId",
                table: "Parabrisas",
                column: "MarcaId",
                principalTable: "Marca",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parabrisas_Marca_MarcaId",
                table: "Parabrisas");

            migrationBuilder.DropColumn(
                name: "Marca",
                table: "Parabrisas");

            migrationBuilder.AlterColumn<int>(
                name: "MarcaId",
                table: "Parabrisas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Parabrisas_Marca_MarcaId",
                table: "Parabrisas",
                column: "MarcaId",
                principalTable: "Marca",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

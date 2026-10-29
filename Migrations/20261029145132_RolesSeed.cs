using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MultiTenantCSharpVargas.Migrations
{
    /// <inheritdoc />
    public partial class RolesSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Activo", "Codigo", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "SUPER_ADMIN", "Super Administrador" },
                    { 2, true, "ADMIN", "Administrador" },
                    { 3, true, "COMERCIANTE", "Comerciante" },
                    { 4, true, "CLIENTE", "Cliente" }
                });

            // Asigna rol Super Administrador (alcance global, EmpresaId = NULL)
            // al primer usuario registrado, para que exista un superadmin desde el inicio.
            migrationBuilder.Sql(@"
                INSERT INTO `UsuarioRoles` (`UsuarioId`, `RolId`, `EmpresaId`)
                SELECT `Id`, 1, NULL FROM `Usuarios` ORDER BY `Id` LIMIT 1;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}

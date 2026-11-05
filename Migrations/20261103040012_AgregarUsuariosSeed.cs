using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MultiTenantCSharpVargas.Migrations
{
    /// <inheritdoc />
    public partial class AgregarUsuariosSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "ContrasenaHash",
                value: "pbkdf2-sha256$210000$GNnBg2xdU59yCbUaTEKYOw==$jOj6rSsxuMLBYRfhT5CqtpkgiKeFLZn5ydpCcjiJ748=");

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "AvatarUrl", "ContrasenaHash", "Correo", "FechaAlta", "Nombre" },
                values: new object[,]
                {
                    { 2001, null, "pbkdf2-sha256$210000$VqcoBR6CgScnSYI5t7ZGEQ==$qoAJWmanNZegflVRvb5K5zVLTjRVn3LCD1bOUdrPVTk=", "Admin1@test.com", new DateTime(2026, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Administrador" },
                    { 2002, null, "pbkdf2-sha256$210000$L63pRxgtk0IjnQGcaoDF1A==$gxTl2eZdKlFtFEfvOoRo4yXsYJle3MjO2nSEMxlRbHs=", "Comerciante1@test.com", new DateTime(2026, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Comerciante" },
                    { 2003, null, "pbkdf2-sha256$210000$v9lrhnUsQzRx+7pkkWFmiQ==$PXLHlMYJoqEZwCn1puanbLhpSHMklR2WsepDgldaI5g=", "Usuario1@test.com", new DateTime(2026, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Usuario Cliente" }
                });

            migrationBuilder.InsertData(
                table: "UsuarioRoles",
                columns: new[] { "Id", "EmpresaId", "RolId", "UsuarioId" },
                values: new object[,]
                {
                    { 2001, null, 2, 2001 },
                    { 2002, null, 3, 2002 },
                    { 2003, null, 4, 2003 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UsuarioRoles",
                keyColumn: "Id",
                keyValue: 2001);

            migrationBuilder.DeleteData(
                table: "UsuarioRoles",
                keyColumn: "Id",
                keyValue: 2002);

            migrationBuilder.DeleteData(
                table: "UsuarioRoles",
                keyColumn: "Id",
                keyValue: 2003);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2001);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2002);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2003);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1000,
                column: "ContrasenaHash",
                value: "ED420D51779BD4722D41DB8203B70893CBB95582BF3E8C337492E8E2ABFDB483");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Appointments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueBusinessHourPerDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No borramos IX_business_hours_BusinessId porque MariaDB lo usa
            // para mantener la foreign key entre business_hours y businesses.
            //
            // En su lugar, agregamos un índice único adicional para evitar que
            // el mismo negocio tenga dos horarios configurados para el mismo día.
            migrationBuilder.CreateIndex(
                name: "IX_business_hours_BusinessId_DayOfWeek",
                table: "business_hours",
                columns: new[] { "BusinessId", "DayOfWeek" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Si revertimos esta migración, solo quitamos el índice único nuevo.
            // No recreamos IX_business_hours_BusinessId porque nunca lo borramos.
            migrationBuilder.DropIndex(
                name: "IX_business_hours_BusinessId_DayOfWeek",
                table: "business_hours");
        }
    }
}

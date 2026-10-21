using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GuessedEventsBecameActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // events used to stay in guessing (2) after every game was guessed, now they become active (4) at that moment
            migrationBuilder.Sql(@"
                UPDATE Events e
                SET e.StatusId = 4
                WHERE e.StatusId = 2
                    AND EXISTS (SELECT 1 FROM EventRecords er WHERE er.EventId = e.Id)
                    AND NOT EXISTS (SELECT 1 FROM EventRecords er WHERE er.EventId = e.Id AND er.GameId IS NULL);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // the old status can't be told apart from events that became active the normal way, so nothing is reverted
        }
    }
}

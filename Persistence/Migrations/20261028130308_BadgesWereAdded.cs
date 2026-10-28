using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BadgesWereAdded : Migration
    {
        // the two badges that came with the site as images of the front (src/assets/Achievements)
        private const string KnightSvg = """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 128 128" width="128" height="128" role="img">
              <title>Knight</title>
              <defs>
                <clipPath id="knight-badge-inner">
                  <circle cx="64" cy="64" r="56"/>
                </clipPath>
                <clipPath id="knight-badge-shadow">
                  <rect x="64" y="0" width="64" height="128"/>
                </clipPath>
                <!-- light shining from behind the head, fading towards the edge -->
                <radialGradient id="knight-badge-glow" cx="64" cy="58" r="56" gradientUnits="userSpaceOnUse">
                  <stop offset="0" stop-color="#fff6c8" stop-opacity="1"/>
                  <stop offset="0.6" stop-color="#ffde59" stop-opacity="0.9"/>
                  <stop offset="0.85" stop-color="#f2c200" stop-opacity="0.35"/>
                  <stop offset="1" stop-color="#e1b800" stop-opacity="0"/>
                </radialGradient>
                <radialGradient id="knight-badge-rays" cx="64" cy="58" r="58" gradientUnits="userSpaceOnUse">
                  <stop offset="0.5" stop-color="#fff3b0" stop-opacity="0.95"/>
                  <stop offset="1" stop-color="#ffd84d" stop-opacity="0"/>
                </radialGradient>
              </defs>
              <circle cx="64" cy="64" r="62" fill="#e1b800"/>
              <circle cx="64" cy="64" r="56" fill="#3d3d3d"/>
              <g clip-path="url(#knight-badge-inner)">
                <!-- radiance behind the helmet -->
                <circle cx="64" cy="58" r="56" fill="url(#knight-badge-glow)"/>
                <path fill="url(#knight-badge-rays)" d="M64 58 L57.9 -11.7 L70.1 -11.7 Z M64 58 L85.0 -8.8 L96.3 -4.1 Z M64 58 L109.0 4.4 L117.6 13.0 Z M64 58 L126.1 25.7 L130.8 37.0 Z M64 58 L133.7 51.9 L133.7 64.1 Z M64 58 L130.8 79.0 L126.1 90.3 Z M64 58 L117.6 103.0 L109.0 111.6 Z M64 58 L96.3 120.1 L85.0 124.8 Z M64 58 L70.1 127.7 L57.9 127.7 Z M64 58 L43.0 124.8 L31.7 120.1 Z M64 58 L19.0 111.6 L10.4 103.0 Z M64 58 L1.9 90.3 L-2.8 79.0 Z M64 58 L-5.7 64.1 L-5.7 51.9 Z M64 58 L-2.8 37.0 L1.9 25.7 Z M64 58 L10.4 13.0 L19.0 4.4 Z M64 58 L31.7 -4.1 L43.0 -8.8 Z"/>
                <!-- neck flare under the helmet, cut by the edge of the badge -->
                <path d="M22 128 L34 106 L64 114 L94 106 L106 128 Z" fill="#8d97a4"/>
                <!-- faceted helm with a ridge in the middle, the right half is in shadow -->
                <path d="M64 14 L88 24 L100 46 L102 76 L92 98 L64 114 L36 98 L26 76 L28 46 L40 24 Z" fill="#c3cad3"/>
                <path d="M64 14 L88 24 L100 46 L102 76 L92 98 L64 114 L36 98 L26 76 L28 46 L40 24 Z" fill="#aab3be" clip-path="url(#knight-badge-shadow)"/>
                <!-- plume on the top of the ridge -->
                <path d="M63 19 L85 11 L75 19 L68 24 Z" fill="#be2908"/>
                <!-- brow -->
                <path d="M27 47 L64 55 L101 47 L101 52 L64 60 L27 52 Z" fill="#9aa4b1"/>
                <!-- slots cut through the face plate -->
                <g fill="#1a1a1a">
                  <path d="M40 60 L41.9 65 V83 L40 88 L38.1 83 V65 Z"/>
                  <path d="M48 62 L49.9 67 V88 L48 93 L46.1 88 V67 Z"/>
                  <path d="M56 63.5 L57.9 68.5 V92 L56 97 L54.1 92 V68.5 Z"/>
                  <path d="M64 65 L65.9 70 V95 L64 100 L62.1 95 V70 Z"/>
                  <path d="M72 63.5 L73.9 68.5 V92 L72 97 L70.1 92 V68.5 Z"/>
                  <path d="M80 62 L81.9 67 V88 L80 93 L78.1 88 V67 Z"/>
                  <path d="M88 60 L89.9 65 V83 L88 88 L86.1 83 V65 Z"/>
                </g>
              </g>
            </svg>
            """;

        private const string WheelSvg = """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 128 128" width="128" height="128" role="img">
              <title>Wheel</title>
              <defs>
                <clipPath id="wheel-badge-inner">
                  <circle cx="64" cy="64" r="56"/>
                </clipPath>
                <clipPath id="wheel-badge-shadow">
                  <rect x="64" y="0" width="64" height="128"/>
                </clipPath>
                <!-- shapes are drawn twice: lit, then the right half in shadow -->
                <g id="wheel-badge-wood">
                  <path fill-rule="evenodd" d="M64 21.5 L73.6 22.8 L82.5 26.5 L90.2 32.3 L96 40 L99.7 48.9 L101 58.5 L99.7 68.1 L96 77 L90.2 84.7 L82.5 90.5 L73.6 94.2 L64 95.5 L54.4 94.2 L45.5 90.5 L37.8 84.7 L32 77 L28.3 68.1 L27 58.5 L28.3 48.9 L32 40 L37.8 32.3 L45.5 26.5 L54.4 22.8 Z M64 28 L71.9 29 L79.2 32.1 L85.6 36.9 L90.4 43.2 L93.5 50.6 L94.5 58.5 L93.5 66.4 L90.4 73.8 L85.6 80.1 L79.2 84.9 L71.9 88 L64 89 L56.1 88 L48.8 84.9 L42.4 80.1 L37.6 73.8 L34.5 66.4 L33.5 58.5 L34.5 50.6 L37.6 43.2 L42.4 36.9 L48.7 32.1 L56.1 29 Z"/>
                  <path d="M66.4 50.5 L65.4 26.5 L62.6 26.5 L61.6 50.5 Z M70.1 52.8 L81.2 31.5 L78.8 30.1 L65.9 50.4 Z M72.1 56.6 L92.4 43.7 L91 41.3 L69.7 52.4 Z M72 60.9 L96 59.9 L96 57.1 L72 56.1 Z M69.7 64.6 L91 75.7 L92.4 73.3 L72.1 60.4 Z M65.9 66.6 L78.8 86.9 L81.2 85.5 L70.1 64.2 Z M61.6 66.5 L62.6 90.5 L65.4 90.5 L66.4 66.5 Z M57.9 64.2 L46.8 85.5 L49.2 86.9 L62.1 66.6 Z M55.9 60.4 L35.6 73.3 L37 75.7 L58.3 64.6 Z M56 56.1 L32 57.1 L32 59.9 L56 60.9 Z M58.3 52.4 L37 41.3 L35.6 43.7 L55.9 56.6 Z M62.1 50.4 L49.2 30.1 L46.8 31.5 L57.9 52.8 Z"/>
                  <path d="M67.4 50.2 L72.3 55.1 L72.3 61.9 L67.4 66.8 L60.6 66.8 L55.7 61.9 L55.7 55.1 L60.6 50.2 Z"/>
                </g>
                <g id="wheel-badge-steel">
                  <path fill-rule="evenodd" d="M64 17 L74.7 18.4 L84.8 22.6 L93.3 29.2 L99.9 37.8 L104.1 47.8 L105.5 58.5 L104.1 69.2 L99.9 79.2 L93.3 87.8 L84.8 94.4 L74.7 98.6 L64 100 L53.3 98.6 L43.3 94.4 L34.7 87.8 L28.1 79.2 L23.9 69.2 L22.5 58.5 L23.9 47.8 L28.1 37.8 L34.7 29.2 L43.2 22.6 L53.3 18.4 Z M64 21.5 L73.6 22.8 L82.5 26.5 L90.2 32.3 L96 40 L99.7 48.9 L101 58.5 L99.7 68.1 L96 77 L90.2 84.7 L82.5 90.5 L73.6 94.2 L64 95.5 L54.4 94.2 L45.5 90.5 L37.8 84.7 L32 77 L28.3 68.1 L27 58.5 L28.3 48.9 L32 40 L37.8 32.3 L45.5 26.5 L54.4 22.8 Z"/>
                  <path d="M65.9 53.9 L68.6 56.6 L68.6 60.4 L65.9 63.1 L62.1 63.1 L59.4 60.4 L59.4 56.6 L62.1 53.9 Z"/>
                </g>
              </defs>
              <circle cx="64" cy="64" r="62" fill="#e1b800"/>
              <circle cx="64" cy="64" r="56" fill="#3d3d3d"/>
              <g clip-path="url(#wheel-badge-inner)">
                <!-- ground: packed road on top of the soil, a few stones -->
                <path d="M6 98 L34 95.5 L72 96.5 L122 99.5 V128 H6 Z" fill="#6b4d33"/>
                <path d="M6 98 L34 95.5 L72 96.5 L122 99.5 L122 108 L80 106 L40 107 L6 109 Z" fill="#9b7650"/>
                <path d="M18 102 L22 100.5 L25 102.5 L21 104 Z M100 102.5 L104 101 L107 103 L103 104.5 Z" fill="#6b4d33"/>
                <path d="M24 115 L29 112.5 L32 116 L27 117.5 Z M50 120 L53 118 L56 120.5 L52.5 122 Z M92 116 L97 113.5 L100 117 L95 118.5 Z" fill="#8a6846"/>
                <!-- shadow where the wheel touches the road -->
                <path d="M36 100 L64 97.5 L94 100 L64 103 Z" fill="#3a2a1c" opacity="0.8"/>
                <!-- wooden spokes, felloes and hub with an iron tire and hub cap -->
                <use href="#wheel-badge-wood" fill="#b07a45"/>
                <use href="#wheel-badge-wood" fill="#946336" clip-path="url(#wheel-badge-shadow)"/>
                <path d="M71.9 29 L73.6 22.8 M85.6 36.9 L90.2 32.3 M93.5 50.6 L99.7 48.9 M93.5 66.4 L99.7 68.1 M85.6 80.1 L90.2 84.7 M71.9 88 L73.6 94.2 M56.1 88 L54.4 94.2 M42.4 80.1 L37.8 84.7 M34.5 66.4 L28.3 68.1 M34.5 50.6 L28.3 48.9 M42.4 36.9 L37.8 32.3 M56.1 29 L54.4 22.8" stroke="#5e3c1f" stroke-width="1.5"/>
                <use href="#wheel-badge-steel" fill="#c3cad3"/>
                <use href="#wheel-badge-steel" fill="#aab3be" clip-path="url(#wheel-badge-shadow)"/>
                <circle cx="64" cy="58.5" r="1.8" fill="#1a1a1a"/>
              </g>
            </svg>
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<byte[]>(type: "longblob", nullable: false),
                    ContentType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedById = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // the badges of the front keep their ids, so the events that give them and the players who got them keep them too
            var addedAt = new DateTimeOffset(2026, 10, 27, 0, 0, 0, TimeSpan.Zero);
            migrationBuilder.InsertData(
                table: "Badges",
                columns: new[] { "Id", "Name", "Content", "ContentType", "CreatedAt" },
                values: new object[,]
                {
                    { "knight-badge", "Рыцарь", Encoding.UTF8.GetBytes(KnightSvg), "image/svg+xml", addedAt },
                    { "wheel-badge", "Колесо", Encoding.UTF8.GetBytes(WheelSvg), "image/svg+xml", addedAt },
                });

            // an event can't refer to a badge that isn't in the table
            migrationBuilder.Sql("UPDATE GlobalEvents SET BadgeId = NULL WHERE BadgeId NOT IN ('knight-badge', 'wheel-badge');");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalEvents_BadgeId",
                table: "GlobalEvents",
                column: "BadgeId");

            migrationBuilder.AddForeignKey(
                name: "FK_GlobalEvents_Badges_BadgeId",
                table: "GlobalEvents",
                column: "BadgeId",
                principalTable: "Badges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GlobalEvents_Badges_BadgeId",
                table: "GlobalEvents");

            migrationBuilder.DropTable(
                name: "Badges");

            migrationBuilder.DropIndex(
                name: "IX_GlobalEvents_BadgeId",
                table: "GlobalEvents");
        }
    }
}

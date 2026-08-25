using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssignmentDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonnelId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentHistoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReplacesDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentDocuments_AssignmentDocuments_ReplacesDocumentId",
                        column: x => x.ReplacesDocumentId,
                        principalTable: "AssignmentDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignmentDocuments_AssignmentHistories_AssignmentHistoryId",
                        column: x => x.AssignmentHistoryId,
                        principalTable: "AssignmentHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignmentDocuments_Personnel_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentDocuments_AssignmentHistoryId",
                table: "AssignmentDocuments",
                column: "AssignmentHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentDocuments_PersonnelId_UploadedAt",
                table: "AssignmentDocuments",
                columns: new[] { "PersonnelId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentDocuments_ReplacesDocumentId",
                table: "AssignmentDocuments",
                column: "ReplacesDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentDocuments_StoredFileName",
                table: "AssignmentDocuments",
                column: "StoredFileName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentDocuments");
        }
    }
}

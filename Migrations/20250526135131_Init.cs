using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QL_BLOG.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Image_Posted",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId_Category",
                table: "Posts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CategoryId_Category",
                table: "Posts",
                column: "CategoryId_Category");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Categories_CategoryId_Category",
                table: "Posts",
                column: "CategoryId_Category",
                principalTable: "Categories",
                principalColumn: "Id_Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Categories_CategoryId_Category",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CategoryId_Category",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CategoryId_Category",
                table: "Posts");

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "Posts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Image_Posted",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}

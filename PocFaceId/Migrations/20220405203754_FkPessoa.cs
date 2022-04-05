using Microsoft.EntityFrameworkCore.Migrations;

namespace PocFaceId.Migrations
{
    public partial class FkPessoa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Usuario_PessoaId",
                table: "Usuario",
                column: "PessoaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuario_Pessoa_PessoaId",
                table: "Usuario",
                column: "PessoaId",
                principalTable: "Pessoa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuario_Pessoa_PessoaId",
                table: "Usuario");

            migrationBuilder.DropIndex(
                name: "IX_Usuario_PessoaId",
                table: "Usuario");
        }
    }
}

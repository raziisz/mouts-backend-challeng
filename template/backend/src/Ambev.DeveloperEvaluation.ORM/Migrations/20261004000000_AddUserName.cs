using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Ambev.DeveloperEvaluation.ORM;

#nullable disable

namespace Ambev.DeveloperEvaluation.ORM.Migrations;

[DbContext(typeof(DefaultContext))]
[Migration("20261004000000_AddUserName")]
public partial class AddUserName : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Name_Firstname",
            table: "Users",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "Name_Lastname",
            table: "Users",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Name_Firstname",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "Name_Lastname",
            table: "Users");
    }
}

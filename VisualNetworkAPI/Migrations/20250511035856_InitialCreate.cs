using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VisualNetworkAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Board",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    decoration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Created_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    Last_update = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Board__3213E83F1704428E", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Created_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Created_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    Last_update = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Tag__3213E83F8BB67BCC", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    date_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    phone = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    genre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    avatar = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    last_update = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user__3213E83FDA174AEF", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Followers",
                columns: table => new
                {
                    Follower_id = table.Column<int>(type: "int", nullable: false),
                    Followed_id = table.Column<int>(type: "int", nullable: false),
                    Follow_date = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Follower__950CECCBC964DD5D", x => new { x.Follower_id, x.Followed_id });
                    table.ForeignKey(
                        name: "FK__Followers__Follo__3A81B327",
                        column: x => x.Follower_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__Followers__Follo__3B75D760",
                        column: x => x.Followed_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Post",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Json_personalizacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created_by = table.Column<int>(type: "int", nullable: true),
                    Created_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    Last_update = table.Column<DateTime>(type: "datetime", nullable: true),
                    imageUrls = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Post__3213E83F3D1A0E2F", x => x.id);
                    table.ForeignKey(
                        name: "FK__Post__Created_by__3E52440B",
                        column: x => x.Created_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    expiry_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    revoked_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    replaced_by_token = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    session_id = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    device_info = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RefreshT__3213E83F58BFF9A9", x => x.id);
                    table.ForeignKey(
                        name: "FK__RefreshTo__user___5629CD9C",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "board_post",
                columns: table => new
                {
                    boardId = table.Column<int>(type: "int", nullable: false),
                    postId = table.Column<int>(type: "int", nullable: false),
                    Created_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Created_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    Last_update = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__board_po__F0520DA994AF1A14", x => new { x.boardId, x.postId });
                    table.ForeignKey(
                        name: "FK__board_pos__board__4F7CD00D",
                        column: x => x.boardId,
                        principalTable: "Board",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__board_pos__postI__5070F446",
                        column: x => x.postId,
                        principalTable: "Post",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    postId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Created_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    Last_update = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Comments__3213E83F261FFB70", x => x.id);
                    table.ForeignKey(
                        name: "FK__Comments__postId__44FF419A",
                        column: x => x.postId,
                        principalTable: "Post",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "like_post",
                columns: table => new
                {
                    userId = table.Column<int>(type: "int", nullable: false),
                    postId = table.Column<int>(type: "int", nullable: false),
                    Created_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Created_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    Last_update = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__like_pos__764ADBC6056A024B", x => new { x.userId, x.postId });
                    table.ForeignKey(
                        name: "FK__like_post__postI__4222D4EF",
                        column: x => x.postId,
                        principalTable: "Post",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__like_post__userI__412EB0B6",
                        column: x => x.userId,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "post_tag",
                columns: table => new
                {
                    postId = table.Column<int>(type: "int", nullable: false),
                    tagId = table.Column<int>(type: "int", nullable: false),
                    Created_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Created_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    Last_update = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__post_tag__B803B38F98462B94", x => new { x.postId, x.tagId });
                    table.ForeignKey(
                        name: "FK__post_tag__postId__49C3F6B7",
                        column: x => x.postId,
                        principalTable: "Post",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__post_tag__tagId__4AB81AF0",
                        column: x => x.tagId,
                        principalTable: "Tag",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_board_post_postId",
                table: "board_post",
                column: "postId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_postId",
                table: "Comments",
                column: "postId");

            migrationBuilder.CreateIndex(
                name: "IX_Followers_Followed_id",
                table: "Followers",
                column: "Followed_id");

            migrationBuilder.CreateIndex(
                name: "IX_like_post_postId",
                table: "like_post",
                column: "postId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_Created_by",
                table: "Post",
                column: "Created_by");

            migrationBuilder.CreateIndex(
                name: "IX_post_tag_tagId",
                table: "post_tag",
                column: "tagId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiryDate",
                table: "RefreshTokens",
                column: "expiry_date");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "token");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ__RefreshT__CA90DA7A7577ACF1",
                table: "RefreshTokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__user__AB6E616479AEDB59",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "board_post");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Followers");

            migrationBuilder.DropTable(
                name: "like_post");

            migrationBuilder.DropTable(
                name: "post_tag");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Board");

            migrationBuilder.DropTable(
                name: "Post");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}

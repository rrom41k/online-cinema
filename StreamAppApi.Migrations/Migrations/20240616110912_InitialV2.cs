using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamAppApi.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CountriesGroups",
                columns: table => new
                {
                    CountriesGroupId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountriesGroups", x => x.CountriesGroupId);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    icon = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    poster = table.Column<byte[]>(type: "bytea", nullable: false),
                    posterIv = table.Column<byte[]>(type: "bytea", nullable: false),
                    bigPoster = table.Column<byte[]>(type: "bytea", nullable: false),
                    bigPosterIv = table.Column<byte[]>(type: "bytea", nullable: false),
                    needSubscribe = table.Column<bool>(type: "boolean", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: true),
                    rating = table.Column<double>(type: "double precision", nullable: false),
                    countOpened = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    surname = table.Column<string>(type: "text", nullable: false),
                    patronymic = table.Column<string>(type: "text", nullable: true),
                    slug = table.Column<string>(type: "text", nullable: false),
                    photo = table.Column<byte[]>(type: "bytea", nullable: true),
                    photoIv = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Serials",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    poster = table.Column<byte[]>(type: "bytea", nullable: false),
                    posterIv = table.Column<byte[]>(type: "bytea", nullable: false),
                    bigPoster = table.Column<byte[]>(type: "bytea", nullable: false),
                    bigPosterIv = table.Column<byte[]>(type: "bytea", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: true),
                    rating = table.Column<double>(type: "double precision", nullable: false),
                    countOpened = table.Column<int>(type: "integer", nullable: false),
                    needSubscribe = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Serials", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Subscribes",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    orderDate = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    duration = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscribes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    login = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    passwordHash = table.Column<byte[]>(type: "bytea", nullable: false),
                    passwordSalt = table.Column<byte[]>(type: "bytea", nullable: false),
                    isAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    refreshToken = table.Column<string>(type: "text", nullable: false),
                    tokenCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tokenUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    CountryId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    CountriesGroupId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.CountryId);
                    table.ForeignKey(
                        name: "FK_Countries_CountriesGroups_CountriesGroupId",
                        column: x => x.CountriesGroupId,
                        principalTable: "CountriesGroups",
                        principalColumn: "CountriesGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    SeasonId = table.Column<string>(type: "text", nullable: false),
                    SerialId = table.Column<string>(type: "text", nullable: false),
                    numberSeason = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.SeasonId);
                    table.ForeignKey(
                        name: "FK_Seasons_Serials_SerialId",
                        column: x => x.SerialId,
                        principalTable: "Serials",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscribeGenres",
                columns: table => new
                {
                    SubscribeId = table.Column<string>(type: "text", nullable: false),
                    GenreId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscribeGenres", x => new { x.SubscribeId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_SubscribeGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscribeGenres_Subscribes_SubscribeId",
                        column: x => x.SubscribeId,
                        principalTable: "Subscribes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscribePersons",
                columns: table => new
                {
                    SubscribeId = table.Column<string>(type: "text", nullable: false),
                    PersonId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscribePersons", x => new { x.SubscribeId, x.PersonId });
                    table.ForeignKey(
                        name: "FK_SubscribePersons_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscribePersons_Subscribes_SubscribeId",
                        column: x => x.SubscribeId,
                        principalTable: "Subscribes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscribeCountries",
                columns: table => new
                {
                    SubscribeId = table.Column<string>(type: "text", nullable: false),
                    CountryId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscribeCountries", x => new { x.SubscribeId, x.CountryId });
                    table.ForeignKey(
                        name: "FK_SubscribeCountries_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscribeCountries_Subscribes_SubscribeId",
                        column: x => x.SubscribeId,
                        principalTable: "Subscribes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    videoUrl = table.Column<byte[]>(type: "bytea", nullable: false),
                    videoUrlIv = table.Column<byte[]>(type: "bytea", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    duration = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    isSendTelegram = table.Column<bool>(type: "boolean", nullable: false),
                    movieId = table.Column<string>(type: "text", nullable: true),
                    episodeNumber = table.Column<int>(type: "integer", nullable: true),
                    seasonId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.id);
                    table.ForeignKey(
                        name: "FK_Videos_Movies_movieId",
                        column: x => x.movieId,
                        principalTable: "Movies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Videos_Seasons_seasonId",
                        column: x => x.seasonId,
                        principalTable: "Seasons",
                        principalColumn: "SeasonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    VideoId = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => new { x.UserId, x.VideoId });
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Crews",
                columns: table => new
                {
                    PersonId = table.Column<string>(type: "text", nullable: false),
                    VideoId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crews", x => new { x.PersonId, x.VideoId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_Crews_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crews_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crews_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    VideoId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => new { x.UserId, x.VideoId });
                    table.ForeignKey(
                        name: "FK_Favorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favorites_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    orderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sum = table.Column<decimal>(type: "numeric", nullable: false),
                    userId = table.Column<string>(type: "text", nullable: false),
                    subscribeId = table.Column<string>(type: "text", nullable: true),
                    serialId = table.Column<string>(type: "text", nullable: true),
                    movieId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_Orders_Serials_serialId",
                        column: x => x.serialId,
                        principalTable: "Serials",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Subscribes_subscribeId",
                        column: x => x.subscribeId,
                        principalTable: "Subscribes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Videos_movieId",
                        column: x => x.movieId,
                        principalTable: "Videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    VideoId = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => new { x.UserId, x.VideoId });
                    table.ForeignKey(
                        name: "FK_Ratings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ratings_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoCountries",
                columns: table => new
                {
                    CountryId = table.Column<string>(type: "text", nullable: false),
                    VideoId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoCountries", x => new { x.CountryId, x.VideoId });
                    table.ForeignKey(
                        name: "FK_VideoCountries_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoCountries_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoGenres",
                columns: table => new
                {
                    GenreId = table.Column<string>(type: "text", nullable: false),
                    VideoId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoGenres", x => new { x.GenreId, x.VideoId });
                    table.ForeignKey(
                        name: "FK_VideoGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoGenres_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_VideoId",
                table: "Comments",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_CountriesGroupId",
                table: "Countries",
                column: "CountriesGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Slug",
                table: "Countries",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CountriesGroups_Name",
                table: "CountriesGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Crews_RoleId",
                table: "Crews",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Crews_VideoId",
                table: "Crews",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_VideoId",
                table: "Favorites",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Genres_slug",
                table: "Genres",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movies_slug",
                table: "Movies",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_movieId",
                table: "Orders",
                column: "movieId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_serialId",
                table: "Orders",
                column: "serialId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_subscribeId",
                table: "Orders",
                column: "subscribeId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_userId",
                table: "Orders",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_slug",
                table: "Persons",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_VideoId",
                table: "Ratings",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_SerialId",
                table: "Seasons",
                column: "SerialId");

            migrationBuilder.CreateIndex(
                name: "IX_Serials_slug",
                table: "Serials",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscribeCountries_CountryId",
                table: "SubscribeCountries",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscribeGenres_GenreId",
                table: "SubscribeGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscribePersons_PersonId",
                table: "SubscribePersons",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                table: "Users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_login",
                table: "Users",
                column: "login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_phone",
                table: "Users",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoCountries_VideoId",
                table: "VideoCountries",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoGenres_VideoId",
                table: "VideoGenres",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_movieId",
                table: "Videos",
                column: "movieId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Videos_seasonId",
                table: "Videos",
                column: "seasonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Crews");

            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "SubscribeCountries");

            migrationBuilder.DropTable(
                name: "SubscribeGenres");

            migrationBuilder.DropTable(
                name: "SubscribePersons");

            migrationBuilder.DropTable(
                name: "VideoCountries");

            migrationBuilder.DropTable(
                name: "VideoGenres");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "Subscribes");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropTable(
                name: "CountriesGroups");

            migrationBuilder.DropTable(
                name: "Movies");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "Serials");
        }
    }
}

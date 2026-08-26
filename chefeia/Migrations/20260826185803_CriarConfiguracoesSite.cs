using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chefeia.Migrations
{
    /// <inheritdoc />
    public partial class CriarConfiguracoesSite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MonthlyAiLimit = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    HasAds = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteName = table.Column<string>(type: "text", nullable: false),
                    SiteSlogan = table.Column<string>(type: "text", nullable: false),
                    HeroTitle = table.Column<string>(type: "text", nullable: false),
                    HeroSubtitle = table.Column<string>(type: "text", nullable: false),
                    HeroImageUrl = table.Column<string>(type: "text", nullable: false),
                    SearchPlaceholder = table.Column<string>(type: "text", nullable: false),
                    Feature1Emoji = table.Column<string>(type: "text", nullable: false),
                    Feature1Title = table.Column<string>(type: "text", nullable: false),
                    Feature1Text = table.Column<string>(type: "text", nullable: false),
                    Feature2Emoji = table.Column<string>(type: "text", nullable: false),
                    Feature2Title = table.Column<string>(type: "text", nullable: false),
                    Feature2Text = table.Column<string>(type: "text", nullable: false),
                    Feature3Emoji = table.Column<string>(type: "text", nullable: false),
                    Feature3Title = table.Column<string>(type: "text", nullable: false),
                    Feature3Text = table.Column<string>(type: "text", nullable: false),
                    Feature4Emoji = table.Column<string>(type: "text", nullable: false),
                    Feature4Title = table.Column<string>(type: "text", nullable: false),
                    Feature4Text = table.Column<string>(type: "text", nullable: false),
                    FreeMonthlyLimit = table.Column<int>(type: "integer", nullable: false),
                    PremiumMonthlyLimit = table.Column<int>(type: "integer", nullable: false),
                    PremiumPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    FeaturedRecipeTitle = table.Column<string>(type: "text", nullable: false),
                    FeaturedRecipeImageUrl = table.Column<string>(type: "text", nullable: false),
                    FeaturedRecipeCountry = table.Column<string>(type: "text", nullable: false),
                    FeaturedRecipeMinutes = table.Column<int>(type: "integer", nullable: false),
                    FeaturedRecipeServings = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "Feature1Emoji", "Feature1Text", "Feature1Title", "Feature2Emoji", "Feature2Text", "Feature2Title", "Feature3Emoji", "Feature3Text", "Feature3Title", "Feature4Emoji", "Feature4Text", "Feature4Title", "FeaturedRecipeCountry", "FeaturedRecipeImageUrl", "FeaturedRecipeMinutes", "FeaturedRecipeServings", "FeaturedRecipeTitle", "FreeMonthlyLimit", "HeroImageUrl", "HeroSubtitle", "HeroTitle", "PremiumMonthlyLimit", "PremiumPrice", "SearchPlaceholder", "SiteName", "SiteSlogan" },
                values: new object[] { 1, "🥗", "Criadas sob medida com os ingredientes que você tem.", "Receitas personalizadas", "⏱️", "Receitas prontas em segundos para facilitar seu dia.", "Rápido e prático", "🌎", "Explore sabores de diferentes países e culturas.", "Cozinha do mundo", "❤️", "Descubra novas combinações e experiências.", "Feito com carinho", "Brasileira", "https://images.unsplash.com/photo-1604908176997-125f25cc6f3d", 35, 4, "Frango Cremoso com Batatas", 3, "https://images.unsplash.com/photo-1604908176997-125f25cc6f3d", "Digite os ingredientes que você tem em casa e deixe o Chefe IA criar algo incrível para você!", "Sua próxima receita começa aqui", 50, 39.90m, "Ex: frango, batata, tomate...", "Chefe IA", "Receitas que surpreendem!" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "SiteSettings");
        }
    }
}

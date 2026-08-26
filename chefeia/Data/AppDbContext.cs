using chefeia.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace chefeia.Data
{
    public class AppDbContext
        : IdentityDbContext<AppUser>
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }


        // =====================================================
        // TABELAS
        // =====================================================

        public DbSet<Plan> Plans { get; set; }

        public DbSet<SiteSettings> SiteSettings { get; set; }

        public DbSet<AiUsage> AiUsages { get; set; }

        public DbSet<RecipeHistory> RecipeHistories { get; set; }

        public DbSet<UserSubscription> UserSubscriptions { get; set; }

        public DbSet<AsaasWebhookEvent> AsaasWebhookEvents { get; set; }


        // =====================================================
        // CONFIGURAÇÃO DO BANCO
        // =====================================================

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // =================================================
            // USUÁRIOS
            // =================================================

            modelBuilder.Entity<AppUser>(
                entity =>
                {
                    entity.Property(
                            x => x.Name
                        )
                        .HasMaxLength(150);

                    entity.Property(
                            x => x.PlanCode
                        )
                        .HasMaxLength(30)
                        .IsRequired();

                    entity.HasIndex(
                        x => x.PlanCode
                    );

                    entity.HasIndex(
                        x => x.CreatedAt
                    );
                }
            );


            // =================================================
            // PLANOS
            // =================================================

            modelBuilder.Entity<Plan>(
                entity =>
                {
                    entity.HasKey(
                        x => x.Id
                    );

                    entity.Property(
                            x => x.Name
                        )
                        .HasMaxLength(50)
                        .IsRequired();

                    entity.Property(
                            x => x.Code
                        )
                        .HasMaxLength(30)
                        .IsRequired();

                    entity.Property(
                            x => x.Price
                        )
                        .HasPrecision(10, 2);

                    entity.HasIndex(
                            x => x.Code
                        )
                        .IsUnique();


                    // PLANO GRATUITO

                    entity.HasData(
                        new Plan
                        {
                            Id = 1,

                            Name = "Gratuito",

                            Code = "FREE",

                            MonthlyAiLimit = 3,

                            Price = 0.00m,

                            HasAds = true,

                            HasHistory = false,

                            CanDownloadRecipes = false,

                            IsActive = true
                        },


                        // PLANO PREMIUM

                        new Plan
                        {
                            Id = 2,

                            Name = "Premium",

                            Code = "PREMIUM",

                            MonthlyAiLimit = 50,

                            Price = 39.90m,

                            HasAds = false,

                            HasHistory = true,

                            CanDownloadRecipes = true,

                            IsActive = true
                        }
                    );
                }
            );


            // =================================================
            // CONFIGURAÇÕES DO SITE
            // =================================================

            modelBuilder.Entity<SiteSettings>()
                .HasData(
                    new SiteSettings
                    {
                        Id = 1,

                        SiteName = "Chefe IA",

                        SiteSlogan =
                            "Receitas que surpreendem!",

                        HeroTitle =
                            "Sua próxima receita começa aqui",

                        HeroSubtitle =
                            "Digite os ingredientes que você tem em casa e deixe o Chefe IA criar algo incrível para você!",

                        HeroImageUrl =
                            "https://images.unsplash.com/photo-1604908176997-125f25cc6f3d",

                        SearchPlaceholder =
                            "Ex: frango, batata, tomate...",

                        Feature1Emoji = "🥗",

                        Feature1Title =
                            "Receitas personalizadas",

                        Feature1Text =
                            "Criadas sob medida com os ingredientes que você tem.",

                        Feature2Emoji = "⏱️",

                        Feature2Title =
                            "Rápido e prático",

                        Feature2Text =
                            "Receitas prontas em segundos para facilitar seu dia.",

                        Feature3Emoji = "🌎",

                        Feature3Title =
                            "Cozinha do mundo",

                        Feature3Text =
                            "Explore sabores de diferentes países e culturas.",

                        Feature4Emoji = "❤️",

                        Feature4Title =
                            "Feito com carinho",

                        Feature4Text =
                            "Descubra novas combinações e experiências.",

                        FreeMonthlyLimit = 3,

                        PremiumMonthlyLimit = 50,

                        PremiumPrice = 39.90m,

                        FeaturedRecipeTitle =
                            "Frango Cremoso com Batatas",

                        FeaturedRecipeImageUrl =
                            "https://images.unsplash.com/photo-1604908176997-125f25cc6f3d",

                        FeaturedRecipeCountry =
                            "Brasileira",

                        FeaturedRecipeMinutes = 35,

                        FeaturedRecipeServings = 4
                    }
                );


            // =================================================
            // CONSUMO DA IA
            // =================================================

            modelBuilder.Entity<AiUsage>(
                entity =>
                {
                    entity.HasKey(
                        x => x.Id
                    );

                    entity.Property(
                            x => x.Preference
                        )
                        .HasMaxLength(150);

                    entity.Property(
                            x => x.PlanName
                        )
                        .HasMaxLength(50);

                    entity.Property(
                            x => x.UserId
                        )
                        .HasMaxLength(450);

                    entity.Property(
                            x => x.ErrorMessage
                        )
                        .HasMaxLength(2000);

                    entity.HasIndex(
                        x => x.CreatedAt
                    );

                    entity.HasIndex(
                        x => x.Success
                    );

                    entity.HasIndex(
                        x => x.UserId
                    );
                }
            );


            // =================================================
            // HISTÓRICO DE RECEITAS
            // =================================================

            modelBuilder.Entity<RecipeHistory>(
                entity =>
                {
                    entity.HasKey(
                        x => x.Id
                    );

                    entity.Property(
                            x => x.UserId
                        )
                        .HasMaxLength(450)
                        .IsRequired();

                    entity.Property(
                            x => x.Name
                        )
                        .HasMaxLength(200)
                        .IsRequired();

                    entity.Property(
                            x => x.Country
                        )
                        .HasMaxLength(100);

                    entity.Property(
                            x => x.Category
                        )
                        .HasMaxLength(100);

                    entity.Property(
                            x => x.Description
                        )
                        .HasMaxLength(2000);

                    entity.Property(
                            x => x.RequestedIngredients
                        )
                        .HasMaxLength(1000);

                    entity.Property(
                            x => x.Preference
                        )
                        .HasMaxLength(150);

                    entity.Property(
                            x => x.IngredientsJson
                        )
                        .HasColumnType("text");

                    entity.Property(
                            x => x.StepsJson
                        )
                        .HasColumnType("text");

                    entity.HasIndex(
                        x => x.UserId
                    );

                    entity.HasIndex(
                        x => x.CreatedAt
                    );

                    entity.HasOne(
                            x => x.User
                        )
                        .WithMany()
                        .HasForeignKey(
                            x => x.UserId
                        )
                        .OnDelete(
                            DeleteBehavior.Cascade
                        );
                }
            );


            // =================================================
            // ASSINATURAS DOS USUÁRIOS
            // =================================================

            modelBuilder.Entity<UserSubscription>(
                entity =>
                {
                    entity.HasKey(
                        x => x.Id
                    );

                    entity.Property(
                            x => x.UserId
                        )
                        .HasMaxLength(450)
                        .IsRequired();

                    entity.Property(
                            x => x.AsaasCustomerId
                        )
                        .HasMaxLength(100);

                    entity.Property(
                            x => x.AsaasSubscriptionId
                        )
                        .HasMaxLength(100);

                    entity.Property(
                            x => x.LastPaymentId
                        )
                        .HasMaxLength(100);

                    entity.Property(
                            x => x.PlanCode
                        )
                        .HasMaxLength(30)
                        .IsRequired();

                    entity.Property(
                            x => x.Status
                        )
                        .HasMaxLength(30)
                        .IsRequired();

                    entity.Property(
                            x => x.BillingType
                        )
                        .HasMaxLength(30);

                    entity.Property(
                            x => x.Price
                        )
                        .HasPrecision(10, 2);


                    // =========================================
                    // ÍNDICES
                    // =========================================

                    entity.HasIndex(
                        x => x.UserId
                    );

                    entity.HasIndex(
                        x => x.AsaasCustomerId
                    );

                    entity.HasIndex(
                        x => x.AsaasSubscriptionId
                    );

                    entity.HasIndex(
                        x => x.LastPaymentId
                    );

                    entity.HasIndex(
                        x => x.Status
                    );

                    entity.HasIndex(
                        x => x.CreatedAt
                    );


                    // =========================================
                    // RELACIONAMENTO COM USUÁRIO
                    // =========================================

                    entity.HasOne(
                            x => x.User
                        )
                        .WithMany()
                        .HasForeignKey(
                            x => x.UserId
                        )
                        .OnDelete(
                            DeleteBehavior.Cascade
                        );
                }
            );


            // =================================================
            // EVENTOS DO WEBHOOK ASAAS
            // =================================================

            modelBuilder.Entity<AsaasWebhookEvent>(
                entity =>
                {
                    entity.HasKey(
                        x => x.Id
                    );


                    // =========================================
                    // EVENTO
                    // =========================================

                    entity.Property(
                            x => x.EventId
                        )
                        .HasMaxLength(150)
                        .IsRequired();

                    entity.Property(
                            x => x.EventType
                        )
                        .HasMaxLength(100)
                        .IsRequired();


                    // =========================================
                    // COBRANÇA
                    // =========================================

                    entity.Property(
                            x => x.PaymentId
                        )
                        .HasMaxLength(150);

                    entity.Property(
                            x => x.CustomerId
                        )
                        .HasMaxLength(150);

                    entity.Property(
                            x => x.SubscriptionId
                        )
                        .HasMaxLength(150);

                    entity.Property(
                            x => x.PaymentStatus
                        )
                        .HasMaxLength(100);


                    // =========================================
                    // REFERÊNCIA INTERNA
                    // =========================================

                    entity.Property(
                            x => x.ExternalReference
                        )
                        .HasMaxLength(500);

                    entity.Property(
                            x => x.UserId
                        )
                        .HasMaxLength(450);


                    // =========================================
                    // ERRO
                    // =========================================

                    entity.Property(
                            x => x.ErrorMessage
                        )
                        .HasMaxLength(2000);


                    // =========================================
                    // PAYLOAD ORIGINAL
                    // =========================================

                    entity.Property(
                            x => x.PayloadJson
                        )
                        .HasColumnType("text")
                        .IsRequired();


                    // =========================================
                    // ÍNDICE ÚNICO
                    //
                    // Esse é o principal mecanismo de
                    // proteção contra webhook duplicado.
                    // =========================================

                    entity.HasIndex(
                            x => x.EventId
                        )
                        .IsUnique();


                    // =========================================
                    // ÍNDICES DE PESQUISA
                    // =========================================

                    entity.HasIndex(
                        x => x.EventType
                    );

                    entity.HasIndex(
                        x => x.PaymentId
                    );

                    entity.HasIndex(
                        x => x.CustomerId
                    );

                    entity.HasIndex(
                        x => x.SubscriptionId
                    );

                    entity.HasIndex(
                        x => x.UserId
                    );

                    entity.HasIndex(
                        x => x.Processed
                    );

                    entity.HasIndex(
                        x => x.Success
                    );

                    entity.HasIndex(
                        x => x.ReceivedAt
                    );
                }
            );
        }
    }
}
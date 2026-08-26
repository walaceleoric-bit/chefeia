using chefeia.Models;
using Microsoft.AspNetCore.Identity;

namespace chefeia.Data
{
    public static class IdentitySeed
    {
        public static async Task InicializarAsync(
            IServiceProvider serviceProvider)
        {
            using var scope =
                serviceProvider.CreateScope();

            var roleManager =
                scope.ServiceProvider
                    .GetRequiredService<
                        RoleManager<IdentityRole>
                    >();

            var userManager =
                scope.ServiceProvider
                    .GetRequiredService<
                        UserManager<AppUser>
                    >();

            // =================================================
            // ROLE ADMIN
            // =================================================

            const string adminRole =
                "Admin";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(adminRole)
                );
            }


            // =================================================
            // USUÁRIO ADMIN
            // =================================================

            var adminEmail =
                "admin@chefeia.com";

            var adminUser =
                await userManager.FindByEmailAsync(
                    adminEmail
                );

            if (adminUser == null)
            {
                adminUser =
                    new AppUser
                    {
                        UserName =
                            adminEmail,

                        Email =
                            adminEmail,

                        EmailConfirmed =
                            true,

                        Name =
                            "Administrador",

                        PlanCode =
                            "PREMIUM",

                        IsActive =
                            true,

                        CreatedAt =
                            DateTime.UtcNow
                    };

                var resultado =
                    await userManager.CreateAsync(
                        adminUser,
                        "Admin123"
                    );

                if (!resultado.Succeeded)
                {
                    var erros =
                        string.Join(
                            " | ",
                            resultado.Errors
                                .Select(
                                    x => x.Description
                                )
                        );

                    throw new InvalidOperationException(
                        "Erro ao criar usuário Admin: " +
                        erros
                    );
                }
            }


            // =================================================
            // ATRIBUIR ROLE ADMIN
            // =================================================

            if (!await userManager.IsInRoleAsync(
                    adminUser,
                    adminRole))
            {
                await userManager.AddToRoleAsync(
                    adminUser,
                    adminRole
                );
            }
        }
    }
}
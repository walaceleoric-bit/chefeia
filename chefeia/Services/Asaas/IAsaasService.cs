using chefeia.Models;

namespace chefeia.Services.Asaas
{
    public interface IAsaasService
    {
        Task<string> CriarOuObterClienteAsync(
            AppUser usuario,
            CancellationToken cancellationToken = default
        );


        Task<string> CriarCheckoutPremiumAsync(
            AppUser usuario,
            decimal preco,
            string successUrl,
            string cancelUrl,
            string expiredUrl,
            CancellationToken cancellationToken = default
        );
    }
}
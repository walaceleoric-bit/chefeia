namespace chefeia.Models
{
    public class AsaasOptions
    {
        public string BaseUrl { get; set; } =
            "https://api-sandbox.asaas.com/v3";

        public string ApiKey { get; set; } =
            string.Empty;

        public string WebhookToken { get; set; } =
            string.Empty;
    }
}
namespace chefeia.Models
{
    public class AdminUsuarioViewModel
    {
        public string Id { get; set; } =
            string.Empty;

        public string Name { get; set; } =
            string.Empty;

        public string Email { get; set; } =
            string.Empty;

        public string PlanCode { get; set; } =
            "FREE";

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public bool IsAdmin { get; set; }

        public int ConsultasUsadasMes { get; set; }

        public int LimiteMensal { get; set; }

        public int ConsultasRestantes { get; set; }
    }
}
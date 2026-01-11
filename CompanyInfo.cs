namespace login_store
{
    public class CompanyInfo
    {
        public int Id { get; set; } = 1;
        public string Description { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public string TaxId { get; set; }
    }
}
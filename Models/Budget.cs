namespace ProiectMTP.Models
{
    public class Budget
    {
        public string Category { get; set; } = string.Empty;
        public decimal Limit { get; set; }
        public decimal CurrentSpent { get; set; }
        public decimal Remaining => Limit - CurrentSpent;
    }
}
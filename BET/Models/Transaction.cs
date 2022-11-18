using System.ComponentModel.DataAnnotations;

namespace BET.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public string UserId { get; set; }
        public int OrderNo { get; set; }

    }
}
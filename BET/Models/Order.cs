using System.ComponentModel.DataAnnotations;
namespace BET.Models
{
    public class Order
    {
        [Key]
        public int OrderNo { get; set; }
        public string UserId { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsProcessed { get; set; }
    }
}
using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class Feedback
    {
        public int Id { get; set; } 
        public int UserId { get; set; } 
        public string? Comment { get; set; } 
        public bool IsSeen { get; set; } = false; 
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User User { get; set; }
        public ICollection<Response> Responses { get; set; } = new List<Response>(); 
    }
}

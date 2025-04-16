using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class Response
    {
        public int ResponseId { get; set; } 
        public int FeedbackId { get; set; } 
        public string ResponseText { get; set; } = string.Empty; 
        public DateTime SentAt { get; set; } = DateTime.Now;

        public Feedback Feedback { get; set; } 
    }
}

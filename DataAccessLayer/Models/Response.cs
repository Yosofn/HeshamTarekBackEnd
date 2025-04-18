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
    public int UserId { get; set; } 
    public string Reply { get; set; } 
    public DateTime SentAt { get; set; } = DateTime.Now; 
    public bool IsSeen { get; set; } = false;  

    public Feedback Feedback { get; set; }  
    public User User { get; set; }  
}
}

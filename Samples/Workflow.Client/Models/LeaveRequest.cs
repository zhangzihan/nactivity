using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Workflow.Client.Models
{
    public class LeaveRequest
    {
        public string Id { get; set; }

        public string Code { get; set; }

        public string UserId { get; set; }

        public User User { get; set; }

        public DateTime Start { get; set; }

        public int Days
        {
            get
            {
                int days = End.Subtract(Start).Days;
                return days + 1;
            }
        }

        public string Reason { get; set; }

        public DateTime End { get; set; }
    }
}

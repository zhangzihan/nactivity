using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Workflow.Client.Models
{
    public class User
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Duty { get; set; }

        public string TenantId { get; set; }
    }
}

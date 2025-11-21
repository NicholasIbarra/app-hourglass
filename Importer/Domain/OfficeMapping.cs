using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class OfficeMapping
    {
        public Guid Id { get; set; }
        public string OfficeCode { get; set; } = "";
        public string OfficeName { get; set; } = "";
        public string Region { get; set; } = "";
    }
}

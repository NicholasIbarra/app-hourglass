using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Imports
{
    public class ImportRecord
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string FileName { get; set; }
        public ImportStatus Status { get; set; }
        public int TotalRecords { get; set; }
        public int ProcessedRecords { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

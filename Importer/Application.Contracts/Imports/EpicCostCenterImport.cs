using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Imports
{

    public class EpicCostCenterImport : IImport
    {
        public long CostCenterId { get; set; }

        public string Name { get; set; } = string.Empty;

        public Guid OfficeId { get; set; }
    }
}

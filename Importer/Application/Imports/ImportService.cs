using Application.Contracts.Imports;
using Microsoft.Extensions.Logging;

namespace Application.Imports
{
    public class ImportService : IImportService
    {
        private readonly ILogger _logger;

        public ImportService(ILogger<ImportService> logger)
        {
            _logger = logger;
        }


    }
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Imports
{

    public interface ImportMappingDto;


    public class ImportResult
    {
        public bool Success { get; set; }
        public int StagedRecords { get; set; }
        public string? Error { get; set; }
    }

    public class ImportItemResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }

        public static ImportItemResult Successful() => new ImportItemResult { Success = true };

        public static ImportItemResult Failed(string error) => new ImportItemResult { Success = false, Error = error };
    }


    public interface IImportWorkflowBase<IImport> where IImport : ImportMappingDto
    {
        //Task<Guid> Import(Stream fileStream);
        Task<ImportResult> RunAsync(IFormFile file);
    }
}

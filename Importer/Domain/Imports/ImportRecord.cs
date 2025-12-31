using Domain.Shared;

namespace Domain.Imports
{
    public class ImportRecord
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string FileName { get; set; } = "";
        public string FileReferenceId { get; set; } = "";
        public ImportType ImportType { get; set; }
        public ImportStatus Status { get; set; }
        public int TotalRecords { get; set; }
        public int ProcessedRecords { get; set; }
        public string? ErrorMessage { get; set; }

        public static ImportRecord Queue(string fileName, string fileReferenceId, ImportType importType)
        {
            return new ImportRecord
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                FileName = fileName,
                FileReferenceId = fileReferenceId,
                Status = ImportStatus.Pending,
                TotalRecords = 0,
                ProcessedRecords = 0,
                ImportType = importType,
                ErrorMessage = null
            };
        }

        public ImportRecord Start(int total)
        {
            Status = ImportStatus.InProgress;
            ProcessedRecords = 0;
            ErrorMessage = null;
            TotalRecords = total;
            return this;
        }

        public void Failed(string errorMessage)
        {
            Status = ImportStatus.Failed;
            ErrorMessage = errorMessage;
        }

        public void UpdateProgress(int processedRecords)
        {
            ProcessedRecords = Math.Max(ProcessedRecords, processedRecords);
        }

        public void Complete()
        {
            Status = ImportStatus.Completed;
            ProcessedRecords = TotalRecords;
        }

        public void RequestCancellation()
        {
            Status = Status == ImportStatus.Cancelled ? ImportStatus.Cancelled : ImportStatus.Cancelling;
        }

        public void Cancel()
        {
            Status = ImportStatus.Cancelled;
        }
    }
}

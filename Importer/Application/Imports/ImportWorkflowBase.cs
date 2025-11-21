using Application.Contracts.Imports;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Imports;
using Domain.Shared;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace Application.Imports
{
    public class ImportOfficeMapping : ImportWorkflowBase<ImportOfficeMappingDto>
    {
        public ImportOfficeMapping(
            ILogger<ImportOfficeMapping> logger,
            IBlobStorage blobStorage,
            IBackgroundJobClient backgroundJobClient,
            IApplicationDbContext dbContext)
            : base(logger, blobStorage, backgroundJobClient, dbContext)
        {
        }
        public override ImportType ImportType => ImportType.EpicCostCenter;

        public override async Task<ImportItemResult> StartProcessingAsync(
            ImportOfficeMappingDto item, 
            object? context)
        {
            try
            {
                await Task.Delay(1000); // Simulate some processing time.
                return ImportItemResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing import item for OfficeCode: {OfficeCode}", item.OfficeCode);
                return ImportItemResult.Failed("Critical error occurred");
            }
        }
    }

    public class ImportOfficeMappingDto : ImportMappingDto
    {
        public string OfficeCode { get; set; } = "";
        public string OfficeName { get; set; } = "";
        public string Region { get; set; } = "";
    }

    public abstract class ImportWorkflowBase<TImport> : IImportWorkflowBase<TImport>
        where TImport : ImportMappingDto
    {
        protected readonly ILogger _logger;
        private readonly IBlobStorage _blobStorage;
        private readonly IBackgroundJobClient _jobs;
        private readonly IApplicationDbContext _db;

        protected ImportWorkflowBase(
            ILogger<ImportWorkflowBase<TImport>> logger,
            IBlobStorage blobStorage,
            IBackgroundJobClient jobs,
            IApplicationDbContext db)
        {
            _logger = logger;
            _blobStorage = blobStorage;
            _jobs = jobs;
            _db = db;
        }

        public const string BlobContainer = "import";
        public abstract ImportType ImportType { get; }

        /// <summary>
        /// Override to parse CSV, JSON, Excel, etc.
        /// </summary>
        protected virtual async Task<List<TImport>> ParseAsync(Stream stream)
        {
            using var reader = new StreamReader(stream);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
            };

            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<TImport>().ToList();

            return records;

        }

        /// <summary>
        /// Override to build context for pre-processing validation.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        protected virtual Task<object?> BuildPreProcessingContextAsync(
            IReadOnlyList<TImport> items)
            => Task.FromResult<object?>(null);


        /// <summary>
        /// Override for business validation before queuing the import.
        /// </summary>
        public virtual Task<bool> PreProcessingValidate(Guid importId, IReadOnlyList<TImport> items)
            => Task.FromResult(true);

        /// <summary>
        /// Actual processing (DB writes, upserts, whatever).
        /// </summary>
        public abstract Task<ImportItemResult> StartProcessingAsync(TImport item, object? context);

        /// <summary>
        /// Entry point when a user uploads a file.
        /// </summary>
        public async Task<ImportResult> Import(IFormFile file)
        {
            _logger.LogInformation("Starting import: {FileName}", file.FileName);

            // BUFFER THE UPLOAD ONCE
            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            ms.Position = 0;
            var fileReference = await _blobStorage.SaveAsync(BlobContainer, new MemoryStream(ms.ToArray()));

            ms.Position = 0;
            var items = await ParseAsync(ms);

            if (items == null || items.Count == 0)
            {
                _logger.LogWarning("File {FileName} contained no parseable items.", file.FileName);
                return new ImportResult { Success = false, StagedRecords = 0, Error = "No valid items found." };
            }

            var import = ImportRecord.Queue(file.FileName, fileReference, ImportType);

            var valid = await PreProcessingValidate(import.Id, items);
            if (!valid)
            {
                import.Failed("Pre-processing validation failed.");
                _db.ImportRecords.Add(import);
                await _db.SaveChangesAsync();
                return new ImportResult { Success = false, StagedRecords = 0, Error = "Validation failed." };
            }

            import.Start(items.Count);
            _db.ImportRecords.Add(import);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Enqueuing background job for import {ImportId}", import.Id);
            _jobs.Enqueue(() => ProcessImport(import.Id));

            return new ImportResult
            {
                Success = true,
                StagedRecords = items.Count
            };
        }

        /// <summary>
        /// Executed in background via Hangfire.
        /// </summary>
        public async Task ProcessImport(Guid importId)
        {
            _logger.LogInformation("Processing import job {ImportId}", importId);

            var import = await _db.ImportRecords.FirstOrDefaultAsync(i => i.Id == importId);
            if (import == null)
            {
                _logger.LogError("Import job {ImportId} not found.", importId);
                return;
            }

            try
            {
                var blob = await _blobStorage.GetAsync(BlobContainer, import.FileReferenceId);
                if (blob == null)
                {
                    import.Failed("Import file not found.");
                    return;
                }

                var items = await ParseAsync(blob);
                if (items == null || items.Count == 0)
                {
                    import.Failed("Failed to parse import file.");
                    return;
                }

                var context = await BuildPreProcessingContextAsync(items);

                foreach (var item in items)
                {
                    var shouldCancel = await _db.ImportRecords
                        .AsNoTracking()
                        .AnyAsync(i => i.Id == importId && (i.Status == ImportStatus.Cancelling || i.Status == ImportStatus.Cancelled));

                    if (shouldCancel)
                    {
                        _logger.LogInformation("Import job {ImportId} cancelled by user.", importId);
                        import.Cancel();
                        return;
                    }

                    var result = await StartProcessingAsync(item, context);

                    if (!result.Success)
                    {
                        _logger.LogWarning("Import job {ImportId} item failed: {Error}", importId, result.Error);
                        import.Failed(result.Error ?? "Item processing failed.");
                    }
                    else
                    {
                        import.UpdateProgress(import.ProcessedRecords + 1);

                        // publish websocket event, etc.
                    }

                    await _db.SaveChangesAsync();
                }

                import.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Import job {ImportId} failed.", importId);
                import.Failed(ex.Message);
            }
            finally
            {
                await _db.SaveChangesAsync();
            }
        }
    }
}

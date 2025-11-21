using Application;
using Microsoft.Extensions.Logging;

namespace Infrasturcture.Storage
{
    public sealed class LocalBlobStorage : IBlobStorage
    {
        private readonly string _rootPath;
        private readonly ILogger<LocalBlobStorage>? _logger;

        public LocalBlobStorage(string rootPath, ILogger<LocalBlobStorage>? logger = null)
        {
            _rootPath = rootPath;
            _logger = logger;

            Directory.CreateDirectory(_rootPath);
        }

        public async Task SaveAsync(
            string container,
            string fileName,
            Stream content,
            CancellationToken cancellationToken = default)
        {
            var containerPath = Path.Combine(_rootPath, container);
            Directory.CreateDirectory(containerPath);

            var filePath = Path.Combine(containerPath, fileName);

            _logger?.LogDebug("Saving file {FilePath}", filePath);

            await using var fileStream = File.Create(filePath);
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        public async Task<Stream?> GetAsync(
            string container,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(_rootPath, container, fileName);

            if (!File.Exists(filePath))
            {
                _logger?.LogWarning("File not found: {FilePath}", filePath);
                return null;
            }

            // Return a FileStream, let caller dispose
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public Task<bool> DeleteAsync(
            string container,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(_rootPath, container, fileName);

            if (!File.Exists(filePath))
            {
                _logger?.LogDebug("Delete requested but file not found: {FilePath}", filePath);
                return Task.FromResult(false);
            }

            File.Delete(filePath);
            return Task.FromResult(true);
        }
    }

}

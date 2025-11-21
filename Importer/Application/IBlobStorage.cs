using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public interface IBlobStorage
    {
        Task SaveAsync(string container, string fileName, Stream content, CancellationToken cancellationToken = default);
        Task<Stream?> GetAsync(string container, string fileName, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string container, string fileName, CancellationToken cancellationToken = default);
    }
}

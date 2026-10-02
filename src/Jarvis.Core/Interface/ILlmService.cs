using System;
using System.Collections.Generic;
using System.Text;

namespace Jarvis.Core.Interface
{
    public interface ILlmService
    {
        Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
    }
}

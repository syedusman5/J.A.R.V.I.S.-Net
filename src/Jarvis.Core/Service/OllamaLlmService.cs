using Jarvis.Core.Interface;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace Jarvis.Core.Service
{
    public class OllamaLlmService(IHttpClientFactory httpClientFactory) : ILlmService
    {
        public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var client = httpClientFactory.CreateClient("jarvis");

            var request = new
            {
                model = "llama3.2",
                prompt = prompt,
                stream = false
            };

            var response = await client.PostAsJsonAsync("http://localhost:11434/api/generate",request, cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>( cancellationToken);

            return result?.Response ?? string.Empty;

        }

        private sealed class OllamaResponse
        {
            public string Response { get; set; } = string.Empty;
        }
    }
}

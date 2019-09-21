using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QueueProcessor.Services
{
    public class HttpClientFileDownloader : IFileDownloader
    {
        private readonly HttpClient _http;
        private readonly ILogger<HttpClientFileDownloader> _logger;

        public HttpClientFileDownloader(HttpClient http, ILogger<HttpClientFileDownloader> logger)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GetContent(Uri target)
        {
            _logger.LogInformation("Downloading {URI}", target);

            using (var request = new HttpRequestMessage(HttpMethod.Get, target))
            using (var response = await _http.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Download from {URI} complete", target);

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
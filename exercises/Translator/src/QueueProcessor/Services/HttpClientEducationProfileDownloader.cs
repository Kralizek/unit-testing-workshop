using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QueueProcessor.Handlers;

namespace QueueProcessor.Services
{
    public class HttpClientEducationProfileDownloader : IEducationProfileDownloader
    {
        private readonly HttpClient _http;
        private readonly ILogger<HttpClientEducationProfileDownloader> _logger;

        public HttpClientEducationProfileDownloader(HttpClient http, ILogger<HttpClientEducationProfileDownloader> logger)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GetProfile(int educationId)
        {
            var uriToDownload = new Uri(string.Format(EducationUrlFormat, educationId));

            _logger.LogInformation("Downloading {URI}", uriToDownload);

            using (var request = new HttpRequestMessage(HttpMethod.Get, uriToDownload))
            using (var response = await _http.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Download from {URI} complete", uriToDownload);

                return await response.Content.ReadAsStringAsync();
            }
        }

        public const string EducationUrlFormat = "https://www.studentum.se/education/{0}";
    }
}
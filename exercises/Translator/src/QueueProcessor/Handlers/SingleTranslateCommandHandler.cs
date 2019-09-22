using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Translate;
using Amazon.Translate.Model;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nybus;
using QueueProcessor.Messages;

namespace QueueProcessor.Handlers
{
    public class SingleTranslateCommandHandler : ICommandHandler<TranslateCommand>
    {
        private readonly HttpClient _http;
        private readonly IAmazonTranslate _translate;
        private readonly IAmazonS3 _s3;
        private readonly TranslateOptions _options;
        private readonly ILogger<SingleTranslateCommandHandler> _logger;

        public SingleTranslateCommandHandler(HttpClient http, IAmazonTranslate translate, IAmazonS3 s3, IOptions<TranslateOptions> options, ILogger<SingleTranslateCommandHandler> logger)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _translate = translate ?? throw new ArgumentNullException(nameof(translate));
            _s3 = s3 ?? throw new ArgumentNullException(nameof(s3));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(IDispatcher dispatcher, ICommandContext<TranslateCommand> context)
        {
            if (context.Command.ToLanguage == Language.ChineseSimplified)
            {
                throw new ArgumentOutOfRangeException(nameof(context.Command.ToLanguage), "Chinese not supported");
            }

            var uriToTranslate = new Uri($@"https://www.studentum.se/education/{context.Command.EducationId}");

            var content = await GetContent(uriToTranslate);

            var contentToTranslate = ExtractTexts(content);

            var translations = new List<string>();

            foreach (var text in contentToTranslate)
            {
                var translatedText = await TranslateContent(text, context.Command.ToLanguage);
                translations.Add(translatedText);
            }

            var now = DateTimeOffset.UtcNow;

            var fileKey = $"translations/{now.Year:00}/{now.Month:00}/{now.Day:00}/{context.Command.EducationId:000000000000}/{LanguageMappings[context.Command.ToLanguage]}";

            await StoreTranslations(fileKey, translations);

            await dispatcher.RaiseEventAsync(new TranslatedEvent
            {
                EducationId = context.Command.EducationId,
                ToLanguage = context.Command.ToLanguage,
                TranslationFileKey = fileKey
            });
        }

        private async Task<string> GetContent(Uri uriToTranslate)
        {
            _logger.LogInformation("Downloading {URI}", uriToTranslate);

            using (var request = new HttpRequestMessage(HttpMethod.Get, uriToTranslate))
            using (var response = await _http.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Download from {URI} complete", uriToTranslate);

                return await response.Content.ReadAsStringAsync();
            }
        }

        private IReadOnlyList<string> ExtractTexts(string content)
        {
            var document = new HtmlDocument();
            document.LoadHtml(content);

            _logger.LogInformation("Extracting text nodes");

            var texts = from node in document.DocumentNode.SelectNodes("//div[@class='lcb-body']//p//text()")
                        let text = node.InnerText
                        let readableText = System.Net.WebUtility.HtmlDecode(text)
                        select readableText;

            return texts.ToArray();
        }

        private async Task<string> TranslateContent(string contentToTranslate, Language toLanguage)
        {
            _logger.LogInformation("Translating to {LANGUAGE}", toLanguage);

            var response = await _translate.TranslateTextAsync(new TranslateTextRequest
            {
                SourceLanguageCode = LanguageMappings[Language.Swedish],
                TargetLanguageCode = LanguageMappings[toLanguage],
                Text = contentToTranslate
            });

            _logger.LogInformation("Translation to {LANGUAGE} complete", toLanguage);

            return response.TranslatedText;
        }

        private static readonly IReadOnlyDictionary<Language, string> LanguageMappings = new Dictionary<Language, string>
        {
            [Language.Danish] = "da",
            [Language.English] = "en",
            [Language.Finnish] = "fi",
            [Language.French] = "fr",
            [Language.German] = "de",
            [Language.Italian] = "it",
            [Language.Norwegian] = "no",
            [Language.Russian] = "ru",
            [Language.Swedish] = "sv"
        };

        private async Task StoreTranslations(string fileKey, IReadOnlyList<string> translatedContent)
        {
            _logger.LogInformation("Uploading to S3 (Bucket: {BUCKET}, FileKey: {KEY})", _options.ResultBucket, fileKey);

            await _s3.PutObjectAsync(new PutObjectRequest
            {
                Key = fileKey,
                BucketName = _options.ResultBucket,
                ContentType = "text/plain",
                ContentBody = MergeContent(translatedContent)
            });

            _logger.LogInformation("Upload to S3 complete (Bucket: {BUCKET}, FileKey: {KEY})", _options.ResultBucket, fileKey);
        }

        private string MergeContent(IReadOnlyList<string> contentToStore)
        {
            var sb = new StringBuilder();

            foreach (var text in contentToStore)
            {
                sb.AppendLine("----------------");
                sb.AppendLine(text);
            }

            return sb.ToString();
        }
    }

    public class TranslateOptions
    {
        public string ResultBucket { get; set; }
    }
}
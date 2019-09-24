using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QueueProcessor.Handlers;

namespace QueueProcessor.Services
{
    public class AmazonS3TranslationPersister : ITranslationPersister
    {
        private readonly IAmazonS3 _s3;
        private readonly TranslateOptions _options;
        private readonly ILogger<AmazonS3TranslationPersister> _logger;

        public AmazonS3TranslationPersister(IAmazonS3 s3, IOptions<TranslateOptions> options, ILogger<AmazonS3TranslationPersister> logger)
        {
            _s3 = s3 ?? throw new ArgumentNullException(nameof(s3));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PersistTranslations(string key, IReadOnlyList<string> translations)
        {
            _logger.LogInformation("Uploading to S3 (Bucket: {BUCKET}, FileKey: {KEY})", _options.ResultBucket, key);

            await _s3.PutObjectAsync(new PutObjectRequest
            {
                Key = key,
                BucketName = _options.ResultBucket,
                ContentType = "text/plain",
                ContentBody = MergeContent(translations)
            });

            _logger.LogInformation("Upload to S3 complete (Bucket: {BUCKET}, FileKey: {KEY})", _options.ResultBucket, key);
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
}
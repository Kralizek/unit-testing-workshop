using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Translate;
using Amazon.Translate.Model;
using Microsoft.Extensions.Logging;
using QueueProcessor.Handlers;
using QueueProcessor.Messages;

namespace QueueProcessor.Services {
    public class AmazonTranslateTranslator : ITranslator
    {
        private readonly IAmazonTranslate _translate;
        private readonly ILogger<AmazonTranslateTranslator> _logger;

        public AmazonTranslateTranslator(IAmazonTranslate translate, ILogger<AmazonTranslateTranslator> logger)
        {
            _translate = translate ?? throw new ArgumentNullException(nameof(translate));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> TranslateText(string textToTranslate, Language toLanguage)
        {
            _logger.LogInformation("Translating to {LANGUAGE}", toLanguage);

            var response = await _translate.TranslateTextAsync(new TranslateTextRequest
            {
                SourceLanguageCode = LanguageMappings[Language.Swedish],
                TargetLanguageCode = LanguageMappings[toLanguage],
                Text = textToTranslate
            });

            _logger.LogInformation("Translation to {LANGUAGE} complete", toLanguage);

            return response.TranslatedText;
        }

        public string GetLanguageCode(Language language) => LanguageMappings[language];

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
    }
}
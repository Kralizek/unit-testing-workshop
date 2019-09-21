using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nybus;
using Nybus.Utils;
using QueueProcessor.Messages;

namespace QueueProcessor
{
    public class ImprovedTranslateCommandHandler : ICommandHandler<TranslateCommand>
    {
        private readonly IFileDownloader _downloader;
        private readonly ITextExtractor _textExtractor;
        private readonly ITranslator _translator;
        private readonly ITranslationPersister _persister;

        public ImprovedTranslateCommandHandler(IFileDownloader downloader, ITextExtractor textExtractor, ITranslator translator, ITranslationPersister persister)
        {
            _downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
            _textExtractor = textExtractor ?? throw new ArgumentNullException(nameof(textExtractor));
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
            _persister = persister ?? throw new ArgumentNullException(nameof(persister));
        }

        public async Task HandleAsync(IDispatcher dispatcher, ICommandContext<TranslateCommand> context)
        {
            if (context.Command.ToLanguage == Language.ChineseSimplified)
            {
                throw new ArgumentOutOfRangeException(nameof(context.Command.ToLanguage), "Chinese not supported");
            }

            var uriToTranslate = new Uri($@"https://www.studentum.se/education/{context.Command.EducationId}");

            var content = await _downloader.GetContent(uriToTranslate);

            var contentToTranslate = _textExtractor.ExtractText(content);

            var translations = new List<string>();

            foreach (var text in contentToTranslate)
            {
                var translatedText = await _translator.TranslateText(text, context.Command.ToLanguage);
                translations.Add(translatedText);
            }

            var fileKey = GenerateKey(context.Command);

            await _persister.PersistTranslations(fileKey, translations);

            await dispatcher.RaiseEventAsync(new TranslatedEvent
            {
                EducationId = context.Command.EducationId,
                ToLanguage = context.Command.ToLanguage,
                TranslationFileKey = fileKey
            });
        }

        private string GenerateKey(TranslateCommand command)
        {
            var now = Clock.Default.Now;

            var fileKey = $"translations/{now.Year:00}/{now.Month:00}/{now.Day:00}/{command.EducationId:000000000000}/{_translator.GetLanguageCode(command.ToLanguage)}";
            return fileKey;
        }
    }

    public interface IFileDownloader
    {
        Task<string> GetContent(Uri target);
    }

    public interface ITextExtractor
    {
        IReadOnlyList<string> ExtractText(string text);
    }

    public interface ITranslator
    {
        Task<string> TranslateText(string textToTranslate, Language toLanguage);

        string GetLanguageCode(Language language);
    }

    public interface ITranslationPersister
    {
        Task PersistTranslations(string key, IReadOnlyList<string> translations);
    }
}
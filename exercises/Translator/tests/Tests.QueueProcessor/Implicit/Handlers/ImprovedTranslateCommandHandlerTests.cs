using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Idioms;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Nybus;
using QueueProcessor.Handlers;
using QueueProcessor.Messages;

namespace Tests.Implicit.Handlers
{
    [TestFixture]
    public class ImprovedTranslateCommandHandlerTests
    {
        [Test, MyAutoData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(ImprovedTranslateCommandHandler).GetConstructors());
        }

        [Test, MyAutoData]
        public async Task HandleAsync_uses_downloader_to_download_file([Frozen] IEducationProfileDownloader downloader, ImprovedTranslateCommandHandler sut, IDispatcher dispatcher, ICommandContext<TranslateEducationCommand> context)
        {
            await sut.HandleAsync(dispatcher, context);

            Mock.Get(downloader).Verify(p => p.GetProfile(context.Command.EducationId));
        }

        [Test, MyAutoData]
        public async Task HandleAsync_uses_extractor_to_extract_paragraphs([Frozen] IEducationProfileDownloader downloader, [Frozen] ITextExtractor extractor, ImprovedTranslateCommandHandler sut, IDispatcher dispatcher, ICommandContext<TranslateEducationCommand> context, string content)
        {
            Mock.Get(downloader).Setup(p => p.GetProfile(It.IsAny<int>())).ReturnsAsync(content);

            await sut.HandleAsync(dispatcher, context);

            Mock.Get(extractor).Verify(p => p.ExtractText(content));
        }

        [Test, MyAutoData]
        public async Task HandleAsync_uses_translator_to_translate_text([Frozen] ITextExtractor extractor, [Frozen] ITranslator translator, ImprovedTranslateCommandHandler sut, IDispatcher dispatcher, ICommandContext<TranslateEducationCommand> context, string[] paragraphs)
        {
            Mock.Get(extractor).Setup(p => p.ExtractText(It.IsAny<string>())).Returns(paragraphs);

            await sut.HandleAsync(dispatcher, context);

            foreach (var text in paragraphs)
            {
                Mock.Get(translator).Verify(p => p.TranslateText(text, context.Command.ToLanguage));
            }
        }

        [Test, MyAutoData]
        public async Task HandleAsync_uses_persister_to_store_text([Frozen] ITranslationPersister persister, ImprovedTranslateCommandHandler sut, IDispatcher dispatcher, ICommandContext<TranslateEducationCommand> context)
        {
            await sut.HandleAsync(dispatcher, context);

            Mock.Get(persister).Verify(p => p.PersistTranslations(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()));
        }

        [Test, MyAutoData]
        public async Task HandleAsync_raises_event_when_completed(SingleTranslateCommandHandler sut, IDispatcher dispatcher, ICommandContext<TranslateEducationCommand> context)
        {
            await sut.HandleAsync(dispatcher, context);

            Mock.Get(dispatcher).Verify(p => p.RaiseEventAsync(It.Is<EducationTranslatedEvent>(te => te.EducationId == context.Command.EducationId && te.ToLanguage == context.Command.ToLanguage), It.IsAny<IDictionary<string, string>>()));
        }
    }
}
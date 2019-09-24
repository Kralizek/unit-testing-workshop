using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using Nybus;
using QueueProcessor.Handlers;
using QueueProcessor.Messages;

namespace Tests.Explicit.Handlers
{
    [TestFixture]
    public class ImprovedTranslateCommandHandlerTests
    {
        private IFixture _fixture;
        private Mock<IEducationProfileDownloader> _mockDownloader;
        private Mock<ITextExtractor> _mockTextExtractor;
        private Mock<ITranslator> _mockTranslator;
        private Mock<ITranslationPersister> _mockPersister;

        private Mock<IDispatcher> _mockDispatcher;

        [SetUp]
        public void Initialize()
        {
            _fixture = new Fixture();

            _mockDownloader = new Mock<IEducationProfileDownloader>();
            _mockTextExtractor = new Mock<ITextExtractor>();
            _mockPersister = new Mock<ITranslationPersister>();
            _mockTranslator = new Mock<ITranslator>();
            _mockDispatcher = new Mock<IDispatcher>();
        }

        #region Constructors

        [Test]
        public void Downloader_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new ImprovedTranslateCommandHandler(null, _mockTextExtractor.Object, _mockTranslator.Object, _mockPersister.Object));
        }

        [Test]
        public void TextExtractor_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new ImprovedTranslateCommandHandler(_mockDownloader.Object, null, _mockTranslator.Object, _mockPersister.Object));
        }

        [Test]
        public void Translator_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new ImprovedTranslateCommandHandler(_mockDownloader.Object, _mockTextExtractor.Object, null, _mockPersister.Object));
        }

        [Test]
        public void TranslationPersister_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new ImprovedTranslateCommandHandler(_mockDownloader.Object, _mockTextExtractor.Object, _mockTranslator.Object, null));
        }

        #endregion

        #region Helper methods

        private ImprovedTranslateCommandHandler CreateSystemUnderTest()
        {
            return new ImprovedTranslateCommandHandler(_mockDownloader.Object, _mockTextExtractor.Object, _mockTranslator.Object, _mockPersister.Object);
        }

        private ICommandContext<TranslateEducationCommand> CreateCommandContext()
        {
            var command = _fixture.Create<TranslateEducationCommand>();

            var commandMessage = new CommandMessage<TranslateEducationCommand>
            {
                Command = command,
                Headers = new HeaderBag
                {
                    CorrelationId = Guid.NewGuid(),
                    SentOn = DateTimeOffset.UtcNow
                },
                MessageId = Guid.NewGuid().ToString()
            };

            return new NybusCommandContext<TranslateEducationCommand>(commandMessage);
        }

        #endregion

        #region HandleAsync

        [Test]
        public async Task HandleAsync_uses_downloader_to_download_file()
        {
            var context = _fixture.Create<NybusCommandContext<TranslateEducationCommand>>();

            _mockDownloader.Setup(p => p.GetProfile(It.IsAny<int>())).ReturnsAsync(_fixture.Create<string>());

            _mockTextExtractor.Setup(p => p.ExtractText(It.IsAny<string>())).Returns(_fixture.Create<List<string>>());

            _mockTranslator.Setup(p => p.GetLanguageCode(It.IsAny<Language>())).Returns(_fixture.Create<string>());

            _mockTranslator.Setup(p => p.TranslateText(It.IsAny<string>(), It.IsAny<Language>())).ReturnsAsync(_fixture.Create<string>());

            var sut = CreateSystemUnderTest();

            await sut.HandleAsync(_mockDispatcher.Object, context);

            _mockDownloader.Verify(p => p.GetProfile(context.Command.EducationId));
        }

        [Test]
        public async Task HandleAsync_uses_extractor_to_extract_paragraphs()
        {
            var context = _fixture.Create<NybusCommandContext<TranslateEducationCommand>>();

            var content = _fixture.Create<string>();

            _mockDownloader.Setup(p => p.GetProfile(It.IsAny<int>())).ReturnsAsync(content);

            _mockTextExtractor.Setup(p => p.ExtractText(It.IsAny<string>())).Returns(_fixture.Create<List<string>>());

            _mockTranslator.Setup(p => p.GetLanguageCode(It.IsAny<Language>())).Returns(_fixture.Create<string>());

            _mockTranslator.Setup(p => p.TranslateText(It.IsAny<string>(), It.IsAny<Language>())).ReturnsAsync(_fixture.Create<string>());

            var sut = CreateSystemUnderTest();

            await sut.HandleAsync(_mockDispatcher.Object, context);

            _mockTextExtractor.Verify(p => p.ExtractText(content));
        }

        [Test]
        public async Task HandleAsync_uses_translator_to_translate_text()
        {
            var context = _fixture.Create<NybusCommandContext<TranslateEducationCommand>>();

            _mockDownloader.Setup(p => p.GetProfile(It.IsAny<int>())).ReturnsAsync(_fixture.Create<string>());

            var list = _fixture.Create<List<string>>();

            _mockTextExtractor.Setup(p => p.ExtractText(It.IsAny<string>())).Returns(list);

            _mockTranslator.Setup(p => p.GetLanguageCode(It.IsAny<Language>())).Returns(_fixture.Create<string>());

            _mockTranslator.Setup(p => p.TranslateText(It.IsAny<string>(), It.IsAny<Language>())).ReturnsAsync(_fixture.Create<string>());

            var sut = CreateSystemUnderTest();

            await sut.HandleAsync(_mockDispatcher.Object, context);

            foreach (var text in list)
            {
                _mockTranslator.Verify(p => p.TranslateText(text, context.Command.ToLanguage));
            }
        }

        [Test]
        public async Task HandleAsync_uses_persister_to_store_text()
        {
            var context = _fixture.Create<NybusCommandContext<TranslateEducationCommand>>();

            _mockDownloader.Setup(p => p.GetProfile(It.IsAny<int>())).ReturnsAsync(_fixture.Create<string>());

            _mockTextExtractor.Setup(p => p.ExtractText(It.IsAny<string>())).Returns(_fixture.Create<List<string>>());

            _mockTranslator.Setup(p => p.GetLanguageCode(It.IsAny<Language>())).Returns(_fixture.Create<string>());

            _mockTranslator.Setup(p => p.TranslateText(It.IsAny<string>(), It.IsAny<Language>())).ReturnsAsync(_fixture.Create<string>());

            var sut = CreateSystemUnderTest();

            await sut.HandleAsync(_mockDispatcher.Object, context);

            _mockPersister.Verify(p => p.PersistTranslations(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()));
        }

        [Test]
        public async Task HandleAsync_raises_event_when_complete()
        {
            var context = _fixture.Create<NybusCommandContext<TranslateEducationCommand>>();

            _mockDownloader.Setup(p => p.GetProfile(It.IsAny<int>())).ReturnsAsync(_fixture.Create<string>());

            _mockTextExtractor.Setup(p => p.ExtractText(It.IsAny<string>())).Returns(_fixture.Create<List<string>>());

            _mockTranslator.Setup(p => p.GetLanguageCode(It.IsAny<Language>())).Returns(_fixture.Create<string>());

            _mockTranslator.Setup(p => p.TranslateText(It.IsAny<string>(), It.IsAny<Language>())).ReturnsAsync(_fixture.Create<string>());

            var sut = CreateSystemUnderTest();

            await sut.HandleAsync(_mockDispatcher.Object, context);

            _mockDispatcher.Verify(p => p.RaiseEventAsync(It.Is<EducationTranslatedEvent>(te => te.EducationId == context.Command.EducationId && te.ToLanguage == context.Command.ToLanguage), It.IsAny<IDictionary<string, string>>()));
        }

        #endregion
    }
}
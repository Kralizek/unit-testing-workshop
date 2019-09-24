using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.Translate;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Nybus;
using QueueProcessor.Handlers;
using QueueProcessor.Messages;
using WorldDomination.Net.Http;

namespace Tests.Explicit.Handlers
{
    [TestFixture]
    public class SingleTranslateCommandHandlerTests
    {
        const string HtmlFormat = @"<html><body><div class=""lcb-body""><p>{0}</p></div></body></html>";

        private IFixture _fixture;

        private Mock<IAmazonTranslate> _mockTranslate;
        private Mock<IAmazonS3> _mockS3;
        private Mock<IDispatcher> _mockDispatcher;

        [SetUp]
        public void Initialize()
        {
            _fixture = new Fixture();

            _mockTranslate = new Mock<IAmazonTranslate>();

            _mockS3 = new Mock<IAmazonS3>();

            _mockDispatcher = new Mock<IDispatcher>();
        }

        [Test]
        public void HttpClient_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new SingleTranslateCommandHandler(null, Mock.Of<IAmazonTranslate>(), Mock.Of<IAmazonS3>(), Mock.Of<IOptions<TranslateOptions>>(), Mock.Of<ILogger<SingleTranslateCommandHandler>>()));
        }

        [Test, Ignore("This test is not finished yet")]
        public void AmazonTranslate_is_required()
        {

        }

        [Test, Ignore("This test is not finished yet")]
        public void AmazonS3_is_required()
        {

        }

        [Test, Ignore("This test is not finished yet")]
        public void TranslateOptions_is_required()
        {

        }

        [Test, Ignore("This test is not finished yet")]
        public void Logger_is_required()
        {

        }

        private SingleTranslateCommandHandler CreateSystemUnderTest(TranslateOptions options, params HttpMessageOptions[] httpOptions)
        {
            var handler = new FakeHttpMessageHandler(httpOptions);
            var http = new HttpClient(handler);

            var wrappedOptions = new OptionsWrapper<TranslateOptions>(options);

            return new SingleTranslateCommandHandler(http, _mockTranslate.Object, _mockS3.Object, wrappedOptions, Mock.Of<ILogger<SingleTranslateCommandHandler>>());
        }

        [Test]
        public void HandleAsync_throws_if_Italian_translation()
        {
            var httpOption = new HttpMessageOptions
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            };

            var options = _fixture.Create<TranslateOptions>();

            var sut = CreateSystemUnderTest(options, httpOption);

            var context = _fixture.Create<NybusCommandContext<TranslateEducationCommand>>();
            context.Command.ToLanguage = Language.Italian;

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.HandleAsync(_mockDispatcher.Object, context));
        }


        [Test, Ignore("This test is not finished yet")]
        public async Task HandleAsync_downloads_the_proper_Education_profile()
        {
            // ARRANGE

            var text = _fixture.Create<string>();

            var httpOption = new HttpMessageOptions
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(text)
                }
            };

            var options = _fixture.Create<TranslateOptions>();

            var sut = CreateSystemUnderTest(options, httpOption);

            var context = _fixture.Create<NybusCommandContext<TranslateEducationCommand>>();

            // continue...

            // ACT

            await sut.HandleAsync(_mockDispatcher.Object, context);

            // ASSERT

            Assert.That(httpOption.HttpResponseMessage.RequestMessage.RequestUri.ToString(), Is.EqualTo(string.Format(SingleTranslateCommandHandler.EducationProfileFormat, context.Command.EducationId)));
        }

        [Test, Ignore("This test is not finished yet")]
        public async Task HandleAsync_uses_Amazon_Translate_to_translate_text()
        {

        }

        [Test, Ignore("This test is not finished yet")]
        public async Task HandleAsync_uses_Amazon_S3_to_store_translations()
        {

        }

        [Test, Ignore("This test is not finished yet")]
        public async Task HandleAsync_raises_event_when_completed()
        {

        }
    }
}
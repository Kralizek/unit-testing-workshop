using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Translate;
using Amazon.Translate.Model;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using QueueProcessor.Messages;
using QueueProcessor.Services;

namespace Tests.Explicit.Services
{
    [TestFixture]
    public class AmazonTranslateTranslatorTests
    {
        private IFixture _fixture;
        private Mock<IAmazonTranslate> _mockTranslate;

        [SetUp]
        public void Initialize()
        {
            _fixture = new Fixture();

            _mockTranslate = new Mock<IAmazonTranslate>();
        }

        [Test]
        public void Translate_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new AmazonTranslateTranslator(null, Mock.Of<ILogger<AmazonTranslateTranslator>>()));
        }

        [Test]
        public void Logger_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new AmazonTranslateTranslator(Mock.Of<IAmazonTranslate>(), null));
        }

        private AmazonTranslateTranslator CreateSystemUnderTest()
        {
            return new AmazonTranslateTranslator(_mockTranslate.Object, Mock.Of<ILogger<AmazonTranslateTranslator>>());
        }

        [Test]
        public async Task TranslateText_uses_Amazon_Translate([Values] Language toLanguage) // see: https://github.com/nunit/docs/wiki/Values-Attribute
        {
            // skips the test if the condition is not matched
            Assume.That(toLanguage, Is.Not.EqualTo(Language.Italian));

            var response = _fixture.Create<TranslateTextResponse>();

            _mockTranslate.Setup(p => p.TranslateTextAsync(It.IsAny<TranslateTextRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

            var sut = CreateSystemUnderTest();

            var textToTranslate = _fixture.Create<string>();

            await sut.TranslateText(textToTranslate, toLanguage);

            _mockTranslate.Verify(p => p.TranslateTextAsync(It.Is<TranslateTextRequest>(ttr => ttr.Text == textToTranslate && ttr.SourceLanguageCode == "sv" && ttr.TargetLanguageCode == sut.GetLanguageCode(toLanguage)), It.IsAny<CancellationToken>()));
        }

        [Test]
        public void Italian_is_not_supported()
        {
            var sut = CreateSystemUnderTest();

            var textToTranslate = _fixture.Create<string>();

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.TranslateText(textToTranslate, Language.Italian));
        }
    }
}
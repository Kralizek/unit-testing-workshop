using System;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using QueueProcessor.Services;

namespace Tests.Explicit.Services
{
    [TestFixture]
    public class HtmlTextExtractorTests
    {
        const string HtmlFormat = @"<html><body><div class=""lcb-body""><p>{0}</p></div></body></html>";

        private IFixture _fixture;

        [SetUp]
        public void Initialize()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Logger_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new HtmlTextExtractor(null));
        }

        private HtmlTextExtractor CreateSystemUnderTest()
        {
            return new HtmlTextExtractor(Mock.Of<ILogger<HtmlTextExtractor>>());
        }

        [Test]
        public void ExtractText_extracts_text_in_paragraphs()
        {
            var sut = CreateSystemUnderTest();

            var textToMatch = _fixture.Create<string>();

            var result = sut.ExtractText(string.Format(HtmlFormat, textToMatch));

            Assert.That(result, Contains.Item(textToMatch));
        }
    }
}
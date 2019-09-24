using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using QueueProcessor.Services;
using WorldDomination.Net.Http;

namespace Tests.Explicit.Services
{
    [TestFixture]
    public class HttpClientEducationProfileDownloaderTests
    {
        private IFixture _fixture;

        [SetUp]
        public void Initialize()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void HttpClient_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpClientEducationProfileDownloader(null, Mock.Of<ILogger<HttpClientEducationProfileDownloader>>()));
        }

        [Test]
        public void Logger_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpClientEducationProfileDownloader(new HttpClient(), null));
        }

        private HttpClientEducationProfileDownloader CreateSystemUnderTest(params HttpMessageOptions[] options)
        {
            var handler = new FakeHttpMessageHandler(options);
            var http = new HttpClient(handler);

            return new HttpClientEducationProfileDownloader(http, Mock.Of<ILogger<HttpClientEducationProfileDownloader>>());
        }


        [Test]
        public async Task GetProfile_requests_proper_profile()
        {
            var content = _fixture.Create<string>();

            var httpOption = new HttpMessageOptions
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(content)
                }
            };

            var sut = CreateSystemUnderTest(httpOption);

            var educationId = _fixture.Create<int>();

            var result = await sut.GetProfile(educationId);

            Assert.That(httpOption.HttpResponseMessage.RequestMessage.RequestUri.ToString(), Is.EqualTo(string.Format(HttpClientEducationProfileDownloader.EducationUrlFormat, educationId)));
        }

        [Test]
        public async Task GetProfile_downloads_returned_content()
        {
            var content = _fixture.Create<string>();

            var httpOption = new HttpMessageOptions
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(content)
                }
            };

            var sut = CreateSystemUnderTest(httpOption);

            var educationId = _fixture.Create<int>();

            var result = await sut.GetProfile(educationId);

            Assert.That(result, Is.EqualTo(content));
        }
    }
}
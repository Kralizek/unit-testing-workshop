using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using QueueProcessor.Handlers;
using QueueProcessor.Services;

namespace Tests.Explicit.Services
{
    [TestFixture]
    public class AmazonS3TranslationPersisterTests
    {
        private IFixture _fixture;
        private Mock<IAmazonS3> _mockS3;

        [SetUp]
        public void Initialize()
        {
            _fixture = new Fixture();

            _mockS3 = new Mock<IAmazonS3>();
        }

        [Test]
        public void S3_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new AmazonS3TranslationPersister(null, Mock.Of<IOptions<TranslateOptions>>(), Mock.Of<ILogger<AmazonS3TranslationPersister>>()));
        }

        [Test]
        public void TranslateOptions_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new AmazonS3TranslationPersister(Mock.Of<IAmazonS3>(), null, Mock.Of<ILogger<AmazonS3TranslationPersister>>()));
        }

        [Test]
        public void Logger_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new AmazonS3TranslationPersister(Mock.Of<IAmazonS3>(), Mock.Of<IOptions<TranslateOptions>>(), null));
        }

        private AmazonS3TranslationPersister CreateSystemUnderTest(TranslateOptions options)
        {
            var wrappedOptions = new OptionsWrapper<TranslateOptions>(options);
            return new AmazonS3TranslationPersister(_mockS3.Object, wrappedOptions, Mock.Of<ILogger<AmazonS3TranslationPersister>>());
        }

        [Test]
        public async Task PersistTranslations_pushes_to_s3()
        {
            var options = _fixture.Create<TranslateOptions>();

            var key = _fixture.Create<string>();

            var translations = _fixture.Create<string[]>();

            var response = _fixture.Build<PutObjectResponse>().OmitAutoProperties().With(p => p.HttpStatusCode).Create();

            _mockS3.Setup(p => p.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

            var sut = CreateSystemUnderTest(options);

            await sut.PersistTranslations(key, translations);

            _mockS3.Verify(p => p.PutObjectAsync(It.Is<PutObjectRequest>(por => por.Key == key && por.BucketName == options.ResultBucket && translations.All(t => por.ContentBody.Contains(t))), It.IsAny<CancellationToken>()));
        }
    }
}
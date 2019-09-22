using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Translate;
using Amazon.Translate.Model;
using AutoFixture.Idioms;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Nybus;
using QueueProcessor.Handlers;
using QueueProcessor.Messages;
using WorldDomination.Net.Http;

namespace Tests.Implicit.Handlers
{
    [TestFixture]
    public class SingleTranslateCommandHandlerTests
    {
        

        [Test, MyAutoData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(SingleTranslateCommandHandler).GetConstructors());
        }

        [Test, MyAutoData]
        public void HandleAsync_throws_if_Italian_translation(SingleTranslateCommandHandler sut, IDispatcher dispatcher, ICommandContext<TranslateEducationCommand> context)
        {
            context.Command.ToLanguage = Language.Italian;

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.HandleAsync(dispatcher, context));
        }

        [Test, MyAutoData]
        public async Task HandleAsync_downloads_the_proper_Education_profile([Frozen] HttpMessageOptions httpOption, SingleTranslateCommandHandler sut, IDispatcher dispatcher, ICommandContext<TranslateEducationCommand> context)
        {
            await sut.HandleAsync(dispatcher, context);

            var expectedRequestUri = string.Format(SingleTranslateCommandHandler.EducationProfileFormat, context.Command.EducationId);

            Assert.That(httpOption.HttpResponseMessage.RequestMessage.RequestUri.ToString(), Is.EqualTo(expectedRequestUri));
        }

        [Test, MyAutoData]
        public async Task HandleAsync_uses_Amazon_Translate_to_translate_text([Frozen] IAmazonTranslate translate, SingleTranslateCommandHandler sut, IDispatcher dispatcher, ICommandContext<TranslateEducationCommand> context)
        {
            await sut.HandleAsync(dispatcher, context);

            Mock.Get(translate).Verify(p => p.TranslateTextAsync(It.IsAny<TranslateTextRequest>(), It.IsAny<CancellationToken>()));
        }

        [Test, MyAutoData]
        public async Task HandleAsync_uses_Amazon_S3_to_store_translations([Frozen] IAmazonS3 s3, SingleTranslateCommandHandler sut, IDispatcher dispatcher, ICommandContext<TranslateEducationCommand> context)
        {
            await sut.HandleAsync(dispatcher, context);

            Mock.Get(s3).Verify(p => p.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()));
        }

        [Test, MyAutoData]
        public async Task HandleAsync_raises_event_when_completed(SingleTranslateCommandHandler sut, IDispatcher dispatcher, ICommandContext<TranslateEducationCommand> context)
        {
            await sut.HandleAsync(dispatcher, context);

            Mock.Get(dispatcher).Verify(p => p.RaiseEventAsync(It.Is<EducationTranslatedEvent>(te => te.EducationId == context.Command.EducationId && te.ToLanguage == context.Command.ToLanguage), It.IsAny<IDictionary<string, string>>()));
        }
    }
}
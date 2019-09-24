using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Amazon.S3.Model;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using QueueProcessor.Handlers;
using WorldDomination.Net.Http;

namespace Tests
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MyAutoDataAttribute : AutoDataAttribute
    {
        public MyAutoDataAttribute() : base(FixtureHelpers.CreateFixture) { }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class MyInlineAutoDataAttribute : InlineAutoDataAttribute
    {
        public MyInlineAutoDataAttribute(params object[] arguments) : base(FixtureHelpers.CreateFixture, arguments) { }

    }

    public static class FixtureHelpers
    {
        public static IFixture CreateFixture()
        {
            var fixture = new Fixture();

            fixture.Customize(new AutoMoqCustomization
            {
                ConfigureMembers = true,
                GenerateDelegates = true
            });

            #region Customizations

            fixture.Customize<PutObjectResponse>(o => o.OmitAutoProperties().With(p => p.HttpStatusCode));

            fixture.Customize<HttpMessageOptions>(o => o.OmitAutoProperties());

            fixture.Customize<HttpClient>(o => o.FromFactory((string text, HttpMessageOptions option) =>
            {
                option.HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent($@"<html><body><div class=""lcb-body""><p>{text}</p></div></body></html>", Encoding.UTF8, "text/html")
                };

                var http = new HttpClient(new FakeHttpMessageHandler(option));

                return http;
            }));

            #endregion

            return fixture;
        }
    }
}
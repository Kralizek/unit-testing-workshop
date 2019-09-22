using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using QueueProcessor.Handlers;

namespace QueueProcessor.Services {
    public class HtmlTextExtractor : ITextExtractor
    {
        private readonly ILogger<HtmlTextExtractor> _logger;

        public HtmlTextExtractor(ILogger<HtmlTextExtractor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IReadOnlyList<string> ExtractText(string text)
        {
            var document = new HtmlDocument();
            document.LoadHtml(text);

            _logger.LogInformation("Extracting text nodes");

            var texts = from node in document.DocumentNode.SelectNodes("//div[@class='lcb-body']//p//text()")
                        let innerText = node.InnerText
                        let readableText = System.Net.WebUtility.HtmlDecode(innerText)
                        select readableText;

            return texts.ToArray();
        }
    }
}
using System;
using Nybus;

namespace QueueProcessor.Messages
{
    [Message("TranslatedEvent", "Examples")]
    public class TranslatedEvent : IEvent
    {
        public int EducationId { get; set; }

        public Language FromLanguage { get; set; }

        public Language ToLanguage { get; set; }

        public string TranslationFileKey { get; set; }

    }
}
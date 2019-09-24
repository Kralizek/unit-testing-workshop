using System;
using Nybus;

namespace QueueProcessor.Messages
{
    [Message("EducationTranslatedEvent", "Examples")]
    public class EducationTranslatedEvent : IEvent
    {
        public int EducationId { get; set; }

        public Language ToLanguage { get; set; }

        public string TranslationFileKey { get; set; }

    }
}
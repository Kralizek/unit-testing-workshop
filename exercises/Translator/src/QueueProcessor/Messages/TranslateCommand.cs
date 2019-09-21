using System;
using System.Collections.Generic;
using System.Text;
using Nybus;

namespace QueueProcessor.Messages
{
    [Message("TranslateCommand", "Examples")]
    public class TranslateCommand : ICommand
    {
        public int EducationId { get; set; }

        public Language ToLanguage { get; set; }
    }

    // https://docs.aws.amazon.com/translate/latest/dg/what-is.html
    public enum Language
    {
        English = 1,
        German = 2,
        Swedish = 3,
        Norwegian = 4,
        Finnish = 5,
        Danish = 6,
        French = 7,
        Italian = 8,
        Russian = 9,
        ChineseSimplified = 10
    }
}

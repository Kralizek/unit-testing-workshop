using System.Threading.Tasks;
using AutoFixture.Idioms;
using NUnit.Framework;

namespace Tests.Implicit.Services
{
    [TestFixture]
    public class AmazonTranslateTranslatorTests
    {
        [Test, MyAutoData, Ignore("This test is not finished yet")]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {

        }

        [Test, MyAutoData, Ignore("This test is not finished yet")]
        public async Task TranslateText_uses_Amazon_Translate()
        {

        }

        [Test, MyAutoData, Ignore("This test is not finished yet")]
        public async Task Italian_is_not_supported()
        {

        }
    }
}
using System.Threading.Tasks;
using AutoFixture.Idioms;
using NUnit.Framework;

namespace Tests.Implicit.Services
{
    [TestFixture]
    public class AmazonS3TranslationPersisterTests
    {
        [Test, MyAutoData, Ignore("This test is not finished yet")]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {

        }

        [Test, MyAutoData, Ignore("This test is not finished yet")]
        public async Task PersistTranslations_pushes_to_s3()
        {

        }
    }
}
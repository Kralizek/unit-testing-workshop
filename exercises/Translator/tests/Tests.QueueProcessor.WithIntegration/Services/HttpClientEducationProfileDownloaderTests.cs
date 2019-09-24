using System.Threading.Tasks;
using AutoFixture.Idioms;
using NUnit.Framework;

namespace Tests.Implicit.Services
{
    [TestFixture]
    public class HttpClientEducationProfileDownloaderTests
    {
        [Test, MyAutoData, Ignore("This test is not finished yet")]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {

        }

        [Test, MyAutoData, Ignore("This test is not finished yet")]
        public async Task GetProfile_requests_proper_profile() { }

        [Test, MyAutoData, Ignore("This test is not finished yet")]
        public async Task GetProfile_downloads_returned_content() { }
    }
}
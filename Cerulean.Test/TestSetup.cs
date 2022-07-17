namespace Cerulean.Test
{
    [SetUpFixture]
    public class TestSetup
    {
        private readonly CeruleanAPI _api = CeruleanAPI.GetAPI();

        [OneTimeSetUp]
        public void Setup()
        {
            _api.UseGraphicsFactory(new MockGraphicsFactory())
                .Initialize();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _api.Quit();
        }
    }
}

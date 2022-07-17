namespace Cerulean.Test
{
    [TestFixture]
    [NonParallelizable]
    public class APIWindowingTests
    {
        private readonly CeruleanAPI _api = CeruleanAPI.GetAPI();

        [Test]
        public void CreateWindowThenClose_NewLayout_ReturnOK()
        {
            Assert.DoesNotThrow(() =>
            {
                var layout = new Layout();
                var window = _api.CreateWindow(layout,
                    "CreateWindowThenClose_NewLayout_ReturnOK");
                window.Close();
            });
        }

        [Test]
        public void CreateWindowThenClose_EmbeddedLayout_ReturnOK()
        {
            Assert.DoesNotThrow(() =>
            {
                var layout = new Layout();
                var window = _api.CreateWindow("EmbeddedLayoutSample",
                    "CreateWindow_EmbeddedLayout_ReturnOK");
                window.Close();
            });
        }

        [Test]
        public void CreateWindowThenClose_EmbeddedLayoutMissing_ThrowsGeneralAPIException()
        {
            Assert.Throws(typeof(GeneralAPIException), () =>
            {
                var layout = new Layout();
                var window = _api.CreateWindow("NonExistentLayout",
                    "CreateWindow_EmbeddedLayoutMissing_ThrowsGeneralAPIException");
                window.Close();
            });
        }
    }
}
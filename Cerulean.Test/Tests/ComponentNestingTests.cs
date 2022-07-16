using Microsoft.CSharp.RuntimeBinder;

namespace Cerulean.Test
{
    [TestFixture]
    [NonParallelizable]
    public class ComponentNestingTests
    {
        private readonly CeruleanAPI _api = CeruleanAPI.GetAPI();

        [Test]
        public void AddPanel_ReturnOK()
        {
            Assert.DoesNotThrow(() =>
            {
                var window = _api.CreateWindow(new Layout());
                window.Layout.AddChild("Panel", new Panel
                {
                    Size = new Size(300, 300),
                    BackColor = new Color(100, 100, 100),
                    X = 4,
                    Y = 4
                });
                window.Close();
            });
        }

        [Test]
        public void NestLabel_FromPanel1_ReturnOK()
        {
            Assert.DoesNotThrow(() =>
            {
                var window = _api.CreateWindow(new Layout());
                window.Layout.AddChild("Panel1", new Panel
                {
                    Size = new Size(300, 300),
                    BackColor = new Color(100, 100, 100),
                    X = 4,
                    Y = 4
                });
                window.Layout.Panel1.AddChild("Label", new Label
                {
                    Text = "This is a test.",
                    ForeColor = new Color(255, 255, 255),
                    X = 4,
                    Y = 4
                });
                window.Close();
            });
        }

        [Test]
        public void NestLabel_FromPanelFromEmbeddedLayout_ReturnOK()
        {
            Assert.DoesNotThrow(() =>
            {
                var window = _api.CreateWindow("EmbeddedLayoutSample");
                window.Layout.ContentPanel.AddChild("NewLabel", new Label
                {
                    Text = "This is a test.",
                    ForeColor = new Color(255, 255, 255),
                    X = 16,
                    Y = 32
                });
                window.Close();
            });
        }

        [Test]
        public void NestLabel_ToMissingComponent_ThrowsRuntimeBinderException()
        {
            var window = _api.CreateWindow("EmbeddedLayoutSample");
            Assert.Throws(typeof(RuntimeBinderException), () =>
            {
                window.Layout.ToolsPanel.AddChild("NewLabel", new Label
                {
                    Text = "These are tools.",
                    ForeColor = new Color(255, 255, 255),
                    X = 16,
                    Y = 32
                });
            });
            window.Close();
        }
    }
}

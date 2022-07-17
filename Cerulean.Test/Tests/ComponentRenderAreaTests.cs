using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;

namespace Cerulean.Test
{
    [TestFixture]
    [NonParallelizable]
    public class ComponentRenderAreaTests
    {
        private readonly CeruleanAPI _api = CeruleanAPI.GetAPI();
        private Window? _window;

        [OneTimeSetUp]
        public void Setup()
        {
            dynamic layout = new Layout();
            layout.AddChild("FirstPanel", new Panel
            {
                X = 4,
                Y = 4,
                Size = new Size(256, 198)
            });
            layout.FirstPanel.AddChild("SecondPanel", new Panel
            {
                X = 32,
                Y = 64,
                Size = new Size(128, 256)
            });
            _window = _api.CreateWindow(layout);
            layout.FirstPanel.SecondPanel.AddChild("ThirdPanel", new Panel
            {
                X = 8,
                Y = 16,
                Size = new Size(64, 64)
            });
            _window = _api.CreateWindow(layout);
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            _window?.Close();
        }

        [Test]
        public void VerifyFirstPanelPosition_MustBe_4_4()
        {
            var done = false;
            Exception? exception = null;
            Panel firstPanel = _window!.Layout.FirstPanel;
            firstPanel.RegisterHook(EventHook.BeforeDraw, (component, args) =>
            {
                try
                {
                    var graphics = (IGraphics)args[0];
                    graphics.GetGlobalPosition(out var x, out var y);
                    Assert.Multiple(() =>
                    {
                        Assert.That(x, Is.EqualTo(4), "X is not equal.");
                        Assert.That(y, Is.EqualTo(4), "Y is not equal.");
                    });
                }
                catch (Exception e)
                {
                    exception = e;
                }
                done = true;
            });
            while (!done) 
            { }

            if (exception != null)
                throw exception;
        }

        [Test]
        public void VerifyFirstPanelRenderAreaSize_MustBe_256_198()
        {
            var done = false;
            Exception? exception = null;
            Panel firstPanel = _window!.Layout.FirstPanel;
            firstPanel.RegisterHook(EventHook.BeforeDraw, (component, args) =>
            {
                try
                {
                    var graphics = (IGraphics)args[0];
                    var renderArea = graphics.GetRenderArea(out _, out _);
                    Assert.Multiple(() =>
                    {
                        Assert.That(renderArea.W, Is.EqualTo(256), "Width is not equal.");
                        Assert.That(renderArea.H, Is.EqualTo(198), "Height is not equal.");
                    });
                }
                catch (Exception e)
                {
                    exception = e;
                }
                done = true;
            });
            while (!done)
            { }

            if (exception != null)
                throw exception;
        }

        [Test]
        public void VerifySecondPanelPosition_MustBe_36_68()
        {
            var done = false;
            Exception? exception = null;
            Panel secondPanel = _window!.Layout.FirstPanel.SecondPanel;
            secondPanel.RegisterHook(EventHook.BeforeDraw, (component, args) =>
            {
                try
                {
                    var graphics = (IGraphics)args[0];
                    graphics.GetGlobalPosition(out var x, out var y);
                    Assert.Multiple(() =>
                    {
                        Assert.That(x, Is.EqualTo(36), "X is not equal.");
                        Assert.That(y, Is.EqualTo(68), "Y is not equal.");
                    });
                }
                catch (Exception e)
                {
                    exception = e;
                }
                done = true;
            });
            while (!done)
            { }

            if (exception != null)
                throw exception;
        }

        [Test]
        public void VerifySecondPanelRenderAreaSize_MustBe_128_134()
        {
            var done = false;
            Exception? exception = null;
            Panel secondPanel = _window!.Layout.FirstPanel.SecondPanel;
            secondPanel.RegisterHook(EventHook.BeforeDraw, (component, args) =>
            {
                try
                {
                    var graphics = (IGraphics)args[0];
                    var renderArea = graphics.GetRenderArea(out _, out _);
                    Assert.Multiple(() =>
                    {
                        Assert.That(renderArea.W, Is.EqualTo(128), "Width is not equal.");
                        Assert.That(renderArea.H, Is.EqualTo(134), "Height is not equal.");
                    });
                }
                catch (Exception e)
                {
                    exception = e;
                }
                done = true;
            });
            while (!done)
            { }

            if (exception != null)
                throw exception;
        }

        [Test]
        public void VerifyThirdPanelPosition_MustBe_44_84()
        {
            var done = false;
            Exception? exception = null;
            Panel thirdPanel = _window!.Layout.FirstPanel.SecondPanel.ThirdPanel;
            thirdPanel.RegisterHook(EventHook.BeforeDraw, (component, args) =>
            {
                try
                {
                    var graphics = (IGraphics)args[0];
                    graphics.GetGlobalPosition(out var x, out var y);
                    Assert.Multiple(() =>
                    {
                        Assert.That(x, Is.EqualTo(44), "X is not equal.");
                        Assert.That(y, Is.EqualTo(84), "Y is not equal.");
                    });
                }
                catch (Exception e)
                {
                    exception = e;
                }
                done = true;
            });
            while (!done)
            { }

            if (exception != null)
                throw exception;
        }

        [Test]
        public void VerifyThirdPanelRenderAreaSize_MustBe_64_64()
        {
            var done = false;
            Exception? exception = null;
            Panel thirdPanel = _window!.Layout.FirstPanel.SecondPanel.ThirdPanel;
            thirdPanel.RegisterHook(EventHook.BeforeDraw, (component, args) =>
            {
                try
                {
                    var graphics = (IGraphics)args[0];
                    var renderArea = graphics.GetRenderArea(out _, out _);
                    Assert.Multiple(() =>
                    {
                        Assert.That(renderArea.W, Is.EqualTo(64), "Width is not equal.");
                        Assert.That(renderArea.H, Is.EqualTo(64), "Height is not equal.");
                    });
                }
                catch (Exception e)
                {
                    exception = e;
                }
                done = true;
            });
            while (!done)
            { }

            if (exception != null)
                throw exception;
        }
    }
}

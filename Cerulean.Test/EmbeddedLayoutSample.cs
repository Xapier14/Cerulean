namespace Cerulean.Test
{
    public class EmbeddedLayoutSample : Layout
    {
        public EmbeddedLayoutSample()
        {
            AddChild("ContentPanel", new Panel
            {
                Size = new Size(400, 400),
                X = 2,
                Y = 2,
                BackColor = new Color(160, 160, 160),
                BorderColor = new Color(200, 200, 200)
            });
            DynamicLayout.ContentPanel.AddChild("HeadingLabel", new Label
            {
                Text = "Hello World!",
                ForeColor = new Color(20, 20, 20),
                X = 16,
                Y = 16
            });
        }
    }
}

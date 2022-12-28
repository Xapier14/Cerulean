using Cerulean.Common;
using Cerulean.Core;

namespace Cerulean.Components
{
    [SkipAutoRefGeneration]
    public static class MessageBoxDialog
    {
        public static Window Show(string title, string message)
        {
            var api = CeruleanAPI.GetAPI();
            dynamic layout = new Layout();
            layout.AddChild("Grid_MainGrid", new Grid
            {
                RowCount = 2,
                ColumnCount = 5
            });
            layout.Grid_MainGrid.AddChild("Label_Text", new Label
            {
                Text = message,
                ForeColor = new Color("#000"),
                GridRow = 0,
                GridColumnSpan = 5
            });
            layout.Grid_MainGrid.AddChild("Button_OK", new Button
            {
                Text = "OK",
                ForeColor = new Color("#000"),
                GridRow = 1,
                GridColumn = 4
            });
            var window = (Window)api.CreateDialogModal(layout, title);
            var scaledHeight = (uint)Scaling.GetDpiScaledValue(window, 32u);
            layout.Grid_MainGrid.SetRowHeight(1, scaledHeight);
            return window;
        }
    }
}

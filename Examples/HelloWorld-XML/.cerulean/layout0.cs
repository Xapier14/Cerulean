using Cerulean;
using Cerulean.Core;
using Cerulean.Common;
using Cerulean.Components;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
// Generated with Cerulean-API Builder
namespace Cerulean.App
{
	public partial class HelloWorldLayout : Layout
	{
		public HelloWorldLayout() : base()
		{
			AddChild("Grid", new Grid()
			{
				RowCount = 2,
				ColumnCount = 2,
				BackColor = new Color(0, 0, 0),
			});
			GetChildNullable("Grid")?.AddChild("Label", new Label()
			{
				Text = "Hello World!",
				ForeColor = new Color(255, 255, 255),
				GridColumn = 1,
				GridRow = 1,
				FontName = "Arial",
				FontSize = 24,
				X = 0,
				Y = 0,
			});
		}
	}
}
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
// Generated on: 6/25/2022 10:16:41 AM
using Cerulean;
using Cerulean.Core;
using Cerulean.Common;
using Cerulean.Components;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
// Generated with Cerulean-API Builder
namespace Cerulean.App
{
	public partial class MainLayout : Layout
	{
		public MainLayout() : base()
		{
			AddChild("MainGrid", new Grid()
			{
				RowCount = 2,
				ColumnCount = 3,
			});
			GetChildNullable("MainGrid")?.AddChild("Image1", new Image()
			{
				FileName = "Cerulean.png",
				PictureMode = PictureMode.None,
				GridRow = 0,
				GridColumn = 0,
			});
			GetChildNullable("MainGrid")?.AddChild("Label1", new Label()
			{
				Text = "None",
				FontName = "Arial",
				FontSize = 28,
				ForeColor = new Color(255, 0, 0),
				X = 8,
				Y = 8,
				GridRow = 0,
				GridColumn = 0,
			});
			GetChildNullable("MainGrid")?.AddChild("Image2", new Image()
			{
				FileName = "Cerulean.png",
				PictureMode = PictureMode.Stretch,
				GridRow = 0,
				GridColumn = 1,
			});
			GetChildNullable("MainGrid")?.AddChild("Label2", new Label()
			{
				Text = "Stretch",
				FontName = "Arial",
				FontSize = 28,
				ForeColor = new Color(255, 0, 0),
				X = 8,
				Y = 8,
				GridRow = 0,
				GridColumn = 1,
			});
			GetChildNullable("MainGrid")?.AddChild("Image3", new Image()
			{
				FileName = "Cerulean.png",
				PictureMode = PictureMode.Center,
				GridRow = 0,
				GridColumn = 2,
			});
			GetChildNullable("MainGrid")?.AddChild("Label3", new Label()
			{
				Text = "Center",
				FontName = "Arial",
				FontSize = 28,
				ForeColor = new Color(255, 0, 0),
				X = 8,
				Y = 8,
				GridRow = 0,
				GridColumn = 2,
			});
			GetChildNullable("MainGrid")?.AddChild("Image4", new Image()
			{
				FileName = "Cerulean.png",
				PictureMode = PictureMode.Tile,
				GridRow = 1,
				GridColumn = 0,
			});
			GetChildNullable("MainGrid")?.AddChild("Label4", new Label()
			{
				Text = "Tile",
				FontName = "Arial",
				FontSize = 28,
				ForeColor = new Color(255, 0, 0),
				X = 8,
				Y = 8,
				GridRow = 1,
				GridColumn = 0,
			});
			GetChildNullable("MainGrid")?.AddChild("Image5", new Image()
			{
				FileName = "Cerulean.png",
				PictureMode = PictureMode.Fit,
				GridRow = 1,
				GridColumn = 1,
			});
			GetChildNullable("MainGrid")?.AddChild("Label5", new Label()
			{
				Text = "Fit",
				FontName = "Arial",
				FontSize = 28,
				ForeColor = new Color(255, 0, 0),
				X = 8,
				Y = 8,
				GridRow = 1,
				GridColumn = 1,
			});
			GetChildNullable("MainGrid")?.AddChild("Image6", new Image()
			{
				FileName = "Cerulean.png",
				PictureMode = PictureMode.Cover,
				GridRow = 1,
				GridColumn = 2,
			});
			GetChildNullable("MainGrid")?.AddChild("Label6", new Label()
			{
				Text = "Cover",
				FontName = "Arial",
				FontSize = 28,
				ForeColor = new Color(255, 0, 0),
				X = 8,
				Y = 8,
				GridRow = 1,
				GridColumn = 2,
			});
		}
	}
}
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
// Generated on: 6/27/2022 9:43:09 AM
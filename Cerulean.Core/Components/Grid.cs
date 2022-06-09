using Cerulean.Common;

namespace Cerulean.Core
{
    public class Grid : Component
    {
        public int ColumnCount { get; set; } = 1;
        public int RowCount { get; set; } = 1;
        public override void Update(Size clientArea)
        {
            // calculate client area for each cell
            // set the sdl viewport to that cell and abstract away the needed position and size data to the component
            Size[,] cellSizes = new Size[RowCount, ColumnCount];
        }
    }
}

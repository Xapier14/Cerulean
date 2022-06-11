using Cerulean.Common;

namespace Cerulean.Core
{
    public sealed class Grid : Component
    {
        private int[] _columns = { 0 };
        private int[] _rows = { 0 };
        private Size[,]? _cellSizes;
        public int ColumnCount {
            get => _columns.Length;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _columns = new int[value];
                for (int i = 0; i < value; i++)
                    _columns[i] = -1;
            }
        }
        public int RowCount
        {
            get => _rows.Length;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _rows = new int[value];
                for (int i = 0; i < value; i++)
                    _rows[i] = -1;
            }
        }
        public override void Update(object? window, Size clientArea)
        {
            // anchor to top left
            X = 0;
            Y = 0;

            // use full window area
            ClientArea = clientArea;

            // calculate client area for each cell
            // set the sdl viewport to that cell and abstract away the needed position and size data to the component
            _cellSizes = new Size[RowCount, ColumnCount];

            // get auto-sized cell sizes
            int fixedRow = 0;
            int fixedColumn = 0;
            int autoRow = 0;
            int autoColumn = 0;

            // for each row in _rows, add to fixedRow if not -1 (auto-sized) & add 1 to autoRow
            _rows
                .ToList()
                .ForEach(x =>
                {
                    fixedRow += x == 0 ? 0 : x;
                    autoRow += x == 0 ? 1 : 0;
                });

            // for each column in _columns, add to fixedColumn if not -1 (auto-sized) & add 1 to autoColumn
            _columns
                .ToList()
                .ForEach(x =>
                {
                    fixedColumn += x == 0 ? 0 : x;
                    autoColumn += x == 0 ? 1 : 0;
                });

            // calculate lows & highs for width and height of auto cells
            int lowHeight = (int)Math.Floor((clientArea.H - fixedRow) / (double)autoRow);
            int highHeight = (int)Math.Ceiling((clientArea.H - fixedRow) / (double)autoRow);
            int lowWidth = (int)Math.Floor((clientArea.W - fixedColumn) / (double)autoColumn);
            int highWidth = (int)Math.Ceiling((clientArea.W - fixedColumn) / (double)autoColumn);

            // for each cell...
            int autoRowsComputed = 0;
            int autoColumnsComputed = 0;
            for (int row = 0; row < RowCount; row++)
            {
                for (int col = 0; col < ColumnCount; col++)
                {
                    int height = _rows[row];
                    int width = _columns[col];
                    if (height == 0)
                    {
                        height = autoRowsComputed < autoRow - 2 ? 
                            lowHeight :
                            highHeight;
                        autoRowsComputed++;
                    }

                    if (width == 0)
                    {
                        width = autoColumnsComputed < autoColumn - 2 ?
                            lowWidth :
                            highWidth;
                        autoColumnsComputed++;
                    }
                    _cellSizes[row, col].W = width;
                    _cellSizes[row, col].H = height;
                }
            }

            // update child components
            foreach (var child in Children)
            {
                // compute element clientArea
                var cell = _cellSizes[child.GridRow, child.GridColumn];
                int width = cell.W;
                int height = cell.H;

                // add additional space via span
                // if i < span OR i 0
                // AND i < row count
                for (int i = 1;
                    (i < child.GridRowSpan || child.GridRowSpan == 0) && i < RowCount;
                    i++)
                {
                    height += _cellSizes[child.GridRow + i, child.GridColumn].H;
                }
                // if i < span OR i 0
                // AND i < column count
                for (int i = 1;
                    (i < child.GridColumnSpan || child.GridColumnSpan == 0) && i < ColumnCount;
                    i++)
                {
                    width += _cellSizes[child.GridRow + i, child.GridColumn].W;
                }
                child.Update(window, new(width, height));
            }
        }
    }
}

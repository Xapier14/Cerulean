﻿namespace Cerulean.Common
{
    public sealed class Grid : Component
    {
        private int[] _columns = { 0 };
        private int[] _rows = { 0 };
        private Size[,]? _cellSizes;
        public int ColumnCount
        {
            get => _columns.Length;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _columns = new int[value];
                for (int i = 0; i < value; i++)
                    _columns[i] = 0;
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
                    _rows[i] = 0;
            }
        }
        public Color? BackColor { get; set; }
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
            int bigHeight = (int)Math.Ceiling((clientArea.H - fixedRow) / (double)autoRow);
            int inverseSmallHeight = 0;
            int bigWidth = (int)Math.Ceiling((clientArea.W - fixedColumn) / (double)autoColumn);
            int inverseSmallWidth = 0;

            // for each cell...
            int autoColumnsComputed = 0;
            int autoRowsComputed = 0;
            for (int row = 0; row < RowCount; row++)
            {
                int height = _rows[row];
                if (height == 0)
                {
                    height = autoRowsComputed < autoRow - 1 ?
                        bigHeight :
                        clientArea.H - fixedRow - inverseSmallHeight;
                    autoRowsComputed++;
                    inverseSmallHeight += height;
                }
                for (int col = 0; col < ColumnCount; col++)
                {
                    int width = _columns[col];
                    if (width == 0)
                    {
                        width = autoColumnsComputed < autoColumn - 1 ?
                            bigWidth :
                            clientArea.W - fixedColumn - inverseSmallWidth;
                        autoColumnsComputed++;
                        inverseSmallWidth += width;
                    }
                    _cellSizes[row, col].W = width;
                    _cellSizes[row, col].H = height;
                }
                autoColumnsComputed = 0;
                inverseSmallWidth = 0;
            }
            autoRowsComputed = 0;
            inverseSmallHeight = 0;

            // update child components
            foreach (var child in Children)
            {
                // compute element clientArea
                var cell = _cellSizes[child.GridRow, child.GridColumn];
                int width = 0;
                int height = 0;

                // add additional space via span
                // if i < span OR i 0
                // AND i < row count
                for (int i = 0;
                    (i < child.GridRowSpan || child.GridRowSpan == 0) && child.GridRow + i < RowCount;
                    i++)
                {
                    height += _cellSizes[child.GridRow + i, child.GridColumn].H;
                }
                // if i < span OR i 0
                // AND i < column count
                for (int i = 0;
                    (i < child.GridColumnSpan || child.GridColumnSpan == 0) && child.GridColumn + i < ColumnCount;
                    i++)
                {
                    width += _cellSizes[child.GridRow, child.GridColumn + i].W;
                }
                child.Update(window, new(width, height));
            }
        }

        public override void Draw(IGraphics graphics)
        {
            if (ClientArea is Size fullArea)
            {
                // Draw fill
                if (BackColor is Color backColor)
                {
                    graphics.DrawFilledRectangle(0, 0, fullArea, backColor);
                }
            }
            var area = graphics.GetRenderArea(out int areaX, out int areaY);
            foreach (var child in Children)
            {
                int childX = areaX + child.X;
                int childY = areaY + child.Y;

                for (int column = 0; column < child.GridColumn && column < ColumnCount; ++column)
                    if (_cellSizes is not null)
                        childX += _cellSizes[0, column].W;

                for (int row = 0; row < child.GridRow && row < RowCount; ++row)
                    if (_cellSizes is not null)
                        childY += _cellSizes[row, 0].H;

                if (child.ClientArea is Size clientArea)
                    graphics.SetRenderArea(clientArea, childX, childY);
                child.Draw(graphics);
            }
            graphics.SetRenderArea(area, areaX, areaY);
        }
    }
}
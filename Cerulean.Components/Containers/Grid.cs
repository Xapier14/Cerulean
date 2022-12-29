using Cerulean.Common;

namespace Cerulean.Components
{
    public class Grid : Component
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
                for (var i = 0; i < value; i++)
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
                for (var i = 0; i < value; i++)
                    _rows[i] = 0;
            }
        }
        public Color? BackColor { get; set; }

        private void UpdateChildComponents(IWindow window)
        {
            if (_cellSizes is null)
                return;

            foreach (var child in Children)
            {
                // compute element clientArea
                var width = 0;
                var height = 0;

                // add additional space via span
                // if i < span OR i 0
                // AND i < row count
                for (var i = 0;
                     (i < child.GridRowSpan || child.GridRowSpan == 0) && child.GridRow + i < RowCount;
                     i++)
                {
                    height += Math.Max(0, _cellSizes[child.GridRow + i, child.GridColumn].H);
                }
                // if i < span OR i 0
                // AND i < column count
                for (var i = 0;
                     (i < child.GridColumnSpan || child.GridColumnSpan == 0) && child.GridColumn + i < ColumnCount;
                     i++)
                {
                    width += Math.Max(0, _cellSizes[child.GridRow, child.GridColumn + i].W);
                }
                var childArea = new Size(width, height);
                CallHook(child, EventHook.BeforeChildUpdate, window, childArea);
                child.Update(window, childArea);
                CallHook(child, EventHook.AfterChildUpdate, window, childArea);
            }
        }

        private void CalculateCellSizes(Size clientArea)
        {
            _cellSizes = new Size[RowCount, ColumnCount];

            // get auto-sized cell sizes
            var fixedRow = 0;
            var fixedColumn = 0;
            var autoRow = 0;
            var autoColumn = 0;

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

            var scaledColumns = new int[_columns.Length];
            Array.Copy(_columns, scaledColumns, _columns.Length);
            var scaledRows = new int[_rows.Length];
            Array.Copy(_rows, scaledRows, _rows.Length);

            // double check row and column sizes
            if (fixedColumn > clientArea.W)
            {
                // scale down fixed columns
                for (var i = 0; i < scaledColumns.Length; ++i)
                {
                    scaledColumns[i] = (int)(((double)_columns[i] / fixedColumn) * clientArea.W);
                }
            }
            if (fixedRow > clientArea.H)
            {
                // scale down fixed rows
                for (var i = 0; i < scaledRows.Length; ++i)
                {
                    scaledRows[i] = (int)(((double)_rows[i] / fixedRow) * clientArea.H);
                }
            }

            // calculate lows & highs for width and height of auto cells
            var bigHeight = (int)Math.Ceiling((clientArea.H - fixedRow) / (double)autoRow);
            var inverseSmallHeight = 0;
            var bigWidth = (int)Math.Ceiling((clientArea.W - fixedColumn) / (double)autoColumn);
            var inverseSmallWidth = 0;

            // for each cell...
            var autoColumnsComputed = 0;
            var autoRowsComputed = 0;
            for (var row = 0; row < RowCount; row++)
            {
                var height = scaledRows[row];
                if (height == 0)
                {
                    height = autoRowsComputed < autoRow - 1 ?
                        bigHeight :
                        clientArea.H - fixedRow - inverseSmallHeight;
                    autoRowsComputed++;
                    inverseSmallHeight += height;
                }
                for (var col = 0; col < ColumnCount; col++)
                {
                    var width = scaledColumns[col];
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
        }

        public override void Update(IWindow window, Size clientArea)
        {
            CallHook(this, EventHook.BeforeUpdate, window, clientArea);

            // anchor to top left
            X = 0;
            Y = 0;

            // use full window area
            ClientArea = clientArea;

            // calculate client area for each cell
            CalculateCellSizes(clientArea);
            
            UpdateChildComponents(window);

            if (Modified)
            {
                Modified = false;
                window.FlagForRedraw();
            }
            
            CallHook(this, EventHook.AfterUpdate, window, clientArea);
        }

        public override void Draw(IGraphics graphics, int viewportX, int viewportY, Size viewportSize)
        {
            if (!ClientArea.HasValue)
                return;
            if (_cellSizes is null)
                return;
            
            CacheViewportData(viewportX, viewportY, viewportSize);

            CallHook(this, EventHook.BeforeDraw, graphics, viewportX, viewportY, viewportSize);

            // Draw fill
            if (BackColor.HasValue)
            {
                graphics.DrawFilledRectangle(0, 0, ClientArea.Value, BackColor.Value);
            }

            graphics.GetGlobalPosition(out var oldX, out var oldY);
            foreach (var component in Children)
            {
                Size childViewport = new();
                var childViewportX = viewportX;
                var childViewportY = viewportY;

                // get viewport x offset from previous cells
                for (var column = 0; column < component.GridColumn && column < ColumnCount; ++column)
                {
                    if (_cellSizes is null)
                        continue;
                    var add = _cellSizes[0, column].W;
                    childViewportX += add > 0 ? add : 0;
                }

                // get viewport y offset from previous cells
                for (var row = 0; row < component.GridRow && row < RowCount; ++row)
                {
                    if (_cellSizes is null)
                        continue;
                    var add = _cellSizes[row, 0].H;
                    childViewportY += add > 0 ? add : 0;
                }

                // get viewport w with respect to column span
                for (var columnIndex = component.GridColumn;
                     columnIndex < component.GridColumn + component.GridColumnSpan && component.GridColumn < ColumnCount;
                     ++columnIndex)
                    childViewport.W += Math.Max(0, _cellSizes![0, columnIndex].W);

                // get viewport h with respect to row span
                for (var rowIndex = component.GridRow;
                     rowIndex < component.GridRow + component.GridRowSpan && component.GridRow < RowCount;
                     ++rowIndex)
                    childViewport.H += Math.Max(0, _cellSizes![rowIndex, 0].H);

                /* Viewport Clipping */
                var aX = childViewportX + Math.Max(0, component.X);
                var aY = childViewportY + Math.Max(0, component.Y);
                var offsetX = aX;
                var offsetY = aY;
                var childSize = new Size(component.ClientArea!.Value);
                
                // check left is clipping
                if (component.X + oldX < 0)
                {
                    childSize.W += component.X;
                    offsetX = oldX + component.X;
                }
                // check right is clipping
                if (Math.Max(0, component.X) + childSize.W > Math.Min(childViewport.W, viewportSize.W))
                {
                    childSize.W -= (Math.Max(0, component.X) + childSize.W) - Math.Min(childViewport.W, viewportSize.W);
                }

                // check top is clipping
                if (component.Y + oldY < 0)
                {
                    childSize.H += component.Y;
                    offsetY = oldY + component.Y;
                }
                // check bottom is clipping
                if (Math.Max(0, component.Y) + childSize.H > Math.Min(childViewport.H, viewportSize.H))
                {
                    childSize.H -= (Math.Max(0, component.Y) + childSize.H) - Math.Min(childViewport.H, viewportSize.H);
                }

                // skip draw if component has invalid area.
                if (childSize.W < 0 || childSize.H < 0)
                    continue;

                graphics.SetRenderArea(childSize, aX, aY);
                graphics.SetGlobalPosition(offsetX, offsetY);

                CallHook(component, EventHook.BeforeChildDraw, graphics, aX, aY, childSize);
                component.Draw(graphics, aX, aY, childSize);
                CallHook(component, EventHook.AfterChildDraw, graphics, aX, aY, childSize);
            }

            CallHook(this, EventHook.AfterDraw, graphics, viewportX, viewportY, viewportSize);

            graphics.SetRenderArea(viewportSize, viewportX, viewportY);
            graphics.SetGlobalPosition(oldX, oldY);
        }

        public void SetRowHeight(int index, uint height)
        {
            if (index < 0 || index >= RowCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            _rows[index] = (int)height;
        }

        public void SetColumnWidth(int index, uint width)
        {
            if (index < 0 || index >= ColumnCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            _columns[index] = (int)width;
        }
    }
}
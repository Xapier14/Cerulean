using System.ComponentModel.DataAnnotations;
using System.Dynamic;

namespace Cerulean.Common
{
    public abstract class Component : DynamicObject
    {
        private readonly Dictionary<string, Component> _components = new();
        protected bool CanBeChild { get; init; } = true;
        protected bool CanBeParent { get; init; } = true;
        public Component? Parent { get; set; }
        public IEnumerable<Component> Children => _components.Values;
        public IDictionary<string, object> Attributes { get; init; } = new Dictionary<string, object>();
        public virtual int GridRow { get; set; } = 0;
        public virtual int GridColumn { get; set; } = 0;
        public virtual int GridRowSpan { get; set; } = 1;
        public virtual int GridColumnSpan { get; set; } = 1;
        public virtual int X { get; set; } = 0;
        public virtual int Y { get; set; } = 0;
        public Size? ClientArea { get; protected set; } = null;

        protected void AddOrUpdateAttribute(string attribute, object value)
        {
            Attributes[attribute] = value;
        }

        protected void ModifyClientArea(int byW, int byH)
        {
            if (ClientArea is not null)
            {
                ClientArea = new Size(ClientArea.Value.W + byW, ClientArea.Value.H + byH);
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            var tryResult = _components.TryGetValue(binder.Name, out var component);
            result = component;
            return tryResult;
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            return false;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
        {
            result = null;
            switch (indexes[0])
            {
                case int intIndex and >= 0 when intIndex < Attributes.Count:
                    var pair = Attributes.ElementAt(intIndex);
                    result = (pair.Key, pair.Value);
                    return true;
                case string stringIndex:
                    var success = Attributes.TryGetValue(stringIndex, out var attributeValue);
                    result = attributeValue;
                    return success;
            }
            return false;
        }

        public dynamic AddChild(string name, Component component)
        {
            if (!CanBeParent)
                throw new GeneralAPIException("This component cannot be a parent of another component.");
            if (_components.ContainsKey(name))
                throw new GeneralAPIException("A child component with the same name already exists.");
            if (!component.CanBeChild)
                throw new GeneralAPIException("Component cannot be a child of another component.");
            _components[name] = component;
            component.Parent = this;
            return component;
        }

        public object? GetAttribute(string attribute)
        {
            return Attributes.TryGetValue(attribute, out var result) ? result : null;
        }

        public dynamic GetChild(string name)
        {
            var scopedNames = name.Split('.');
            var element = this;
            foreach (var scopedName in scopedNames)
            {
                if (element._components.TryGetValue(scopedName, out var value))
                    element = value;
            }
            if (element != this)
                return element;
            throw new GeneralAPIException($"Child \"{name}\" not found.");
        }

        public T GetChild<T>(string name)
        {
            return GetChild(name);
        }
        public dynamic? GetChildNullable(string name)
        {
            return _components.TryGetValue(name, out var value) ? value : null;
        }

        public virtual void Init()
        {

        }

        public virtual void Update(object? window, Size clientArea)
        {
            // if component does not use client area
            if (ClientArea is null)
            {
                // set the origin position to top-left of window.
                X = 0;
                Y = 0;
            }

            if (!CanBeParent) return;
            foreach (var child in Children)
                child.Update(window, clientArea);
        }

        public virtual void Draw(IGraphics graphics, int viewportX, int viewportY, Size viewportSize)
        {
            // skip if component is functional, aka: no draw func
            if (!ClientArea.HasValue) return;

            // check if viewport is at least visible
            if (viewportX + viewportSize.W <= 0 && viewportY + viewportSize.H <= 0)
                return;

            // for all child components that has a non-null client area
            var children = Children.Where(x => x.ClientArea.HasValue);
            var components = children as Component[] ?? children.ToArray();
            if (!components.Any()) return;
            foreach (var component in components)
            {
                /*
                 * Algorithm:
                 *  *NOTE: huehuehue
                 *
                 *  WHERE:
                 *      VP          = viewport
                 *      C           = child component (X & Y are new viewport position)
                 *      CA          = child component client area
                 *      childSize   = child component's new viewport
                 *      offset      = sent to graphics backend
                 *
                 * -> (VP.X + Max(0, C.X), VP.Y + Max(0, C.Y)) = (A.X, A.Y)
                 * -> childSize = CA.W x CA.H
                 * -> offset = -C.X, -C.Y
                 *
                 * [WIDTH CHECKS]
                 * [CHECK LEFT]
                 * if (C.X < 0):
                 *      offset.X = C.X
                 *      C.W += C.X
                 * [CHECK RIGHT]
                 * if (Max(0, C.X) + C.W > VP.W):
                 *      C.W -= (Max(0, C.X) + C.W) - VP.W
                 *
                 * [HEIGHT CHECKS]
                 * [CHECK TOP]
                 * if (C.Y < 0):
                 *      offset.Y = C.Y
                 *      C.H += C.Y
                 * if (Max(0, C.Y) + C.H > VP.H):
                 *      C.H -= (Max(0, C.Y) + C.H) - VP.H
                 *
                 * [SET VIEWPORT]
                 * setRenderArea(A.X, A.Y, childSize, offset.X, offset.Y)
                 * [DRAW AS CONTAINER]
                 * child.draw(A.X, A.Y, childSize)
                 */

                var aX = viewportX + Math.Max(0, component.X);
                var aY = viewportY + Math.Max(0, component.Y);
                var childSize = new Size(component.ClientArea!.Value);

                /* WIDTH CHECKS */
                // check left
                if (component.X < 0)
                {
                    Console.WriteLine("Left clipping...");
                    childSize.W += component.X;
                }
                // check right
                if (Math.Max(0, component.X) + childSize.W > viewportSize.W)
                {
                    Console.WriteLine("Right clipping...");
                    childSize.W -= (Math.Max(0, component.X) + childSize.W) - viewportSize.W;
                }
                /* HEIGHT CHECKS */
                // check top
                if (component.Y < 0)
                {
                    Console.WriteLine("Top clipping...");
                    childSize.H += component.Y;
                }
                // check bottom
                if (Math.Max(0, component.Y) + childSize.H > viewportSize.H)
                {
                    Console.WriteLine("Bottom clipping...");
                    childSize.H -= (Math.Max(0, component.Y) + childSize.H) - viewportSize.H;
                }

                if (childSize.W < 0 || childSize.H < 0)
                    continue;

                graphics.SetRenderArea(childSize, aX, aY);
                graphics.SetGlobalPosition(aX - component.X, aY - component.Y);
                component.Draw(graphics, aX, aY, childSize);


                //// use the child component's size as viewport accounting for negative position
                ////var adjustedViewport = new Size(component.X + viewportSize.W, component.Y + viewportSize.H);
                //var adjustedViewport = new Size(component.ClientArea!.Value.W, component.ClientArea!.Value.H);
                //var childX = viewportX + Math.Max(0, component.X);
                //var childY = viewportY + Math.Max(0, component.Y);

                //// if the component's absolute area is out of bounds.
                //if (adjustedViewport.W > viewportSize.W - X)
                //    adjustedViewport.W = viewportSize.W - X;
                //if (adjustedViewport.H > viewportSize.H - Y)
                //    adjustedViewport.H = viewportSize.H - Y;

                //if (childX + component.X < 0)
                //{
                //    adjustedViewport.W += childX;
                //    childX = viewportX;
                //}
                //if (childY + component.Y < 0)
                //{
                //    adjustedViewport.H += childY;
                //    childY = viewportY;
                //}

                //// new adjustedViewport: 20x15
                //graphics.SetRenderArea(adjustedViewport, childX + X, childY + Y);
                //component.Draw(graphics, childX + X, childY + Y, adjustedViewport);
            }

            // restore render area
            graphics.SetRenderArea(viewportSize, viewportX, viewportY);
        }
    }
}
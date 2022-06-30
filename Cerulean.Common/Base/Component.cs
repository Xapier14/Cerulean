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
            if (!ClientArea.HasValue) return;

            // for all child components that has a non-null client area
            var children = Children.Where(x => x.ClientArea.HasValue);
            if (!children.Any()) return;
            foreach (var component in children)
            {
                // use the child component's size as viewport accounting for negative position
                var adjustedViewport = new Size(component.ClientArea!.Value.W + Math.Max(component.X, 0), component.ClientArea!.Value.H + Math.Max(component.Y, 0));
                // adjustedViewport = 17x12

                // if the component's absolute area is out of bounds.
                if (adjustedViewport.W > viewportSize.W - X)
                    adjustedViewport.W = viewportSize.W - X;
                if (adjustedViewport.H > viewportSize.H - Y)
                    adjustedViewport.H = viewportSize.H - Y;
                // new adjustedViewport: 20x15
                graphics.SetRenderArea(adjustedViewport, viewportX + X, viewportY + Y);
                component.Draw(graphics, viewportX + X, viewportY + Y, adjustedViewport);
            }

            // restore render area
            graphics.SetRenderArea(viewportSize, viewportX, viewportY);
        }
    }
}
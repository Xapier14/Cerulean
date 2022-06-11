using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Cerulean.Common
{
    public abstract class Component : DynamicObject
    {
        private Dictionary<string, Component> _components = new();
        protected bool CanBeChild { get; init; } = true;
        protected bool CanBeParent { get; init; } = true;
        public Component? Parent { get; set; }
        public IEnumerable<Component> Children { get => _components.Values; }
        public IDictionary<string, string> Attributes { get; init; } = new Dictionary<string, string>();
        public int GridRow { get; set; } = 0;
        public int GridColumn { get; set; } = 0;
        public int GridRowSpan { get; set; } = 0;
        public int GridColumnSpan { get; set; } = 0;
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public Size? ClientArea { get; protected set; } = null;

        protected void AddOrUpdateAttribute(string attribute, string value)
        {
            Attributes[attribute] = value;
        }

        public int IChildCount
        {
            get
            {
                return _components.Count;
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            bool tryResult = _components.TryGetValue(binder.Name, out Component? component);
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
                case int intIndex when intIndex >= 0 && intIndex < Attributes.Count:
                    var pair = Attributes.ElementAt(intIndex);
                    result = (pair.Key, pair.Value);
                    return true;
                case string stringIndex:
                    var success = Attributes.TryGetValue(stringIndex, out string? attributeValue);
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

        public string? GetAttribute(string attribute)
        {
            if (Attributes.TryGetValue(attribute, out string? result))
                return result;
            return null;
        }

        public dynamic GetChild(string name)
        {
            if (_components.TryGetValue(name, out Component? value))
                return value;
            throw new GeneralAPIException("Child not found.");
        }
        public dynamic? GetChildNullable(string name)
        {
            if (_components.TryGetValue(name, out Component? value))
                return value;
            return null;
        }

        public virtual void Init()
        {

        }

        public virtual void Update(Size clientArea)
        {
            // if component does not use client area
            if (ClientArea is null)
            {
                // set the origin position to top-left of window.
                X = 0;
                Y = 0;
            }
            if (CanBeParent)
                foreach (var child in Children)
                    child.Update(clientArea);
        }

        public virtual void Draw(IGraphics graphics, int x, int y)
        {
            // remember old render area
            Size renderArea = graphics.GetRenderArea(
                out int areaX,
                out int areaY);

            // set component position relative to parent;
            int componentX = x + X;
            int componentY = y + Y;

            // set render area to component clientArea
            if (ClientArea is Size clientArea)
                graphics.SetRenderArea(
                    clientArea,
                    componentX,
                    componentY);

            // draw child elements
            if (CanBeParent)
                foreach (var child in Children)
                    child.Draw(
                        graphics,
                        componentX,
                        componentY);

            // restore old render area
            graphics.SetRenderArea(renderArea, areaX, areaY);
        }
    }
}
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
                    var success = Attributes.TryGetValue(stringIndex, out object? attributeValue);
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
            if (Attributes.TryGetValue(attribute, out object? result))
                return result;
            return null;
        }

        public dynamic GetChild(string name)
        {
            string[] scopedNames = name.Split('.');
            Component element = this;
            foreach (string scopedName in scopedNames)
            {
                if (element._components.TryGetValue(scopedName, out Component? value))
                    element = value;
            }
            if (element != this)
                return element;
            throw new GeneralAPIException($"Child \"{name}\" not found.");
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

        public virtual void Update(object? window, Size clientArea)
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
                    child.Update(window, clientArea);
        }

        public virtual void Draw(IGraphics graphics)
        {
            // remember old render area
            Size renderArea = graphics.GetRenderArea(
                out int areaX,
                out int areaY);

            // set render area to component clientArea
            if (ClientArea is Size clientArea)
                graphics.SetRenderArea(
                    clientArea,
                    areaX + X,
                    areaY + Y);

            // draw child elements
            if (CanBeParent)
                foreach (var child in Children)
                    child.Draw(
                        graphics);

            // restore old render area
            graphics.SetRenderArea(renderArea, areaX, areaY);
        }
    }
}
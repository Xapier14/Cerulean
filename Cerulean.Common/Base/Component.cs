using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Cerulean.Common
{
    /// <summary>
    /// The base class for all Cerulean UI components.
    /// </summary>
    public abstract class Component : DynamicObject
    {
        private readonly List<(EventHook, Action<Component, object[]>)> _eventHooks = new();
        private readonly Dictionary<string, Component> _components = new();
        private object? _parentWindow = null;
        protected Size? CachedViewportSize;
        protected int? CachedViewportX, CachedViewportY;
        protected bool CanBeChild { get; init; } = true;
        protected bool CanBeParent { get; init; } = true;
        protected bool DisableTopLevelHooks { get; init; } = false;
        public Component? Parent { get; set; }
        public IEnumerable<Component> Children => _components.Values;
        public IDictionary<string, object> Attributes { get; init; } = new Dictionary<string, object>();
        public virtual int GridRow { get; set; }
        public virtual int GridColumn { get; set; }
        public virtual int GridRowSpan { get; set; } = 1;
        public virtual int GridColumnSpan { get; set; } = 1;
        public virtual int X { get; set; }
        public virtual int Y { get; set; }
        public virtual bool IsHoverableComponent { get; init; } = false;
        public Size? ClientArea { get; protected set; }
        public virtual bool Modified { get; protected set; }

        protected string SeedId { get; } = Guid.NewGuid().ToString();

        public object? ParentWindow
        {
            get => _parentWindow ?? Parent?.ParentWindow;
            set => _parentWindow = value;
        }

        public object this[string attribute]
        {
            get
            {
                var value = Attributes[attribute];
                CallHook(this, EventHook.GetAttribute, attribute, value);
                return value;
            }
            set
            {
                CallHook(this, EventHook.SetAttribute, attribute, value);
                Attributes[attribute] = value;
            }
        }

        protected void CallHook(Component caller, EventHook eventType, params object[] arguments)
        {
            _ = _eventHooks.All(x =>
            {
                if (x.Item1 == eventType)
                    x.Item2(caller, arguments);
                return true;
            });
        }

        protected void CacheViewportData(int x, int y, Size size)
        {
            CachedViewportSize = size;
            CachedViewportX = x;
            CachedViewportY = y;
        }

        protected void AddOrUpdateAttribute(string attribute, object value)
        {
            CallHook(this, EventHook.SetAttribute, attribute, value);
            Attributes[attribute] = value;
        }

        public void RegisterHook(EventHook eventType, Action<Component, object[]> hookCallback)
        {
            _eventHooks.Add(new ValueTuple<EventHook, Action<Component, object[]>>(eventType, hookCallback));
        }

        public void RemoveHook(EventHook eventType, Action<Component, object[]> hookCallback)
        {
            _eventHooks.RemoveAll(x => x.Item1 == eventType && x.Item2 == hookCallback);
        }

        public void RemoveHooks()
        {
            _eventHooks.Clear();
        }

        public void RemoveHooks(EventHook eventType)
        {
            _eventHooks.RemoveAll(x => x.Item1 == eventType);
        }
        
        #region Dynamic

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            var tryResult = _components.TryGetValue(binder.Name, out var component);
            result = component;
            if (result is not null)
                CallHook(this, EventHook.GetChild, binder.Name, result);
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
                    CallHook(this, EventHook.GetAttribute, indexes[0], result);
                    return true;
                case string stringIndex:
                    var success = Attributes.TryGetValue(stringIndex, out var attributeValue);
                    result = attributeValue;
                    if (result is not null)
                        CallHook(this, EventHook.GetAttribute, indexes[0], result);
                    return success;
            }
            return false;
        }

        #endregion

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
            component.Init();
            Modified = true;
            CallHook(this, EventHook.AddChild, name, component);
            return component;
        }

        public object? GetAttribute(string attribute)
        {
            var value = Attributes.TryGetValue(attribute, out var result) ? result : null;
            if (value is not null)
                CallHook(this, EventHook.GetAttribute, attribute, value);
            return value;
        }

        /// <summary>
        /// Retrieves a child component.
        /// </summary>
        /// <param name="name">The name of the child component.</param>
        /// <returns>The child component as a dynamic var.</returns>
        public dynamic GetChild(string name)
        {
            var scopedNames = name.Split('.');
            var element = this;
            foreach (var scopedName in scopedNames)
            {
                if (element._components.TryGetValue(scopedName, out var value))
                    element = value;
            }

            if (element == this)
                throw new GeneralAPIException($"Child \"{name}\" not found.");
            CallHook(this, EventHook.GetChild, name, element);
            return element;
        }

        /// <summary>
        /// Retrieves a child component.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="name">The name of the child component.</param>
        /// <returns>The type-cast child component.</returns>
        public T GetChild<T>(string name)
        {
            var child = GetChild(name);
            if (child is not T)
                throw new GeneralAPIException("Invalid component type.");

            CallHook(this, EventHook.GetChild, name, child);
            return child;
        }

        /// <summary>
        /// Retrieves a child component.
        /// </summary>
        /// <param name="name">The name of the child component.</param>
        /// <returns>The child component as a dynamic? var.</returns>
        public dynamic? GetChildNullable(string name)
        {
            var child = _components.TryGetValue(name, out var value) ? value : null;
            if (child is not null)
                CallHook(this, EventHook.GetChild, name, child);
            return child;
        }

        public virtual void Init()
        {
            if (!DisableTopLevelHooks)
                CallHook(this, EventHook.BeforeInit);
            if (!DisableTopLevelHooks)
                CallHook(this, EventHook.AfterInit);
        }

        /// <summary>
        /// The component's update step.
        /// </summary>
        /// <param name="window">The window that performed the update.</param>
        /// <param name="clientArea">The client area given to the component.</param>
        public virtual void Update(IWindow window, Size clientArea)
        {
            // if component does not use client area
            if (ClientArea is null)
            {
                // set the origin position to top-left of window.
                X = 0;
                Y = 0;
            }
            if (!DisableTopLevelHooks)
                CallHook(this, EventHook.BeforeUpdate, window, clientArea);

            if (!CanBeParent)
            {
                foreach (var child in Children)
                {
                    var childArea = new Size(clientArea.W - child.X, clientArea.H - child.Y);
                    CallHook(child, EventHook.BeforeChildUpdate, window, childArea);
                    child.Update(window, childArea);
                    CallHook(child, EventHook.AfterChildUpdate, window, childArea);
                }
            }

            if (!DisableTopLevelHooks)
                CallHook(this, EventHook.AfterUpdate, window, clientArea);
        }

        /// <summary>
        /// Check if the coordinate is within the drawn area of this component or its other child components.
        /// </summary>
        /// <param name="x">The X coordinate to check.</param>
        /// <param name="y">The Y coordinate to check.</param>
        /// <returns>The top-most component of the component result set.</returns>
        public virtual Component? CheckHoveredComponent(int x, int y)
        {
            if (CachedViewportSize is not { } viewport ||
                CachedViewportX is not { } viewportX ||
                CachedViewportY is not { } viewportY)
                return null;
            if (x < viewportX || x > viewportX + viewport.W ||
                y < viewportY || y > viewportY + viewport.H)
                return null;
            var hoveredComponent = IsHoverableComponent ? this : null;

            return Children.Aggregate(hoveredComponent,
                (current, child) => child.CheckHoveredComponent(x, y) ?? current);
        }

        /// <summary>
        /// The component's draw step.
        /// Base behavior is a simple rectangle container.
        /// </summary>
        /// <param name="graphics">The window's graphics module.</param>
        /// <param name="viewportX">The assigned X viewport coordinate.</param>
        /// <param name="viewportY">The assigned Y viewport coordinate.</param>
        /// <param name="viewportSize">The assigned viewport size.</param>
        public virtual void Draw(IGraphics graphics, int viewportX, int viewportY, Size viewportSize)
        {
            // skip if component is functional, aka: no draw func
            if (!ClientArea.HasValue) return;

            // cache viewport data to be used by CheckHoveredComponent()
            CacheViewportData(viewportX, viewportY, viewportSize);

            // check if viewport is at least visible
            if (viewportX + viewportSize.W <= 0 && viewportY + viewportSize.H <= 0)
                return;
            if (!DisableTopLevelHooks)
                CallHook(this, EventHook.BeforeDraw, graphics, viewportX, viewportY, viewportSize);

            graphics.GetGlobalPosition(out var oldX, out var oldY);

            // for all child components that has a non-null client area
            var children = Children.Where(x => x.ClientArea.HasValue);
            var components = children as Component[] ?? children.ToArray();
            foreach (var component in components)
            {
                var aX = viewportX + Math.Max(0, component.X);
                var aY = viewportY + Math.Max(0, component.Y);
                var offsetX = aX;
                var offsetY = aY;
                var childSize = new Size(component.ClientArea!.Value);

                /* WIDTH CHECKS */
                // check left is clipping
                if (component.X + oldX < 0)
                {
                    childSize.W += component.X;
                    offsetX = oldX + component.X;
                }
                // check right is clipping
                if (Math.Max(0, component.X) + childSize.W > viewportSize.W)
                {
                    childSize.W -= (Math.Max(0, component.X) + childSize.W) - viewportSize.W;
                }
                /* HEIGHT CHECKS */
                // check top is clipping
                if (component.Y + oldY < 0)
                {
                    childSize.H += component.Y;
                    offsetY = oldY + component.Y;
                }
                // check bottom is clipping
                if (Math.Max(0, component.Y) + childSize.H > viewportSize.H)
                {
                    childSize.H -= (Math.Max(0, component.Y) + childSize.H) - viewportSize.H;
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

            // restore render area
            graphics.SetRenderArea(viewportSize, viewportX, viewportY);
            graphics.SetGlobalPosition(oldX, oldY);

            if (!DisableTopLevelHooks)
                CallHook(this, EventHook.AfterDraw, graphics, viewportX, viewportY, viewportSize);
        }
    }
}
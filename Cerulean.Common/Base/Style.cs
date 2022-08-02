using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Formats.Asn1;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.Common
{
    public class Style
    {
        private readonly ConcurrentDictionary<string, Setter> _setters = new();
        public Type? TargetType { get; set; }
        public bool ApplyToSelf { get; set; } = true;
        public bool ApplyToChildren { get; set; } = false;

        public void AddSetter(string property, object value)
        {
            if (_setters.TryGetValue(property, out var setter))
            {
                setter.Value = value;
            }
            else
            {
                _setters.TryAdd(property, new Setter(property, value));
            }
        }

        public void ApplyStyle(Component component, bool forced = false)
        {
            // apply style to target component
            var setters = _setters.Values.ToList();
            var applyToSelfOrForced = ApplyToSelf || forced;
            var isMismatchedType = TargetType is not null && component.GetType() != TargetType;
            if (applyToSelfOrForced && !isMismatchedType)
            {
                setters.ForEach(setter => setter.ApplyTo(component));
            }
            
            // apply style to component's children
            if (!ApplyToChildren)
                return;
            var targetChildren = component.Children
                .ToList();
            targetChildren.ForEach(child =>
            {
                if (child.GetType() == TargetType)
                    setters.ForEach(setter => setter.ApplyTo(child));
                ApplyStyle(child, true);
            });
        }

        public void DeriveFrom(Style style)
        {
            style._setters.Values.ToList()
                .ForEach(setter => AddSetter(setter.Property, setter.Value));
        }
    }
}

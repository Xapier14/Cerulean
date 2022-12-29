using System.Reflection;

namespace Cerulean.Common
{
    public sealed class Setter
    {
        public string Property { get; set; }
        public object Value { get; set; }

        public Setter(string property, object value)
        {
            Property = property;
            Value = value;
        }

        public void ApplyTo(Component component)
        {
            var componentType = component.GetType();
            const BindingFlags invokeAttributes = BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty;
            componentType.InvokeMember(Property, invokeAttributes, Type.DefaultBinder, component,
                new[] { Value });
        }
    }
}

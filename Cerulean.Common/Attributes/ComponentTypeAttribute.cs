namespace Cerulean.Common
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ComponentTypeAttribute : Attribute
    {
        public string TypeHint { get; }
        public bool IsLateBound { get; }
        public ComponentTypeAttribute(string typeHint, bool lateBound = true)
        {
            TypeHint = typeHint;
            IsLateBound = lateBound;
        }
    }
}

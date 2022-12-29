namespace Cerulean.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ScopeAttribute : Attribute
    {
        public StyleScope Scope { get; }
        public string? LocalScopeId { get; }

        public ScopeAttribute(StyleScope scope, string? localScopeId = null)
        {
            Scope = scope;
            LocalScopeId = localScopeId;
            if (scope == StyleScope.Local && localScopeId == null)
                throw new ArgumentNullException(nameof(localScopeId),
                    "Local scope id must be specified if using local scope.");
        }
    }
}

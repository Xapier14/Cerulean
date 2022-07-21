namespace Cerulean.CLI
{
    public interface ICommand
    {
        public static string? CommandName { get; set; }

        // Requires .NET 7.0
        // This throws ReflectionTypeLoadException: Method not found when loading.
        // A known issue with the .NET 6 preview of static abstract interface members.
        // https://github.com/dotnet/runtime/issues/59432
        // https://github.com/dotnet/runtime/issues/63411
        
#pragma warning disable S125 // Sections of code should not be commented out
// public static abstract int DoAction(string[] args);
    }
#pragma warning restore S125 // Sections of code should not be commented out
}

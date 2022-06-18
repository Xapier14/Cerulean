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
        // public static abstract void DoAction(string[] args);
    }
}

using System.Text;

namespace Cerulean.Common
{
    public class Resource
    {
        public byte[] Data { get; init; } = Array.Empty<byte>();
        public ResourceType Type { get; init; } = (ResourceType)(-1);
        public override string ToString()
            => Type == ResourceType.Text
                ? Encoding.UTF8.GetString(Data)
                : "";
    }
}

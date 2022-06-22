using System.Runtime.InteropServices;

namespace Cerulean.Core
{
    internal enum TextureType
    {
        Texture,
        Text
    }
    [StructLayout(LayoutKind.Auto)]
    internal struct Texture
    {
        public string Identity { get; init; }
        public TextureType Type { get; init; }
        public object? UserData { get; init; }
        public IntPtr SDLTexture { get; init; }
        public long Score { get; set; }

        public void SetScore(long score)
        {
            Score = score;
        }

        public void AccScore(long acc = -1)
        {
            Score += acc;
        }
    }
}

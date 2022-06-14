﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

namespace Cerulean.Core
{
    public static class MessageBox
    {
        public static void Show(string title, string message, Window? parentWindow = null)
        {
            SDL_ShowSimpleMessageBox(0, title, message, parentWindow?.WindowPtr ?? IntPtr.Zero);
        }
    }
}

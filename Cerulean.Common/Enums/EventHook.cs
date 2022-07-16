using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.Common
{
    public enum EventHook
    {
        BeforeUpdate,
        AfterUpdate,
        BeforeChildUpdate,
        AfterChildUpdate,
        BeforeDraw,
        AfterDraw,
        BeforeChildDraw,
        AfterChildDraw,
        BeforeInit,
        AfterInit,
        GetChild,
        AddChild,
        GetAttribute,
        SetAttribute
    }
}

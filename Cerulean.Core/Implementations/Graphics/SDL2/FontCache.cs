using Cerulean.Common;

using LinkedList = Cerulean.Common.Collections.LinkedList<Cerulean.Core.Font>;
using LinkedListNode = Cerulean.Common.Collections.LinkedListNode<Cerulean.Core.Font>;

namespace Cerulean.Core
{
    internal class FontCache
    {
        private readonly LinkedList _cache;
        public FontCache()
        {
            _cache = new();
        }

        public static string GetID(string name, string style, int pointSize)
        {
            return $"{name.Trim()}#{style.Trim()}@{pointSize}pt";
        }

        public Font GetFont(string name, string style, int pointSize)
        {
            if (!TryGetFont(name, style, pointSize, out var font))
            {
                CeruleanAPI.GetAPI().Log($"Could not find font {name} {style} {pointSize}pt in cache. Loading font...");
                font = Font.LoadFont(name, style, pointSize);
                _cache.AddLast(font);
            }
            if (font is null)
                throw new GeneralAPIException("Font load general error.");
            return font;
        }

        private bool TryGetFont(string name, string style, int pointSize, out Font? font)
        {
            font = null;
            // select a single element with the same Identity as identifier
            var node = _cache.Head;
            while (node is LinkedListNode fontNode)
            {
                if (fontNode.Data.ToString() == GetID(name, style, pointSize))
                {
                    font = fontNode.Data;
                    break;
                }
                node = node.Next;
            }
            var found = font != null;

            // move font node closer to front
            if (node is not null && node.Previous is not null)
            {
                _cache.SwitchNodes(node, node.Previous);
            }

            return found;
        }

        public void Clear()
        {
            foreach (var font in _cache)
                font.Dispose();
            _cache.Clear();
        }

        public int Count()
            => _cache.Count;
    }
}

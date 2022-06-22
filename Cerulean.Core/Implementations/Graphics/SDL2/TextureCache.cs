
using static SDL2.SDL;
using LinkedList = Cerulean.Common.Collections.LinkedList<Cerulean.Core.Texture>;
using LinkedListNode = Cerulean.Common.Collections.LinkedListNode<Cerulean.Core.Texture>;

namespace Cerulean.Core
{
    internal class TextureCache
    {
        private int _maxCount;
        private readonly LinkedList _cache;

        public TextureCache(int maxCount)
        {
            _cache = new();
            _maxCount = maxCount;
        }

        public void AddTexture(Texture texture)
        {
            if (!TryGetTexture(texture.Identity, out _))
            {
                if (_cache.Count >= _maxCount && _cache.Tail is not null)
                {
                    IntPtr ptr = _cache.Tail.Data.SDLTexture;
                    SDL_DestroyTexture(ptr);
                    _cache.DeleteNode(_cache.Tail);
                }
                _cache.AddLast(texture);
            }
        }

        public bool TryGetTexture(string identifier, out Texture? texture)
        {
            texture = null;
            // select a single element with the same Identity as identifier
            LinkedListNode? node = _cache.Head;
            while (node is LinkedListNode textureNode)
            {
                if (textureNode.Data.Identity == identifier)
                {
                    texture = textureNode.Data;
                    break;
                }
                node = node.Next;
            }
            bool found = texture != null;

            // move texture node closer to front
            if (node is not null && node.Previous is not null)
            {
                _cache.SwitchNodes(node, node.Previous);
            }

            return found;
        }

        public void Clear()
        {
            foreach (Texture texture in _cache)
                SDL_DestroyTexture(texture.SDLTexture);
            _cache.Clear();
        }

        public void DevalueTextures()
        {
            for (int i = 0; i < _cache.Count; ++i)
            {
                if (_cache[i].Score <= 0)
                {
                    SDL_DestroyTexture(_cache[i].SDLTexture);
                    _cache.DeleteNode(i);
                    i--;
                }
                else
                {
                    _cache[i].AccScore(-1);
                }
            }
        }

        public int Count()
            => _cache.Count;
    }
}

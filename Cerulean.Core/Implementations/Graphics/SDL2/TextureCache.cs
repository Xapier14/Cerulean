
using static SDL2.SDL;
using LinkedList = Cerulean.Common.Collections.LinkedList<Cerulean.Core.Texture>;
using LinkedListNode = Cerulean.Common.Collections.LinkedListNode<Cerulean.Core.Texture>;

namespace Cerulean.Core
{
    internal class TextureCache
    {
        private int _maxCount;
        private readonly LinkedList _cache;
        internal List<IntPtr> ActiveAllocatedPtrs = new();

        internal long DeletedPointers = 0;

        public TextureCache(int maxCount)
        {
            _cache = new();
            _maxCount = maxCount;
        }

        public void AddTexture(Texture texture)
        {
            if (TryGetTexture(texture.Identity, out _)) return;
            if (_cache.Count >= _maxCount && _cache.Tail is not null)
            {
                var ptr = _cache.Tail.Data.SDLTexture;
                SDL_DestroyTexture(ptr);
                ActiveAllocatedPtrs.Remove(ptr);
                DeletedPointers--;
                _cache.DeleteNode(_cache.Tail);
            }
            _cache.AddLast(texture);
        }

        public bool TryGetTexture(string identifier, out Texture? texture)
        {
            texture = null;
            // select a single element with the same Identity as identifier
            var node = _cache.Head;
            while (node is LinkedListNode textureNode)
            {
                if (textureNode.Data.Identity == identifier)
                {
                    texture = textureNode.Data;
                    break;
                }
                node = node.Next;
            }
            var found = texture != null;

            // move texture node closer to front
            if (node?.Previous != null)
            {
                _cache.SwitchNodes(node, node.Previous);
            }

            return found;
        }

        public void Clear()
        {
            foreach (var texture in _cache)
            {
                SDL_DestroyTexture(texture.SDLTexture);
                ActiveAllocatedPtrs.Remove(texture.SDLTexture);

                DeletedPointers--;
            }

            _cache.Clear();
        }

        public void DevalueTextures()
        {
            for (var i = 0; i < _cache.Count; ++i)
            {
                if (_cache[i].Score <= 0)
                {
                    SDL_DestroyTexture(_cache[i].SDLTexture);
                    ActiveAllocatedPtrs.Remove(_cache[i].SDLTexture);
                    DeletedPointers--;
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


using static SDL2.SDL;
using LinkedList = Cerulean.Common.Collections.LinkedList<Cerulean.Core.Texture>;
using LinkedListNode = Cerulean.Common.Collections.LinkedListNode<Cerulean.Core.Texture>;

namespace Cerulean.Core
{
    internal class TextureCache
    {
        private readonly LinkedList _cache;

        public TextureCache()
        {
            _cache = new();
        }

        public void AddTexture(Texture texture)
        {
            if (!TryGetTexture(texture.Identity, out _))
                _cache.AddLast(texture);
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
    }
}

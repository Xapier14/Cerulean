using System.ComponentModel;

namespace Cerulean.Common
{
    public enum PictureMode
    {
        // Stretch to bounds of frame
        Stretch,
        // center image to bounds of frame without resize
        Center,
        // tile image starting from top-left
        Tile,
        // fit image to frame without cropping (possible empty space)
        Fit,
        // cover frame with image (possible cropping, but no empty space)
        Cover,
        // draw image as-is without scaling and other stuff
        None
    }
}
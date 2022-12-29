namespace Cerulean.Common
{
    public enum PictureMode
    {
        /// <summary>
        /// Stretches the image to the bounds of the container.
        /// </summary>
        Stretch,
        /// <summary>
        /// Positions the image to the center of the container without resizing.
        /// </summary>
        Center,
        /// <summary>
        /// Tiles the image starting from the top-left corner of the container.
        /// </summary>
        Tile,
        /// <summary>
        /// Fits the image into the container without cropping. May introduce empty space.
        /// </summary>
        Fit,
        /// <summary>
        /// Covers the container with the image by cropping. Will not introduce empty space.
        /// </summary>
        Cover,
        /// <summary>
        /// No resizing or cropping.
        /// </summary>
        None
    }
}
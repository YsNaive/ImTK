namespace ImTK.Database
{
    /// <summary>
    /// Represents a 2D Texture asset loaded into the GPU.
    /// </summary>
    public class Texture2D : ImTKAsset
    {
        /// <summary>OpenGL texture handle (GLuint cast to nint).</summary>
        public nint TextureId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        
        /// <summary>
        /// A callback provided by the graphics backend to delete the texture.
        /// </summary>
        public System.Action<nint> DisposeAction { get; set; }

        public override void Dispose()
        {
            if (TextureId != 0 && DisposeAction != null)
            {
                DisposeAction(TextureId);
                TextureId = 0;
            }
            base.Dispose();
        }
    }
}

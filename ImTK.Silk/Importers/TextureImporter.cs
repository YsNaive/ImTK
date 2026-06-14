using System;
using System.IO;
using Silk.NET.OpenGL;
using ImTK.Database;
using ImTK.Database.Importers;
using StbImageSharp;
using ImTK.Log;

namespace ImTK.Silk.Importers
{
    public class TextureImporter : IAssetImporter<Texture2D>
    {
        private GL _gl;

        public TextureImporter(GL gl)
        {
            _gl = gl;
            StbImage.stbi_set_flip_vertically_on_load(0); // ImGui expects top-left origin
        }

        public Texture2D Import(string absolutePath, string normalizedPath)
        {
            try
            {
                using var stream = File.OpenRead(absolutePath);
                var result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                uint handle = _gl.GenTexture();
                _gl.BindTexture(TextureTarget.Texture2D, handle);

                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);

                unsafe
                {
                    fixed (byte* ptr = result.Data)
                    {
                        _gl.TexImage2D(
                            TextureTarget.Texture2D, 
                            0, 
                            InternalFormat.Rgba8, 
                            (uint)result.Width, 
                            (uint)result.Height, 
                            0, 
                            PixelFormat.Rgba, 
                            PixelType.UnsignedByte, 
                            ptr);
                    }
                }

                _gl.BindTexture(TextureTarget.Texture2D, 0);

                return new Texture2D
                {
                    TextureId = (nint)handle,
                    Width = result.Width,
                    Height = result.Height,
                    DisposeAction = (id) => 
                    {
                        _gl.DeleteTexture((uint)id);
                    }
                };
            }
            catch (Exception ex)
            {
                ImTKLog.Error($"Failed to import texture {absolutePath}: {ex.Message}");
                throw;
            }
        }
    }
}

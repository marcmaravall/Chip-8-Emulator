using System;
using OpenTK.Graphics.OpenGL;

namespace Chip_8_Emulator.Graphics
{
    public class VAO : IDisposable
    {
        int Handle;

        public VAO()
        {
            Handle = GL.GenVertexArray();
        }

        public void Bind()
        {
            GL.BindVertexArray(Handle);
        }

        public void Unbind()
        {
            GL.BindVertexArray(0);
        }

        public void SetVertexAttribute(int index, int size, VertexAttribPointerType type, bool normalized, int stride, int offset)
        {
            Bind();
            GL.VertexAttribPointer(index, size, type, normalized, stride, offset);
            GL.EnableVertexAttribArray(index);
            Unbind();
        }

        public void Delete()
        {
            if (Handle != 0)
            {
                GL.DeleteVertexArray(Handle);
                Handle = 0;
            }
        }

        public void Dispose()
        {
            Delete();
            GC.SuppressFinalize(this);
        }
    }
}

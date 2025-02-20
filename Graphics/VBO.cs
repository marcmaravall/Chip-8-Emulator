using System;
using OpenTK.Graphics.OpenGL;

namespace Chip_8_Emulator.Graphics
{
    public class VBO<T> where T : struct
    {
        int Handle;
        BufferTarget bufferType;

        public VBO(BufferTarget type)
        {
            bufferType = type;
            Handle = GL.GenBuffer();
        }

        public void Bind()
        {
            GL.BindBuffer(bufferType, Handle);
        }

        public void SetData(T[] data, BufferUsageHint usageHint = BufferUsageHint.StaticDraw)
        {
            Bind();
            int size = data.Length * sizeof(float);
            GL.BufferData(bufferType, size, data, usageHint);
        }

        public void Unbind()
        {
            GL.BindBuffer(bufferType, 0);
        }

        public void Delete()
        {
            if (Handle != 0)
            {
                GL.DeleteBuffer(Handle);
                Handle = 0;
            }
        }
    }
}

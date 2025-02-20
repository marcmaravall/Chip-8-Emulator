using System;
using Chip_8_Emulator.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Chip_8_Emulator.Graphics
{
    public class Triangle
    {
        public VAO vao { get; private set; }
        public VBO<float> vbo { get; private set; }
        public float[] vertices { get; private set; }

        public Triangle(float[] _vertices)
        {
            this.vertices = _vertices;

            vao = new VAO();
            vao.Bind();
            vbo = new VBO<float>(BufferTarget.ArrayBuffer);
            vao.Bind();
            vbo.SetData(vertices);

            vao.SetVertexAttribute(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            vao.SetVertexAttribute(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            vao.SetVertexAttribute(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));

            vao.Unbind();
            vbo.Unbind();
        }

        public void Render()
        {
            vao.Bind();
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }
    }
}

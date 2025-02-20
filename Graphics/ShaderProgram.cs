using System;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace Chip_8_Emulator
{
    public class ShaderProgram : IDisposable
    {
        public int Handle { get; private set; }

        public ShaderProgram(string vertexPath, string fragmentPath)
        {
            int vertexShader = CompileShader(ShaderType.VertexShader, File.ReadAllText(vertexPath));
            int fragmentShader = CompileShader(ShaderType.FragmentShader, File.ReadAllText(fragmentPath));

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int status);
            if (status == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                throw new Exception($"Error linking shader program: {infoLog}");
            }

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error compiling {type} shader: {infoLog}");
            }

            return shader;
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void Delete()
        {
            if (Handle != 0)
            {
                GL.DeleteProgram(Handle);
                Handle = 0;
            }
        }

        public void SetUniform(string name, float value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, value);
        }

        public void SetUniform(string name, int value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, value);
        }

        public void SetUniform(string name, OpenTK.Mathematics.Vector3 value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform3(location, value);
        }

        public void SetUniform(string name, OpenTK.Mathematics.Matrix4 value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.UniformMatrix4(location, false, ref value);
        }

        public void Dispose()
        {
            Delete();
            GC.SuppressFinalize(this);
        }

        ~ShaderProgram()
        {
            Delete();
        }
    }
}

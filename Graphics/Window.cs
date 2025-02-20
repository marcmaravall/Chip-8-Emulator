using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Chip_8_Emulator.Graphics
{
    public class Window : GameWindow
    {
        public static int CELL_SIZE;
        public static int Width;
        public static int Height;

        public byte[] Display = new byte[64 * 32];
        private Emulator emulator;
        private ShaderProgram shaderProgram;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, Emulator emulator)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            this.emulator = emulator;
            CELL_SIZE = this.Size.Y / 32;
            Width = Size.X;
            Height = Size.Y;
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            const string vertexShaderPath = "Shaders/shader.vert";
            const string fragmentShaderPath = "Shaders/shader.frag";

            shaderProgram = new ShaderProgram(vertexShaderPath, fragmentShaderPath);
            shaderProgram.Use();

            GL.ClearColor(0.1f, 0.5f, 0.1f, 1);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            //Render();
            Pixel pixel = new Pixel(0, 0, true);
            pixel.Render();

            SwapBuffers();
        }

        public void Clear()
        {
            for (int i = 0; i < Display.Length; i++)
            {
                Display[i] = 1;
            }
            GL.Clear(ClearBufferMask.ColorBufferBit);
            Render();
        }

        public void Render()
        {
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    Pixel p = new Pixel(i, j, Display[i + j * 64] == 1);
                    p.Render();
                }
            }
        }

        public void OnKeyDown(byte key)
        {
            emulator.Keys[key] = true;
            Console.WriteLine("Key down " + key);
        }

        public void OnKeyUp(byte key)
        {
            emulator.Keys[key] = false;
        }
    }
}

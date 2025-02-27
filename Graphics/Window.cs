using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Threading;

namespace Chip_8_Emulator.Graphics
{
    public class Window : GameWindow
    {
        public static int CELL_SIZE;
        public static int Width;
        public static int Height;

        public byte[] Display = new byte[64 * 32];
        public byte DelayTimer = 255;
        public byte SoundTimer = 0;

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

            StartTimers();

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        private void StartTimers()
        {
            new Thread(() =>
            {
                while (true)
                {
                    if (DelayTimer > 0) DelayTimer--;
                    if (SoundTimer > 0)
                    {
                        SoundTimer--;
                        PlaySound();
                    }
                    Thread.Sleep(16);
                }
            })
            { IsBackground = true }.Start();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            ManageInput();

            Render();

            SwapBuffers();
        }

        private void PlaySound()
        {
            Console.Beep(440, 100);
        }

        public void Clear()
        {
            for (int i = 0; i < Display.Length; i++)
            {
                Display[i] = 0;
            }
            GL.Clear(ClearBufferMask.ColorBufferBit);
            Render();
        }

        public void Render()
        {
            Pixel p;
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if (Display[i + j * 64] == 0)
                        continue;
                    p = new Pixel(i, j, Display[i + j * 64] != 0);
                    p.Render();
                }
            }
        }

        private void ManageInput()
        {
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            Keys[] keys = new Keys[16]
            {
                Keys.D1, Keys.D2, Keys.D3, Keys.D4,
                Keys.Q, Keys.W, Keys.E, Keys.R,
                Keys.A, Keys.S, Keys.D, Keys.F,
                Keys.Z, Keys.X, Keys.C, Keys.V
            };

            for (int i = 0; i < 16; i++)
            {
                if (KeyboardState.IsKeyDown(keys[i]))
                {
                    emulator.Keys[i] = true;
                }
                else
                {
                    emulator.Keys[i] = false;
                }
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }
    }
}

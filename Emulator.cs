using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using Chip_8_Emulator.Graphics;

namespace Chip_8_Emulator
{
    public partial class Emulator
    {
        public byte[] Memory { get; private set; } = new byte[4096];
        private byte[] V = new byte[16];
        private byte I = 0x050;
        public bool[] Keys = new bool[16];
        public ushort PC { get; private set; } = 0x200;

        private Window window;
        public int Size { get; private set; }
        public Emulator(int size) 
        {
            Size = size;
        }

        public void Run()
        {
            var nativeSettings = new NativeWindowSettings()
            {
                ClientSize = new OpenTK.Mathematics.Vector2i(64, 32) * Size,
                Title = "Chip-8 Emulator",
                MaximumClientSize = new OpenTK.Mathematics.Vector2i(64, 32) * Size,
                MinimumClientSize = new OpenTK.Mathematics.Vector2i(64, 32) * Size,
            };
            window = new Window(GameWindowSettings.Default, nativeSettings, this);

            LoadROM(@"C:\Users\marcm\source\repos\Chip-8-Emulator\ROMS\Tetris [Fran Dachille, 1991].ch8");
            StartExecution();

            window.Run();
        }

        public void LoadROM(string path)
        {
            byte[] romData = File.ReadAllBytes(path);

            if (romData.Length > (4096 - 0x200))
            {
                throw new Exception("ROM demasiado grande para la memoria del Chip-8.");
            }

            Array.Copy(romData, 0, Memory, 0x200, romData.Length);
        }

        public void StartExecution()
        {
            window.UpdateFrame += (FrameEventArgs e) => ExecuteInstruction();
        }


        private void ExecuteInstruction()
        {
            ushort opcode = (ushort)(Memory[PC] << 8 | Memory[PC + 1]);

            // Basic instructions
            if ((opcode & 0xF000) == 0x6000) // 6XNN
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                byte NN = (byte)(opcode & 0x00FF);

                V[X] = NN;
                PC += 2;
            }
            else if ((opcode & 0xF000) == 0x1000) // 1NNN
            {
                ushort NNN = (ushort)(opcode & 0x0FFF);
                PC = NNN;
            }
            else if ((opcode & 0xF000) == 0x7000) // 7XNN
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                byte NN = (byte)(opcode & 0x00FF);

                V[X] += NN;
                PC += 2;
            }
            else if (opcode == 00E0) // 00E0
            {
                window.Clear();
                PC += 2;
            }

            // Flow control

            else if (((opcode & 0xF000) == 0x3000)) // 3XNN
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                byte NN = (byte)(opcode & 0x00FF);

                if (V[X] == NN)
                {
                    PC += 2;
                }

                PC += 2;
            }
            else if ((opcode & 0xF000) == 0x4000) // 4XNN
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                byte NN = (byte)(opcode & 0x00FF);

                if (V[X] != NN)
                {
                    PC += 2;
                }

                PC += 2;
            }
            else if ((opcode & 0xF000) == 0x5000) // 5XY0
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                byte Y = (byte)((opcode & 0x00F0) >> 4);

                if (V[X] == V[Y])
                {
                    PC+= 2;
                }
                PC += 2;
            }

            // Records and mathematical operations

            else if ((opcode & 0xF000) == 0x8000) // 8XY_
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                byte Y = (byte)((opcode & 0x00F0) >> 4);

                if ((opcode & 0x000F) == 0x0000) // 8XY0 - Vx = Vy
                {
                    V[X] = V[Y];
                    PC += 2;
                }
                else if ((opcode & 0x000F) == 0x0001) // 8XY1 - Vx = Vx OR Vy
                {
                    V[X] |= V[Y];
                    PC += 2;
                }
                else if ((opcode & 0x000F) == 0x0002) // 8XY2 - Vx = Vx AND Vy
                {
                    V[X] &= V[Y];
                    PC += 2;
                }
                else if ((opcode & 0x000F) == 0x0003) // 8XY3 - Vx = Vx XOR Vy
                {
                    V[X] ^= V[Y];
                    PC += 2;
                }
                else if ((opcode & 0x000F) == 0x0004) // 8XY4 - ADD Vx, Vy (with carry)
                {
                    int sum = V[X] + V[Y];
                    V[X] = (byte)sum;
                    V[0xF] = (byte)((sum > 255) ? 1 : 0);
                    PC += 2;
                }
                else if ((opcode & 0x000F) == 0x0005) // 8XY5
                {
                    int sum = V[X] - V[Y];
                    V[X] = (byte)sum;
                    V[0xF] = (byte)((sum < 255) ? 1 : 0);
                    PC += 2;
                }
            }

            // Graphics and user input

            else if ((opcode & 0xF000) == 0xD000) // DXYN
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                byte Y = (byte)((opcode & 0x00F0) >> 4);
                byte N = (byte)(opcode & 0x000F);

                ushort spriteStartAddr = I;
                V[0xF] = 0;

                for (int row = 0; row < N; row++)
                {
                    byte spriteRow = Memory[spriteStartAddr + row];

                    for (int col = 0; col < 8; col++)
                    {
                        int screenX = (V[X] + col) % 64;
                        int screenY = (V[Y] + row) % 32;

                        bool spritePixel = (spriteRow & (0x80 >> col)) != 0;

                        if (spritePixel)
                        {
                            int screenIndex = screenY * 64 + screenX;
                            if (window.Display[screenIndex] == 1)
                            {
                                V[0xF] = 1;
                            }
                            window.Display[screenIndex] ^= 1;
                        }
                    }
                }

                PC += 2;
            }
            else if ((opcode & 0xF0FF) == 0xE09E) // E09E
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                byte key = V[X];

                if (Keys[key])
                {
                    PC += 2;
                }
                    PC += 2;
            }
            else if ((opcode & 0xF0FF) == 0xE0A1) // EXA1
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                byte key = V[X];

                if (!Keys[key])
                {
                    PC += 2;
                }
                PC += 2;
            }

            else
            {
                PC += 2;
            }
        }
    }
}

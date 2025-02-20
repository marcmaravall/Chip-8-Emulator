using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using Chip_8_Emulator.Graphics;

namespace Chip_8_Emulator
{
    public partial class Emulator
    {
        public byte[] Memory { get; private set; } = new byte[4096];
        private byte[] V = new byte[16];
        private ushort[] Stack = new ushort[16];
        private byte SP = 0;

        private ushort I = 0x050;
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

            LoadROM(@"ROMS\Tetris [Fran Dachille, 1991].ch8");
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
            // Console.WriteLine($"Ejecutando opcode: {opcode:X4}");

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
            else if ((opcode & 0xF000) == 0x2000) // 2NNN
            {
                ushort NNN = (ushort)(opcode & 0x0FFF);
                Stack[SP++] = PC;
                PC = NNN;
            }
            else if ((opcode & 0xF000) == 0x7000) // 7XNN
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                byte NN = (byte)(opcode & 0x00FF);

                V[X] += NN;
                PC += 2;
            }
            else if (opcode == 0x00E0) // 00E0
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
                    PC += 2;
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
                // Console.WriteLine("DXYN");

                byte X = (byte)((opcode & 0x0F00) >> 8);
                byte Y = (byte)((opcode & 0x00F0) >> 4);
                byte N = (byte)(opcode & 0x000F);

                // Console.WriteLine($"X:{X} Y:{Y} N: {N}");
                // Console.WriteLine($"Sprite en I={I:X3}: {Convert.ToString(Memory[I], 2).PadLeft(8, '0')}");

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

                //window.Render();

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

            else if ((opcode & 0xF000) == 0x2000) // 2NNN - Call subroutine at NNN
            {
                SP++;
                ushort NNN = (ushort)(opcode & 0x0FFF);
                PC = NNN;
            }

            else if (opcode == 0x00EE) // 00EE - Return from subroutine
            {
                PC = Stack[--SP];

                PC += 2;
            }

            else if ((opcode & 0xF0FF) == 0XF007) // FX07
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                V[X] = window.DelayTimer;

                PC += 2;
            }
            else if ((opcode & 0xF0FF) == 0XF015) // FX15
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                window.DelayTimer = V[X];
                PC += 2;
            }
            else if ((opcode & 0xF0FF) == 0XF018) // FX18
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                window.SoundTimer = V[X];
                PC += 2;
            }
            else if ((opcode & 0xF0FF) == 0XF01E) // FX1E
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                I += V[X];
                PC += 2;
            }
            else if ((opcode & 0xF0FF) == 0XF033) // FX33
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                Memory[I] = (byte)(V[X] / 100);
                Memory[I + 1] = (byte)((V[X] / 10) % 10);
                Memory[I + 2] = (byte)((V[X] % 100) % 10);
                PC += 2;
            }
            else if ((opcode & 0xF0FF) == 0XF065) // FX65
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                for (int i = 0; i <= X; i++)
                {
                    V[i] = Memory[I + i];
                }
                PC += 2;
            }
            else if ((opcode & 0xF0FF) == 0xF029) // FX29 - Set I to the location of the sprite for the character in Vx
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                I = (ushort)(V[X] * 5);
                PC += 2;
            }

            else if ((opcode & 0xF000) == 0xC000) // CXNN - Set Vx to a random number with a mask of NN
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                byte NN = (byte)(opcode & 0x00FF);

                V[X] = (byte)(new Random().Next(0, 256) & NN);
                PC += 2;
            }
            else if ((opcode & 0xF0FF) == 0xF055) // FX55
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                for (int i = 0; i <= X; i++)
                {
                    Memory[I + i] = V[i];
                }
                I += (ushort)(X + 1);
                PC += 2;
            }


            else if ((opcode & 0xF000) == 0xA000) // ANNN - Set I to NNN
            {
                I = (ushort)(opcode & 0x0FFF);
                // Console.WriteLine($"I is: {I:X3}");
                PC += 2;
            }
             
            else if ((opcode & 0xF000) == 0x9000) // 9XY0
            {
                byte X = (byte)((opcode & 0x0F00) >> 8);
                byte Y = (byte)((opcode & 0x00F0) >> 4);

                if (V[X] != V[Y])
                {
                    PC += 2;
                }
                PC += 2;
            }

            else
            {
                Console.WriteLine("Unknown instruction: "+opcode);
                PC += 2;
            }
        }
    }
}

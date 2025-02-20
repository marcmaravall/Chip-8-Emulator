// See https://aka.ms/new-console-template for more information
using Chip_8_Emulator;
using Chip_8_Emulator.Graphics;
using OpenTK.Windowing.Desktop;

Emulator emulator = new Emulator(15);
emulator.Run();

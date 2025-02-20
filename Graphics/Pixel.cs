using System;

namespace Chip_8_Emulator.Graphics
{
    public class Pixel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int XScreen { get => X * Window.CELL_SIZE; }
        public int YScreen { get => Y * Window.CELL_SIZE; }

        private Triangle[] render;
        private bool color;

        public Pixel(int x, int y, bool color)
        {
            X = x;
            Y = y;

            UpdateColor(color);
        }

        private void UpdateColor(bool color)
        {
            this.color = color;
            int c = color ? 1 : 0;

            float size = Window.CELL_SIZE/2;

            float normalizedX = (XScreen / (float)Window.Width) * 2f - 1f;
            float normalizedY = (YScreen / (float)Window.Height) * -2f + 1f;

            render = new Triangle[2]
            {
                new Triangle(new float[]
                {
                    normalizedX - size, normalizedY - size, 0,   c, c, c,    0, 0,
                    normalizedX + size, normalizedY - size, 0,   c, c, c,    0, 0,
                    normalizedX - size, normalizedY + size, 0,   c, c, c,    0, 0
                }),
                new Triangle(new float[]
                {
                    normalizedX + size, normalizedY + size, 0,   c, c, c,    0, 0,
                    normalizedX + size, normalizedY - size, 0,   c, c, c,    0, 0,
                    normalizedX - size, normalizedY + size, 0,   c, c, c,    0, 0
                })
            };

            Console.WriteLine($"Triángulo 1: ({XScreen - size}, {YScreen - size}), ({XScreen + size}, {YScreen - size}), ({XScreen - size}, {YScreen + size})");
            Console.WriteLine($"Triángulo 2: ({XScreen + size}, {YScreen + size}), ({XScreen + size}, {YScreen - size}), ({XScreen - size}, {YScreen + size})");

        }

        public void Render()
        {
            Console.WriteLine($"Pixel lógico: ({X}, {Y}) -> Pixel en pantalla: ({XScreen}, {YScreen})");
            render[0].Render();
            //render[1].Render();
        }
    }
}

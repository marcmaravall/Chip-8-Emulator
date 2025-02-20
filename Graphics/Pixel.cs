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

            float sizeX = (Window.CELL_SIZE / (float)Window.Width) * 2f;
            float sizeY = (Window.CELL_SIZE / (float)Window.Height) * 2f;

            float normalizedX = (XScreen / (float)Window.Width) * 2f - 1f;
            float normalizedY = (YScreen / (float)Window.Height) * -2f + 1f;

            render = new Triangle[2]
            {
                new Triangle(new float[]
                {
                    normalizedX, normalizedY, 0,   c, c, c,    0, 0,
                    normalizedX + sizeX, normalizedY, 0,   c, c, c,    0, 0,
                    normalizedX, normalizedY - sizeY, 0,   c, c, c,    0, 0
                }),
                new Triangle(new float[]
                {
                    normalizedX + sizeX, normalizedY - sizeY, 0,   c, c, c,    0, 0,
                    normalizedX + sizeX, normalizedY, 0,   c, c, c,    0, 0,
                    normalizedX, normalizedY - sizeY, 0,   c, c, c,    0, 0
                })
            };
        }


        public void Render()
        {
            render[0].Render();
            render[1].Render();
        }
    }
}

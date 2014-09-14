using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using AForge;

namespace dpi_aforge_bluetooth_control
{
    public class Ball
    {
        private Bitmap image; //Extracted(transformed) image of the ball
        private Color color;
        private DoublePoint corners;
        private int radius;

        public Ball(Bitmap ballImg, DoublePoint pontos, float raio, Color cor)
        {
            image = ballImg;
            corners = pontos;
            radius = (int)raio;
            color = cor;
        }

        public Bitmap Image { get { return image; } }
        public Color Cor { get { return color; } }
        public DoublePoint Corners { get { return corners; } }
        public int Radius { get { return radius; } }

        public override string ToString()
        {
            if (color == Color.Red) return "Red";
            if (color == Color.Green) return "Green";
            return "I don't know...";
        }
    }
}

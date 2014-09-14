using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace dpi_aforge_bluetooth_control
{
    public class BallCollection : CollectionBase
    {
        public void Add(Ball ball)
        {
            this.List.Add(ball);
        }
        public Ball this[int index]
        {
            get { return this.List[index] as Ball; }
            set { this.List[index] = value; }
        }

        public List<Bitmap> ToImageList()
        {
            List<Bitmap> list = new List<Bitmap>();

            foreach (Ball ball in this.List)
                list.Add(ball.Image);

            return list;
        }
    }
}

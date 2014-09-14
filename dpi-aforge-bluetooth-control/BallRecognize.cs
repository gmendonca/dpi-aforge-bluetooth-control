using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
namespace dpi_aforge_bluetooth_control
{
    public class BallRecognize
    {
        public Bitmap VerVer(Bitmap bmp)
        {
            Bitmap temp2 = new Bitmap(bmp);
            BitmapData imageData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadOnly, bmp.PixelFormat);
            int height;
            int width;

            unsafe
            {
                try
                {
                    UnmanagedImage img = new UnmanagedImage(imageData);

                    height = img.Height;
                    width = img.Width;
                    int pixelSize = (img.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                    byte* p = (byte*)img.ImageData.ToPointer();

                    // for each line
                    for (int y = 0; y < height; y++)
                    {
                        // for each pixel
                        for (int x = 0; x < width; x++, p += pixelSize)
                        {
                            int r = (int)p[RGB.R]; //Red pixel value
                            int g = (int)p[RGB.G]; //Green pixel value
                            int b = (int)p[RGB.B]; //Blue pixel value

                            if (r > g * 1.5 && r > b * 1.5 && r > 150) ;
                            else if (g > r * 1.1 && g > b * 1.1 &&  g > 80) ;
                            else temp2.SetPixel(x, y, Color.Black);
                        }
                    }
                }
                finally
                {
                    bmp.UnlockBits(imageData); //Unlock
                }
            }
            return temp2;
        }

        public Color QualCor(Bitmap bmp)
        {
            BitmapData imageData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
    ImageLockMode.ReadOnly, bmp.PixelFormat);
            int totalRed = 0;
            int totalGreen = 0;
            int totalBlue = 0;
            int height;
            int width;
            int numPix;

            unsafe
            {
                //Count red and green and blue pixels
                try
                {
                    UnmanagedImage img = new UnmanagedImage(imageData);

                    height = img.Height;
                    width = img.Width;
                    int pixelSize = (img.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                    byte* p = (byte*)img.ImageData.ToPointer();

                    // for each line
                    for (int y = 0; y < height; y++)
                    {
                        // for each pixel
                        for (int x = 0; x < width; x++, p += pixelSize)
                        {
                            int r = (int)p[RGB.R]; //Red pixel value
                            int g = (int)p[RGB.G]; //Green pixel value
                            int b = (int)p[RGB.B]; //Blue pixel value

                            totalRed += r;
                            totalGreen += g;
                            totalBlue += b;
                        }
                    }
                }
                finally
                {
                    bmp.UnlockBits(imageData); //Unlock
                }
            }

            numPix = width * height;

            totalRed = totalRed / numPix;
            totalBlue = totalBlue / numPix;
            totalGreen = totalGreen / numPix;

            if (totalRed / 1.5 > (int)((totalBlue + totalGreen) / 2)) return Color.Red;
            if (totalGreen / 1.1 > (int)((totalRed + totalBlue) / 2)) return Color.Green;
            return Color.Black;
        }

        public BallCollection Recognize(Bitmap source)
        {
            BallCollection collection = new BallCollection();  //Collection that will hold balls

            //source.Save("D:\\Original.bmp");

            Bitmap temp = VerVer(source);
            //temp.Save("D:\\STR.bmp");

            FiltersSequence seq = new FiltersSequence();
            seq.Add(Grayscale.CommonAlgorithms.BT709);
            temp = seq.Apply(temp); // Apply filters on source image
            //temp.Save("D:\\STR1.bmp");

            seq.Clear();
            seq.Add(new OtsuThreshold());
            temp = seq.Apply(temp);
            //temp.Save("D:\\STR2.bmp");

            //Extract blobs from image whose size width and height larger than 150
            BlobCounter extractor = new BlobCounter();
            extractor.FilterBlobs = true;
            extractor.MinWidth = extractor.MinHeight = 15;
            extractor.MaxWidth = extractor.MaxHeight = 35;
            extractor.ProcessImage(temp);

            IntPoint minXY, maxXY;

            foreach (Blob blob in extractor.GetObjectsInformation())
            {
                List<IntPoint> edgePoints = extractor.GetBlobsEdgePoints(blob);
                //List<IntPoint> corners = 
                PointsCloud.GetBoundingRectangle(edgePoints, out minXY, out maxXY);
                IntPoint cloudSize = maxXY - minXY;
                DoublePoint center = minXY + (DoublePoint)cloudSize / 2;
                float radius = ((float)cloudSize.X + cloudSize.Y) / 4;

                Crop crop = new Crop(blob.Rectangle);
                Bitmap ballImage = crop.Apply(source);

                float meanDistance = 0;

                for (int i = 0, n = edgePoints.Count; i < n; i++)
                {
                    meanDistance += Math.Abs(
                        (float)center.DistanceTo(edgePoints[i]) - radius);
                }
                meanDistance /= edgePoints.Count;

                float maxDitance = ((float)cloudSize.X + cloudSize.Y) / (2 * 0.03f);
                Bitmap bmp = new Bitmap(ballImage);

                Color cor = QualCor(bmp);
                if (meanDistance <= maxDitance && cor != Color.Black)
                {
                    Ball ball = new Ball(ballImage, center, radius, cor);
                    collection.Add(ball);
                }
            }
            return collection;
        }
    }
}

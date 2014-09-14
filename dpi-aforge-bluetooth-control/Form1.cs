using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using Microsoft.Ink;

namespace dpi_aforge_bluetooth_control
{
    public partial class Form1 : Form
    {
        private const int CameraWidth = 320;  // constant Width
        private const int CameraHeight = 240; // constant Height

        private FilterInfoCollection cameras; //Collection of Cameras that connected to PC
        private VideoCaptureDevice device; //Current chosen device(camera) 
        private Dictionary<string, string> cameraDict = new Dictionary<string, string>();
        private Pen pen = new Pen(Brushes.Orange, 4); //is used for drawing rectangle around card
        public Font font = new Font("Tahoma", 15, FontStyle.Bold); //is used for writing string on card
        private BallRecognize recognize = new BallRecognize();
        private BallCollection balls;

        private int frameCounter = 0;
        private int flag = 0;

        private int CompMinDistancia = 8;
        private double DistanciaBolas = 0;

        public Form1()
        {
            InitializeComponent();

            //Fetch cameras 
            this.cameras = new FilterInfoCollection(AForge.Video.DirectShow.FilterCategory.VideoInputDevice);
            int i = 1;
            foreach (AForge.Video.DirectShow.FilterInfo camera in this.cameras)
            {
                if (!this.cameraDict.ContainsKey(camera.Name))
                    this.cameraDict.Add(camera.Name, camera.MonikerString);
                else
                {
                    this.cameraDict.Add(camera.Name + "-" + i.ToString(), camera.MonikerString);
                    i++;
                }
            }
            this.cbCamera.DataSource = new List<string>(cameraDict.Keys); //Bind camera names to combobox

            if (this.cbCamera.Items.Count == 0)
                button1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Start")
            {
                this.button1.Text = "Stop";
                this.device = new VideoCaptureDevice(this.cameraDict[cbCamera.SelectedItem.ToString()]);
                this.device.NewFrame += videoNewFrame;
                this.device.DesiredFrameSize = new Size(CameraWidth, CameraHeight);

                device.Start(); //Start Device
            }
            else
            {
                this.StopCamera();
                button1.Text = "Start";
                this.inkPicture1.Image = null;
            }
        }

        private Bitmap ResizeBitmap(Bitmap bmp)
        {
            ResizeBilinear resizer = new ResizeBilinear(inkPicture1.Width, inkPicture1.Height);

            return resizer.Apply(bmp);
        }

        private List<GraphicsPath> InkToGraphicsPaths()
        {
            Renderer renderer = inkPicture1.Renderer;
            Strokes strokes = inkPicture1.Ink.Strokes;

            if (strokes.Count > 0)
            {
                using (Graphics g = this.CreateGraphics())
                {
                    List<GraphicsPath> paths =
                        new List<GraphicsPath>(strokes.Count);
                    foreach (Stroke stroke in strokes)
                    {
                        Point[] points = stroke.GetPoints();
                        for (int i = 0; i < points.Length; i++)
                        {
                            renderer.InkSpaceToPixel(g, ref points[i]);
                        }
                        GraphicsPath path = new GraphicsPath();
                        path.AddPolygon(points);
                        path.CloseFigure();
                        paths.Add(path);
                    }
                    return paths;
                }
            }
            return null;
        }

        private void videoNewFrame(object sender, NewFrameEventArgs args)
        {

            Bitmap temp = args.Frame.Clone() as Bitmap;

            try
            {
                frameCounter++;

                if (frameCounter > 2)
                {
                    balls = recognize.Recognize(temp);
                    frameCounter = 0;
                }

                using (Graphics graph = Graphics.FromImage(temp))
                {
                    foreach (Ball ball in balls)
                    {
                        graph.DrawEllipse(pen, (int)(ball.Corners.X - ball.Radius), (int)(ball.Corners.Y - ball.Radius), ball.Radius * 2, ball.Radius * 2);
                    }
                }
            }
            catch { }
            this.inkPicture1.Image = ResizeBitmap(temp);
        }

        private void StopCamera()
        {
            if (device != null && device.IsRunning)
            {
                device.SignalToStop(); //stop device
                device.WaitForStop();
                device = null;
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.StopCamera();
            var by = new char[1];
            if (serialPort1.IsOpen)
            {
                by[0] = '3';
                serialPort1.Write(by, 0, 1);
                serialPort1.Close();
                timer1.Enabled = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (inkPicture1.Ink.Strokes.Count == 0) return;

            //var by = new char[1];
            //if (flag == 0)
            //{
            //    by[0] = '2';
            //    serialPort1.Write(by, 0, 1);
            //    flag = 1;
            //    return;
            //}

            List<GraphicsPath> pontos = InkToGraphicsPaths();

            Ball vermelha = balls.Cast<Ball>().Where(b => b.Cor == Color.Red).FirstOrDefault();
            Ball verde = balls.Cast<Ball>().Where(b => b.Cor == Color.Green).FirstOrDefault();

            if (vermelha == null) return;
            if (verde == null) return;

            //int centro = 0;
            //int cima = 0;
            //int baixo = 0;
            //foreach (var p in pontos.First().PathPoints)
            //{
            //    if (p.Y <= (vermelha.Corners.Y - (int)(vermelha.Radius / 3)))
            //    {
            //        baixo++;
            //    }
            //    if (p.Y >= (vermelha.Corners.Y + (int)(vermelha.Radius / 3)))
            //    {
            //        cima++;
            //    }
            //    if (p.Y <= (vermelha.Corners.Y + (int)(vermelha.Radius / 3)) && p.Y >= (vermelha.Corners.Y - (int)(vermelha.Radius / 3)))
            //    {
            //        centro++;
            //    }
            //}

            //if (centro / 1.2 > baixo && centro / 1.2 > cima)
            //{
            //    by[0] = '2';
            //    serialPort1.Write(by, 0, 1);
            //}
            //if (baixo / 1.2 > centro && baixo / 1.2 > cima)
            //{
            //    by[0] = '0';
            //    serialPort1.Write(by, 0, 1);
            //}
            //if (cima / 1.2 > centro && cima / 1.2 > baixo)
            //{
            //    by[0] = '1';
            //    serialPort1.Write(by, 0, 1);
            //}

            double x = verde.Corners.X - vermelha.Corners.X;
            double y = verde.Corners.Y - vermelha.Corners.Y;

            double minDistancia = 1000;
            double [] pos = new double[2];
            char [] by = new char[1];
            

            foreach (var p in pontos.First().PathPoints)
            {
                double distancia = Math.Sqrt((Math.Pow(p.X - vermelha.Corners.X, 2) + (Math.Pow(p.Y - vermelha.Corners.Y, 2))));
                if (distancia < minDistancia)
                {
                    minDistancia = distancia;
                    pos[0] = p.X;
                    pos[1] = p.Y;
                }
            }
            

            DistanciaBolas = Math.Sqrt((Math.Pow(verde.Corners.X - vermelha.Corners.X, 2) + (Math.Pow(verde.Corners.Y - vermelha.Corners.Y, 2))));
            Console.WriteLine(DistanciaBolas);
            Console.WriteLine(minDistancia);
            if (x > DistanciaBolas * 0.87)
            {
                Console.WriteLine("Horizontal Positivo");

                if (minDistancia < CompMinDistancia)
                {
                    by[0] = '2';
                    serialPort1.Write(by, 0, 1);
                }
                else if (vermelha.Corners.Y > pos[1])
                {
                    by[0] = '1';
                    serialPort1.Write(by, 0, 1);
                }
                else
                {
                    by[0] = '0';
                    serialPort1.Write(by, 0, 1);
                }
            }
            else if (x < -DistanciaBolas * 0.87)
            {
                Console.WriteLine("Horizontal Negativo");

                if (minDistancia < CompMinDistancia)
                {
                    by[0] = '2';
                    serialPort1.Write(by, 0, 1);
                }
                else if (vermelha.Corners.Y > pos[1])
                {
                    by[0] = '0';
                    serialPort1.Write(by, 0, 1);
                }
                else
                {
                    by[0] = '1';
                    serialPort1.Write(by, 0, 1);
                }
            }
            else if (y > DistanciaBolas * 0.87)
            {
                Console.WriteLine("Vertical Positivo");

                if (minDistancia < CompMinDistancia)
                {
                    by[0] = '2';
                    serialPort1.Write(by, 0, 1);
                }
                else if (vermelha.Corners.X > pos[0])
                {
                    by[0] = '0';
                    serialPort1.Write(by, 0, 1);
                }
                else
                {
                    by[0] = '1';
                    serialPort1.Write(by, 0, 1);
                }
            }
            else if (y < -DistanciaBolas * 0.87)
            {
                Console.WriteLine("Vertical Negativo");

                if (minDistancia < CompMinDistancia)
                {
                    by[0] = '2';
                    serialPort1.Write(by, 0, 1);
                }
                else if (vermelha.Corners.X > pos[0])
                {
                    by[0] = '1';
                    serialPort1.Write(by, 0, 1);
                }
                else
                {
                    by[0] = '0';
                    serialPort1.Write(by, 0, 1);
                }
            }
            else if (x > 0 && y > 0)
            {
                Console.WriteLine("Diagonal Secundária Positiva");
                if (minDistancia < CompMinDistancia)
                {
                    by[0] = '2';
                    serialPort1.Write(by, 0, 1);
                }
                else if (vermelha.Corners.X < pos[0])
                {
                    by[0] = '1';
                    serialPort1.Write(by, 0, 1);
                }
                else
                {
                    by[0] = '0';
                    serialPort1.Write(by, 0, 1);
                }
            }
            else if (x > 0 && y < 0)
            {
                Console.WriteLine("Diagonal Principal Negativa");
                if (minDistancia < CompMinDistancia)
                {
                    by[0] = '2';
                    serialPort1.Write(by, 0, 1);
                }
                else if (vermelha.Corners.X < pos[0])
                {
                    by[0] = '0';
                    serialPort1.Write(by, 0, 1);
                }
                else
                {
                    by[0] = '1';
                    serialPort1.Write(by, 0, 1);
                }
            }
            else if (x < 0 && y > 0)
            {
                Console.WriteLine("Diagonal Principal Positiva");
                if (minDistancia < CompMinDistancia)
                {
                    by[0] = '2';
                    serialPort1.Write(by, 0, 1);
                }
                else if (vermelha.Corners.X < pos[0])
                {
                    by[0] = '1';
                    serialPort1.Write(by, 0, 1);
                }
                else
                {
                    by[0] = '0';
                    serialPort1.Write(by, 0, 1);
                }
            }
            else if (x < 0 && y < 0)
            {
                Console.WriteLine("Diagonal Secundaria Negativa");
                if (minDistancia < CompMinDistancia)
                {
                    by[0] = '2';
                    serialPort1.Write(by, 0, 1);
                }
                else if (vermelha.Corners.X < pos[0])
                {
                    by[0] = '0';
                    serialPort1.Write(by, 0, 1);
                }
                else
                {
                    by[0] = '1';
                    serialPort1.Write(by, 0, 1);
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Open();
                    timer1.Enabled = true;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            inkPicture1.InkEnabled = false;
            inkPicture1.Ink = new Microsoft.Ink.Ink();
            inkPicture1.InkEnabled = true;
            inkPicture1.Invalidate();

            var by = new char[1];
            if (serialPort1.IsOpen)
            {
                by[0] = '3';
                serialPort1.Write(by, 0, 1);
                serialPort1.Close();
                timer1.Enabled = false;
            }
        }
    }
}

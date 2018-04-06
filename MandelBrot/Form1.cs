using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpAvi;
using SharpAvi.Output;
using SharpAvi.Codecs;
using System.Threading;

namespace MandelBrot
{
    public partial class Form1 : Form
    {
        public DirectBitmap currentFractal;
        public Task Manager;
        List<Task> threads = new List<Task>();
        List<Frame> workingFrames = new List<Frame>();
        public Frame lastFrame;
        public int frameCount = 0;
        public AviWriter videoFile;
        public IAviVideoStream stream;
        public bool rendering = false;
        public int max_iteration = 5000;
        double offsetX = -0.743643887037158704752191506114774;
        double offsetY =  0.131825904205311970493132056385139;
        public Form1()
        {
            InitializeComponent();
        }
        private byte[] BitmapToByteArray(Bitmap img)
        {
            // and buffer of appropriate size for storing its bits
            var buffer = new byte[stream.Width * stream.Height * 4];

            // Now copy bits from bitmap to buffer
            var bits = img.LockBits(new Rectangle(0, 0, stream.Width, stream.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            Marshal.Copy(bits.Scan0, buffer, 0, buffer.Length);
            img.UnlockBits(bits);
            return buffer;
        }
        public double Map(double OldValue, double OldMin, double OldMax, double NewMin, double NewMax)
        {
            double OldRange = (OldMax - OldMin);
            double NewRange = (NewMax - NewMin);
            double NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
            return NewValue;
        }
        public void ThreadManager()
        {
            while (threads.Count > 0)
            {
                threads[0].Wait();
                workingFrames = workingFrames.OrderBy(f => f.frameNum).ToList();
                byte[] frame = BitmapToByteArray(workingFrames[0].Bmp.Bitmap);
                stream.WriteFrame(true, frame, 0, frame.Length);
                pictureBox1.Image = workingFrames[0].Bmp.Bitmap;
                if (lastFrame != null)
                {
                    lastFrame.Bmp.Dispose();
                }
                lastFrame = workingFrames[0];
                threads.RemoveAt(0);
                workingFrames.RemoveAt(0);
                if (rendering)
                {
                    threads.Add(Task.Run(new Action(MandelBrot)));
                }
            }
        }
        public void MandelBrot()
        {
            frameCount++;
            Frame frame = new Frame { frameNum = frameCount };
            DirectBitmap fractal = new DirectBitmap(currentFractal.Width, currentFractal.Height);
            for (var px = 0; px < fractal.Width; px++)
            {
                for (var py = 0; py < fractal.Height; py++)
                {
                    var zoom = Math.Pow(frame.frameNum, frame.frameNum / 100.0);
                    var x0 = Map(px, 0, fractal.Width, -0.15 / zoom + offsetX, 0.15 / zoom + offsetX);
                    var y0 = Map(py, 0, fractal.Height, -0.15 / zoom + offsetY, 0.15 / zoom + offsetY);
                    var x = 0.0;
                    var y = 0.0;
                    var xx = x * x;
                    var yy = y * y;
                    var iteration = 0;
                    while (xx + yy < 16 && iteration < max_iteration)
                    {
                        var xtemp = xx - yy + x0;
                        y = 2 * x * y + y0;
                        x = xtemp;
                        xx = x * x;
                        yy = y * y;
                        iteration++;
                    }
                    
                    if (xx + yy > 16)
                    {
                        fractal.SetPixel(px, py, getColor(iteration));
                    }
                    else
                    {
                        fractal.SetPixel(px, py, 0);
                    }
                }
            }
            frame.Bmp = fractal;
            workingFrames.Add(frame);
            
        }


        public int getColor(int i)
        {
            int a = (int)(255 * ((double)i) / (max_iteration / 4));
            return ((0) | (2 * a << 16) | (2 * a << 8) | (a << 0));
        }

        /// <summary>
        /// Clamp a value to 0-255
        /// </summary>

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (!rendering)
            {
                int width = 640, height = 480;
                if (x480ToolStripMenuItem.Checked)
                {
                    width = 640;
                    height = 480;
                }
                else if (x720ToolStripMenuItem.Checked) {
                    width = 900;
                    height = 720;
                }
                else if (x960ToolStripMenuItem.Checked) {
                    width = 1280;
                    height = 960;
                }
                currentFractal = new DirectBitmap(width, height);
                videoFile = new AviWriter("test.avi");
                videoFile.FramesPerSecond = 30;
                stream = videoFile.AddMotionJpegVideoStream(currentFractal.Width, currentFractal.Height, 90);
                rendering = true;
                for (var i = 0; i < 10; i++)
                {
                    threads.Add(Task.Run(new Action(MandelBrot)));
                }
                Manager = Task.Run(new Action(ThreadManager));
                startToolStripMenuItem.Text = "Stop";
            }
            else
            {
                rendering = false;
                MessageBox.Show("Finalizing... Please Wait.", "Finalizing Video", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Manager.Wait();
                foreach (Frame frame in workingFrames)
                {
                    frame.Bmp.Dispose();
                }
                workingFrames.RemoveRange(0, workingFrames.Count);
                videoFile.Close();
                startToolStripMenuItem.Text = "Start";
                MessageBox.Show("Rendering Completed!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void x480ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            x480ToolStripMenuItem.Checked = true;
            x720ToolStripMenuItem.Checked = false;
            x960ToolStripMenuItem.Checked = false;
            Width = 640;
            Height = 480;
        }

        private void x720ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            x480ToolStripMenuItem.Checked = false;
            x720ToolStripMenuItem.Checked = true;
            x960ToolStripMenuItem.Checked = false;
            Width = 900;
            Height = 720;
        }

        private void x960ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            x480ToolStripMenuItem.Checked = false;
            x720ToolStripMenuItem.Checked = false;
            x960ToolStripMenuItem.Checked = true;
            Width = 1280;
            Height = 960;
        }
    }

    public class Frame
    {
        public int frameNum;
        public DirectBitmap Bmp;
    }

    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppRgb, BitsHandle.AddrOfPinnedObject());
        }

        public void SetPixel(int x, int y, int color)
        {
            int index = x + (y * Width);
            //int col = colour.ToArgb();

            Bits[index] = color;
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}

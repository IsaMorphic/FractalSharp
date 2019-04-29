using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Mathematics;
using MandelbrotSharp.Rendering;
using MandelbrotSharp.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using MandelbrotSharp.Extras;
using System.Runtime.InteropServices;

namespace Mandelbrot
{
    public partial class Explorer : Form
    {
        private List<HistogramRenderSettings> UndoBuffer = new List<HistogramRenderSettings>();

        private int UndoIndex = 0;

        private int Iterations = 400;

        private bool ShouldRestartRender = true;
        private bool UseGPU = false;

        private bool MovingUp;
        private bool MovingDown;
        private bool MovingLeft;
        private bool MovingRight;

        private bool ZoomingIn;
        private bool ZoomingOut;

        private bool MousePressed = false;

        private Point MouseStart;
        private Point MouseEnd;

        private float DeltaX;
        private float DeltaY;

        private HistogramRenderSettings ExplorationSettings = new HistogramRenderSettings();
        private ExplorationRenderer ExplorationRenderer = new ExplorationRenderer();

        private DirectBitmap CurrentFrame;
        private bool firstFrameDone;

        private DateTime RenderStartTime;

        public Explorer(string palettePath, BigDecimal offsetX, BigDecimal offsetY, Type algorithm, Type numType)
        {
            ExplorationSettings.Palette = Utils.LoadPallete(palettePath);
            ExplorationSettings.offsetX = offsetX;
            ExplorationSettings.offsetY = offsetY;

            ExplorationSettings.TilesX = 8;
            ExplorationSettings.TilesY = 6;

            ExplorationSettings.MaxChunkSizes = Enumerable.Repeat(8, 48).ToArray();

            ExplorationSettings.AlgorithmType = algorithm;
            ExplorationSettings.ArithmeticType = numType;


            InitializeComponent();

            UpdateTimer.Start();
        }

        private void Explorer_KeyDown(object sender, KeyEventArgs e)
        {
            ExplorationSettings.MaxIterations = Iterations;
            switch (e.KeyCode)
            {
                case Keys.ShiftKey:
                    ZoomingIn = true;
                    break;
                case Keys.ControlKey:
                    ZoomingOut = true;
                    break;
                case Keys.Up:
                    UndoIndex = Math.Min(UndoIndex + 1, UndoBuffer.Count - 1);
                    ExplorationSettings = UndoBuffer[UndoIndex];
                    break;
                case Keys.Down:
                    UndoBuffer.Add(new HistogramRenderSettings
                    {
                        AlgorithmType = ExplorationSettings.AlgorithmType,
                        ArithmeticType = ExplorationSettings.ArithmeticType,
                        MaxChunkSizes = ExplorationSettings.MaxChunkSizes,
                        Magnification = ExplorationSettings.Magnification,
                        offsetX = ExplorationSettings.offsetX,
                        offsetY = ExplorationSettings.offsetY,
                        MaxIterations = ExplorationSettings.MaxIterations
                    });
                    UndoIndex = Math.Max(UndoIndex - 1, 0);
                    ExplorationSettings = UndoBuffer[UndoIndex];
                    break;
                case Keys.Oemplus:
                    ExplorationSettings.MaxIterations =
                        Iterations += 100;
                    break;
                case Keys.OemMinus:
                    ExplorationSettings.MaxIterations =
                        Iterations -= 100;
                    break;
                case Keys.Enter:
                    Task.Run((Action)RenderPhoto);
                    break;
            }
        }

        private void Explorer_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    MovingLeft = false;
                    break;
                case Keys.Right:
                    MovingRight = false;
                    break;
                case Keys.Up:
                    MovingUp = false;
                    break;
                case Keys.Down:
                    MovingDown = false;
                    break;
                case Keys.ShiftKey:
                    ZoomingIn = false;
                    break;
                case Keys.ControlKey:
                    ZoomingOut = false;
                    break;
                case Keys.Escape:
                    Close();
                    break;
            }
        }

        private void Explorer_Load(object sender, EventArgs e)
        {
            Bounds = Screen.PrimaryScreen.Bounds;

            //if (!ExplorationRenderer.GPUAvailable())
            //{
            //    MessageBox.Show("A CUDA supporting device is not present.  The exploration feature may be slow if you choose to continue.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    UseGPU = false;
            //}

            ExplorationSettings.Width = 1280;
            ExplorationSettings.Height = 720;

            DeltaX = Bounds.Width / (float)ExplorationSettings.Width;
            DeltaY = Bounds.Height / (float)ExplorationSettings.Height;

            ExplorationSettings.MaxIterations = Iterations;

            ExplorationSettings.ThreadCount = Environment.ProcessorCount - 1;

            ExplorationRenderer.FrameStarted += ExplorationRenderer_FrameStart;
            ExplorationRenderer.FrameFinished += ExplorationRenderer_FrameEnd;

            ExplorationRenderer.Initialize(
                ExplorationSettings);

            //if (UseGPU)
            //    ExplorationRenderer.InitGPU();

            CurrentFrame = new DirectBitmap(ExplorationSettings.Width, ExplorationSettings.Height);

            Task.Run((Action)NextFrame);
        }

        private void ExplorationRenderer_FrameStart(object sender, EventArgs e)
        {
            TimeSpan renderTime = DateTime.Now - RenderStartTime;

            BigDecimal stepAmount = .01 / ExplorationSettings.Magnification;
            if (MovingUp)
                ExplorationSettings.offsetY -= stepAmount;
            if (MovingDown)
                ExplorationSettings.offsetY += stepAmount;
            if (MovingLeft)
                ExplorationSettings.offsetX -= stepAmount;
            if (MovingRight)
                ExplorationSettings.offsetX += stepAmount;
            if (ZoomingIn)
                ExplorationSettings.Magnification *= 1.05;
            if (ZoomingOut)
                ExplorationSettings.Magnification /= 1.05;

            ExplorationRenderer.Update(ExplorationSettings);

            RenderStartTime = DateTime.Now;
        }

        private void ExplorationRenderer_FrameEnd(object sender, FrameEventArgs e)
        {
            firstFrameDone = true;
            CurrentFrame.SetBits(e.Frame.CopyDataAsBits());
            Task.Run((Action)NextFrame);
        }


        private void NextFrame()
        {
            //if (UseGPU)
            //    ExplorationRenderer.RenderFrameGPU();
            //else
            ExplorationRenderer.RenderFrame();
        }

        private void RenderPhoto()
        {
            HistogramRenderer PhotoRenderer = new HistogramRenderer();
            PhotoRenderer.FrameFinished += PhotoRenderer_FrameEnd;
            HistogramRenderSettings PhotoSettings = new HistogramRenderSettings();
            PhotoSettings.offsetX = ExplorationSettings.offsetX;
            PhotoSettings.offsetY = ExplorationSettings.offsetY;
            PhotoSettings.Magnification = ExplorationSettings.Magnification;
            PhotoSettings.MaxIterations = ExplorationSettings.MaxIterations;
            PhotoSettings.AlgorithmType = ExplorationSettings.AlgorithmType;
            PhotoSettings.ArithmeticType = ExplorationSettings.ArithmeticType;
            PhotoSettings.Width = 1920;
            PhotoSettings.Height = 1080;
            PhotoSettings.Palette = ExplorationSettings.Palette;
            PhotoRenderer.Initialize(PhotoSettings);
            if (UseGPU)
            {
                //PhotoRenderer.InitGPU();
                //PhotoRenderer.RenderFrameGPU();
                //PhotoRenderer.CleanupGPU();
            }
            else
            {
                PhotoRenderer.RenderFrame();
            }
        }

        private void PhotoRenderer_FrameEnd(object sender, FrameEventArgs e)
        {
            int count = 1;

            string fileNameOnly = DateTime.Now.ToShortDateString().Replace('/', '-');
            string extension = ".png";
            string path = "Photos";
            string newFullPath = Path.Combine(path, fileNameOnly + extension);

            while (File.Exists(newFullPath))
            {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }
            using (DirectBitmap bitmap = new DirectBitmap(e.Frame.Width, e.Frame.Height)) {
                bitmap.SetBits(e.Frame.CopyDataAsBits());
                bitmap.Bitmap.Save(newFullPath, ImageFormat.Png);
            }
        }

        public BigDecimal GetXOffset()
        {
            return ExplorationSettings.offsetX;
        }

        public BigDecimal GetYOffset()
        {
            return ExplorationSettings.offsetY;
        }

        private void Explorer_FormClosing(object sender, FormClosingEventArgs e)
        {
            UpdateTimer.Stop();
            Cursor.Show();
            ExplorationRenderer.StopRender();
            CurrentFrame.Dispose();
            pictureBox1.Image.Dispose();
            ShouldRestartRender = false;
        }

        private void Explorer_MouseMove(object sender, MouseEventArgs e)
        {
            if (MousePressed)
                MouseEnd = e.Location;
        }

        private void Explorer_MouseDown(object sender, MouseEventArgs e)
        {
            MouseEnd = MouseStart = e.Location;
            MousePressed = true;
        }

        private void Explorer_MouseUp(object sender, MouseEventArgs e)
        {
            UndoBuffer.Add(new HistogramRenderSettings
            {
                AlgorithmType = ExplorationSettings.AlgorithmType,
                ArithmeticType = ExplorationSettings.ArithmeticType,
                MaxChunkSizes = ExplorationSettings.MaxChunkSizes,
                Magnification = ExplorationSettings.Magnification,
                offsetX = ExplorationSettings.offsetX,
                offsetY = ExplorationSettings.offsetY,
                MaxIterations = ExplorationSettings.MaxIterations
            });
            UndoIndex = UndoBuffer.Count;
            int startX = (int)(MouseStart.X / DeltaX);
            int startY = (int)(MouseStart.Y / DeltaY);
            int endX = (int)(MouseEnd.X / DeltaX);
            int endY = (int)(MouseEnd.Y / DeltaY);

            int rectWidth = Math.Abs(startX - endX);
            int rectHeight = Math.Abs(startY - endY);

            int cornerX = (startX > endX) ? endX : startX;
            int cornerY = (startY > endY) ? endY : startY;

            ExplorationSettings.Magnification *= ExplorationSettings.Height / rectHeight;
            Point centerPoint = new Point(cornerX + rectWidth / 2, cornerY + rectHeight / 2);
            BigDecimal offsetX, offsetY;
            ExplorationRenderer.GetPointFromFrameLocation(
                centerPoint.X, centerPoint.Y,
                out offsetX,
                out offsetY);
            ExplorationSettings.offsetX = offsetX;
            ExplorationSettings.offsetY = offsetY;
            MousePressed = false;
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (!firstFrameDone)
                return;

            var bitmap = new Bitmap(CurrentFrame.Bitmap);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawString("real: " + ExplorationSettings.offsetX, SystemFonts.DefaultFont, Brushes.White, 0, 0);
                g.DrawString("imag: " + ExplorationSettings.offsetY, SystemFonts.DefaultFont, Brushes.White, 0, 10);
                g.DrawString("zoom: " + ExplorationSettings.Magnification, SystemFonts.DefaultFont, Brushes.White, 0, 20);
                g.DrawString("iter: " + ExplorationSettings.MaxIterations, SystemFonts.DefaultFont, Brushes.White, 0, 30);

                if (MousePressed)
                {
                    int startX = (int)(MouseStart.X / DeltaX);
                    int startY = (int)(MouseStart.Y / DeltaY);
                    int endX = (int)(MouseEnd.X / DeltaX);
                    int endY = (int)(MouseEnd.Y / DeltaY);

                    int cornerX = (startX > endX) ? endX : startX;
                    int cornerY = (startY > endY) ? endY : startY;

                    Rectangle SelectArea = new Rectangle(cornerX, cornerY, Math.Abs(startX - endX), Math.Abs(startY - endY));
                    g.DrawRectangle(Pens.White, SelectArea);
                }
            }
            var previousImage = pictureBox1.Image;
            pictureBox1.Image = bitmap;
            if (previousImage != null)
                previousImage.Dispose();
        }
    }
    class ExplorationRenderer : HistogramRenderer
    {
        public void GetPointFromFrameLocation(int x, int y, out BigDecimal offsetX, out BigDecimal offsetY)
        {
            offsetX = PointMapper.MapPointX(x);
            offsetY = PointMapper.MapPointY(y);
        }

        public void Update(RenderSettings settings)
        {

            bool hasChanged = (
                    offsetX != settings.offsetX ||
                    offsetY != settings.offsetY ||
                    Magnification != settings.Magnification ||
                    MaxIterations != settings.MaxIterations);

            offsetX = settings.offsetX;
            offsetY = settings.offsetY;
            Magnification = settings.Magnification;
            MaxIterations = settings.MaxIterations;

            if (hasChanged)
            {
                UpdateAlgorithmProvider();
                ResetChunkSizes();
            }
        }
    }
}

using Mandelbrot.Algorithms;
using Mandelbrot.Imaging;
using Mandelbrot.Mathematics;
using Mandelbrot.Rendering;
using Mandelbrot.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mandelbrot
{
    public partial class Explorer : Form
    {
        private int Iterations = 400;

        private bool ShouldRestartRender = true;
        private bool UseGPU = true;

        private bool MovingUp;
        private bool MovingDown;
        private bool MovingLeft;
        private bool MovingRight;

        private bool ZoomingIn;
        private bool ZoomingOut;

        private RenderSettings ExplorationSettings = new RenderSettings();
        private MandelbrotRenderer ExplorationRenderer = new MandelbrotRenderer();

        private RGB[] ColorPalette;

        private DirectBitmap directBitmap;

        private GenericMathResolver MathResolver =
            new GenericMathResolver(new Assembly[]
            { Assembly.GetExecutingAssembly() });

        private DateTime RenderStartTime;

        public Explorer(string palettePath, decimal offsetX, decimal offsetY)
        {
            ColorPalette = Utils.LoadPallete(palettePath);
            ExplorationSettings.offsetX = offsetX;
            ExplorationSettings.offsetY = offsetY;

            ExplorationSettings.Gradual = true;

            ExplorationSettings.MaxChunkSizes = new int[12]
            {
                8, 4, 4, 8,
                4, 2, 2, 4,
                8, 4, 4, 8,
            };

            InitializeComponent();
        }

        private void Explorer_KeyDown(object sender, KeyEventArgs e)
        {
            ExplorationSettings.MaxIterations = Iterations;
            switch (e.KeyCode)
            {
                case Keys.Left:
                    MovingLeft = true;
                    break;
                case Keys.Right:
                    MovingRight = true;
                    break;
                case Keys.Up:
                    MovingUp = true;
                    break;
                case Keys.Down:
                    MovingDown = true;
                    break;
                case Keys.ShiftKey:
                    ZoomingIn = true;
                    break;
                case Keys.ControlKey:
                    ZoomingOut = true;
                    break;
                case Keys.Oemplus:
                    ExplorationSettings.MaxIterations =
                        Iterations += 100;
                    break;
                case Keys.OemMinus:
                    ExplorationSettings.MaxIterations =
                        Iterations -= 100;
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

            if (!ExplorationRenderer.GPUAvailable())
            {
                MessageBox.Show("A CUDA supporting device is not present.  The exploration feature may be slow if you choose to continue.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ExplorationSettings.Width = 426;
                ExplorationSettings.Height = 240;
                UseGPU = false;
            }
            else
            {
                ExplorationSettings.Width = 640;
                ExplorationSettings.Height = 360;
            }

            directBitmap = new DirectBitmap(ExplorationSettings.Width, ExplorationSettings.Height);

            Cursor.Hide();

            ExplorationSettings.MaxIterations = Iterations;

            ExplorationSettings.ThreadCount = Environment.ProcessorCount - 1;

            ExplorationRenderer.FrameStart += ExplorationRenderer_FrameStart;
            ExplorationRenderer.FrameEnd += ExplorationRenderer_FrameEnd;
            ExplorationRenderer.RenderHalted += ExplorationRenderer_RenderHalted;

            ExplorationRenderer.Initialize(
                ExplorationSettings,
                ColorPalette,
                MathResolver);

            if (UseGPU)
                ExplorationRenderer.InitGPU();

            Task.Run((Action)NextFrame);
        }

        private void ExplorationRenderer_FrameStart()
        {
            TimeSpan renderTime = DateTime.Now - RenderStartTime;

            decimal stepAmount = .01M / (decimal)ExplorationSettings.Magnification;
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

            ExplorationRenderer.Setup(ExplorationSettings);

            RenderStartTime = DateTime.Now;
        }

        private void ExplorationRenderer_FrameEnd(Bitmap frame)
        {
            Bitmap newFrame = (Bitmap)frame.Clone();
            using (var g = Graphics.FromImage(newFrame))
            {
                g.DrawString("real: " + ExplorationSettings.offsetX, SystemFonts.DefaultFont, Brushes.White, 0, 0);
                g.DrawString("imag: " + ExplorationSettings.offsetY, SystemFonts.DefaultFont, Brushes.White, 0, 10);
                g.DrawString("zoom: " + ExplorationSettings.Magnification, SystemFonts.DefaultFont, Brushes.White, 0, 20);
                g.DrawString("iter: " + ExplorationSettings.MaxIterations, SystemFonts.DefaultFont, Brushes.White, 0, 30);

                g.DrawEllipse(Pens.White, new Rectangle(frame.Width / 2 - 10, frame.Height / 2 - 10, 20, 20));
                g.DrawEllipse(Pens.White, new Rectangle(frame.Width / 2 - 5, frame.Height / 2 - 5, 10, 10));
            }

            pictureBox1.Image = newFrame;

            Task.Run((Action)NextFrame);
        }

        private void ExplorationRenderer_RenderHalted()
        {
            if (ShouldRestartRender)
            {
                ExplorationSettings.Magnification /= 1.2;
            }
            if (UseGPU)
                ExplorationRenderer.CleanupGPU();
        }

        private void NextFrame()
        {
            if (UseGPU)
                ExplorationRenderer.RenderFrameGPU();
            else
                ExplorationRenderer.RenderFrame<double>();
        }

        public decimal GetXOffset()
        {
            return ExplorationSettings.offsetX;
        }

        public decimal GetYOffset()
        {
            return ExplorationSettings.offsetY;
        }

        private void Explorer_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cursor.Show();
            ShouldRestartRender = false;
            ExplorationRenderer.StopRender();
        }
    }
}

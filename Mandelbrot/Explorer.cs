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

        private bool MovingUp;
        private bool MovingDown;
        private bool MovingLeft;
        private bool MovingRight;

        private bool ZoomingIn;
        private bool ZoomingOut;

        private RenderSettings ExplorationSettings = new RenderSettings();
        private MandelbrotRenderer ExplorationRenderer = new MandelbrotRenderer();

        private RGB[] ColorPalette;

        private GenericMathResolver MathResolver =
            new GenericMathResolver(new Assembly[]
            { Assembly.GetExecutingAssembly() });

        private DateTime RenderStartTime;

        private List<GenericComplex<decimal>> PointList;

        private Font TextFont = new Font(new FontFamily("Arial"), 8);

        public Explorer(string palettePath, decimal offsetX, decimal offsetY)
        {
            ColorPalette = Utils.LoadPallete(palettePath);
            ExplorationSettings.offsetX = offsetX;
            ExplorationSettings.offsetY = offsetY;

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
            Width = 700;
            Height = 500;

            ExplorationSettings.Width = 350;
            ExplorationSettings.Height = 250;

            ExplorationSettings.MaxIterations = Iterations;

            ExplorationSettings.ThreadCount = Environment.ProcessorCount - 1;

            ExplorationRenderer.FrameStart += ExplorationRenderer_FrameStart;
            ExplorationRenderer.FrameEnd += ExplorationRenderer_FrameEnd;
            ExplorationRenderer.RenderHalted += ExplorationRenderer_RenderHalted;

            ExplorationRenderer.Initialize(
                ExplorationSettings,
                ColorPalette,
                MathResolver);

            ExplorationRenderer.InitGPU();

            timer1.Start();
        }

        private void ExplorationRenderer_FrameStart()
        {
            TimeSpan renderTime = DateTime.Now - RenderStartTime;

            decimal stepAmount = .03M / (decimal)ExplorationSettings.Magnification;
            if (MovingUp)
                ExplorationSettings.offsetY -= stepAmount;
            if (MovingDown)
                ExplorationSettings.offsetY += stepAmount;
            if (MovingLeft)
                ExplorationSettings.offsetX -= stepAmount;
            if (MovingRight)
                ExplorationSettings.offsetX += stepAmount;
            if (ZoomingIn)
                ExplorationSettings.Magnification *= 1.2;
            if (ZoomingOut)
                ExplorationSettings.Magnification /= 1.2;

            ExplorationRenderer.Setup(ExplorationSettings);

            RenderStartTime = DateTime.Now;
        }

        private void ExplorationRenderer_FrameEnd(Bitmap frame)
        {
            using (var g = Graphics.FromImage(frame))
            {
                g.DrawString("real: " + ExplorationSettings.offsetX, SystemFonts.DefaultFont, Brushes.White, 0, 0);
                g.DrawString("imag: " + ExplorationSettings.offsetY, SystemFonts.DefaultFont, Brushes.White, 0, 10);
                g.DrawString("zoom: " + ExplorationSettings.Magnification, SystemFonts.DefaultFont, Brushes.White, 0, 20);
                g.DrawString("iter: " + ExplorationSettings.MaxIterations, SystemFonts.DefaultFont, Brushes.White, 0, 30);

                g.DrawEllipse(Pens.White, new Rectangle(frame.Width / 2 - 10, frame.Height / 2 - 10, 20, 20));
                g.DrawEllipse(Pens.White, new Rectangle(frame.Width / 2 - 5, frame.Height / 2 - 5, 10, 10));
            }
            pictureBox1.Image = frame;
        }

        private void ExplorationRenderer_RenderHalted()
        {
            if (ShouldRestartRender)
            {
                ExplorationSettings.Magnification /= 1.2;
            }
        }

        private void NextFrame()
        {
            ExplorationRenderer.RenderFrameGPU();
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
            ShouldRestartRender = false;
            ExplorationRenderer.StopRender();
            timer1.Stop();
            ExplorationRenderer.CleanupGPU();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            NextFrame();
        }
    }
}

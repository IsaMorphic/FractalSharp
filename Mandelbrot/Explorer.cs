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
        private int LowIterations = 400;
        private int HighIterations = 6000;

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

        private PerturbationAlgorithmProvider<decimal> PointChecker =
            new PerturbationAlgorithmProvider<decimal>();

        private Pen CrosshairColor = Pens.White;

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
            CrosshairColor = Pens.White;
            ExplorationSettings.MaxIterations = LowIterations;
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
                        LowIterations += 100;
                    break;
                case Keys.OemMinus:
                    ExplorationSettings.MaxIterations = 
                        LowIterations -= 100;
                    break;
                case Keys.Enter:
                    PointChecker.Init(
                        new DecimalMath(),
                        ExplorationSettings.offsetX,
                        ExplorationSettings.offsetY,
                        1000000);
                    if (PointChecker.GetSurroundingPoints().Count < HighIterations)
                        CrosshairColor = Pens.Red;
                    else
                    {
                        CrosshairColor = Pens.Green;
                        ExplorationSettings.MaxIterations = HighIterations;
                    }
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

            ExplorationSettings.Width = 262;
            ExplorationSettings.Height = 187;

            ExplorationSettings.MaxIterations = LowIterations;

            ExplorationSettings.ThreadCount = Environment.ProcessorCount - 1;

            ExplorationRenderer.FrameStart += ExplorationRenderer_FrameStart;
            ExplorationRenderer.FrameEnd += ExplorationRenderer_FrameEnd;
            ExplorationRenderer.RenderHalted += ExplorationRenderer_RenderHalted;

            ExplorationRenderer.Initialize(
                ExplorationSettings,
                ColorPalette,
                MathResolver);

            Task.Run((Action)ExplorationRenderer.RenderFrame<float>);
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
                g.DrawString("real: " + ExplorationSettings.offsetX, TextFont, Brushes.White, 0, 0);
                g.DrawString("imag: " + ExplorationSettings.offsetY, TextFont, Brushes.White, 0, 10);
                g.DrawString("zoom: " + ExplorationSettings.Magnification, TextFont, Brushes.White, 0, 20);
                g.DrawString("iter: " + ExplorationSettings.MaxIterations, TextFont, Brushes.White, 0, 30);

                g.DrawEllipse(CrosshairColor, new Rectangle(frame.Width / 2 - 10, frame.Height / 2 - 10, 20, 20));
                g.DrawEllipse(CrosshairColor, new Rectangle(frame.Width / 2 - 5, frame.Height / 2 - 5, 10, 10));
            }
            pictureBox1.Image = frame;
            Thread.Sleep(1000 / 30);

            NextFrame();
        }

        private void ExplorationRenderer_RenderHalted()
        {
            if (ShouldRestartRender)
            {
                ExplorationSettings.Magnification /= 1.2;
                NextFrame();
            }
        }

        private void NextFrame()
        {
            if (ExplorationSettings.Magnification < 81140)
                ExplorationRenderer.RenderFrame<float>();
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
            ShouldRestartRender = false;
                    ExplorationRenderer.StopRender();
        }
    }
}

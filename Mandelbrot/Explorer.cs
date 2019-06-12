/*
 *  Copyright 2018-2019 Chosen Few Software
 *  This file is part of an example application that demostrates 
 *  the usage of the MandelbrotSharp library.
 *
 *  MandelbrotSharp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  MandelbrotSharp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with MandelbrotSharp.  If not, see <https://www.gnu.org/licenses/>.
 */
using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Numerics;
using MandelbrotSharp.Rendering;
using MandelbrotSharp.Utilities;
using MiscUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mandelbrot
{
    public partial class Explorer : Form
    {
        private const int RenderWidth = 1280;
        private const int RenderHeight = 720;
        private List<SuccessiveRenderSettings> UndoBuffer = new List<SuccessiveRenderSettings>();

        private int UndoIndex = 0;

        private int Iterations = 400;

        private bool MovingUp;
        private bool MovingDown;
        private bool MovingLeft;
        private bool MovingRight;

        private bool ZoomingIn;
        private bool ZoomingOut;

        private bool MousePressed;

        private Point MouseStart;
        private Point MouseEnd;

        private float DeltaX;
        private float DeltaY;

        private SuccessiveRenderSettings ExplorationSettings = new SuccessiveRenderSettings();
        private ExplorationRenderer ExplorationRenderer = new ExplorationRenderer(RenderWidth, RenderHeight);

        private DirectBitmap CurrentFrame;
        private bool firstFrameDone;
        private bool ShouldUpdateRenderer;

        private DateTime RenderStartTime;

        public Explorer(string palettePath, BigDecimal offsetX, BigDecimal offsetY, Type algorithm, Type numType)
        {
            ExplorationSettings.OuterColors = Utils.LoadPallete(palettePath);
            ExplorationSettings.offsetX = offsetX;
            ExplorationSettings.offsetY = offsetY;

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
                    UndoBuffer.Add(new SuccessiveRenderSettings
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
            ShouldUpdateRenderer = true;
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

            DeltaX = Bounds.Width / (float)RenderWidth;
            DeltaY = Bounds.Height / (float)RenderHeight;

            ExplorationSettings.TilesX = 4;
            ExplorationSettings.TilesY = 3;

            ExplorationSettings.MaxChunkSizes = Enumerable.Repeat(8, 12).ToArray();

            ExplorationSettings.MaxIterations = Iterations;

            ExplorationSettings.ThreadCount = Environment.ProcessorCount - 1;

            ExplorationRenderer.FrameStarted += ExplorationRenderer_FrameStart;
            ExplorationRenderer.FrameFinished += ExplorationRenderer_FrameEnd;

            ExplorationRenderer.Setup(ExplorationSettings);

            CurrentFrame = new DirectBitmap(RenderWidth, RenderHeight);

            NextFrame();
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

            RenderStartTime = DateTime.Now;
        }

        private void ExplorationRenderer_FrameEnd(object sender, FrameEventArgs e)
        {
            firstFrameDone = true;
            CurrentFrame.SetBits(e.Frame.CopyDataAsBits());
            if (ShouldUpdateRenderer)
            {
                ExplorationRenderer.Update(ExplorationSettings);
                ShouldUpdateRenderer = false;
            }
            NextFrame();
        }


        private void NextFrame()
        {
            ExplorationRenderer.StartRenderFrame();
        }

        private void RenderPhoto()
        {
            MandelbrotRenderer PhotoRenderer = new MandelbrotRenderer(1920, 1080);
            PhotoRenderer.FrameFinished += PhotoRenderer_FrameEnd;
            RenderSettings PhotoSettings = new RenderSettings();
            PhotoSettings.offsetX = ExplorationSettings.offsetX;
            PhotoSettings.offsetY = ExplorationSettings.offsetY;
            PhotoSettings.Magnification = ExplorationSettings.Magnification;
            PhotoSettings.MaxIterations = ExplorationSettings.MaxIterations;
            PhotoSettings.AlgorithmType = ExplorationSettings.AlgorithmType;
            PhotoSettings.ArithmeticType = ExplorationSettings.ArithmeticType;
            PhotoSettings.OuterColors = ExplorationSettings.OuterColors;
            PhotoRenderer.Setup(PhotoSettings);

            PhotoRenderer.StartRenderFrame();
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
            using (DirectBitmap bitmap = new DirectBitmap(e.Frame.Width, e.Frame.Height))
            {
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
            ExplorationRenderer.StopRenderFrame();
            CurrentFrame.Dispose();
            pictureBox1.Image.Dispose();
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
            UndoBuffer.Add(new SuccessiveRenderSettings
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

            ExplorationSettings.Magnification *= RenderHeight / rectHeight;
            Point centerPoint = new Point(cornerX + rectWidth / 2, cornerY + rectHeight / 2);
            BigDecimal offsetX, offsetY;
            ExplorationRenderer.GetPointFromFrameLocation(
                centerPoint.X, centerPoint.Y,
                out offsetX,
                out offsetY);
            ExplorationSettings.offsetX = offsetX;
            ExplorationSettings.offsetY = offsetY;
            if (ExplorationRenderer.RenderStatus == TaskStatus.Canceled)
            {
                ExplorationRenderer.Update(ExplorationSettings);
                NextFrame();
            }
            else
            {
                ShouldUpdateRenderer = true;
            }
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
    class ExplorationRenderer : SuccessiveRenderer
    {
        public ExplorationRenderer(int width, int height) : base(width, height)
        {
        }

        public void GetPointFromFrameLocation(int x, int y, out BigDecimal offsetX, out BigDecimal offsetY)
        {
            offsetX = PointMapper.MapPointX(x).As<BigDecimal>();
            offsetY = PointMapper.MapPointY(y).As<BigDecimal>();
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

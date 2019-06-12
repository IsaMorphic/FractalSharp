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
using Mandelbrot.Movies;
using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Imaging;
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
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Mandelbrot
{
    public partial class FractalApp : Form
    {
        private Type traditionalAlgorithm =
            typeof(TraditionalAlgorithmProvider<>);

        private Type perturbationAlgorithm =
            typeof(PerturbationAlgorithmProvider<>);

        // Process statistics
        private DateTime currentFrameStartTime;
        private int coreCount = Environment.ProcessorCount;

        // Rendering related Properties
        private bool Rendering = false;
        private bool PrecisionSwitched = false;
        private double ExtraPrecisionThreshold = Math.Pow(500, 5);

        private DirectBitmap CurrentFrame;

        private Type PreferredAlgorithm =
            typeof(TraditionalAlgorithmProvider<>);

        private MandelbrotMovieRenderer Renderer;
        private ZoomMovieSettings RenderSettings = new ZoomMovieSettings();

        // Fractal loading and saving properties.  
        private bool RenderHasBeenStarted = false;
        private bool LoadedFile = false;
        private bool SequenceOpen = false;

        private string VideoPath;
        private string SettingsPath;
        private string PalletePath = Path.Combine(
            Application.StartupPath,
            "Palettes", "blues.map");

        public FractalApp()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Directory.CreateDirectory("Renders");
            Directory.CreateDirectory("Photos");

            startFrameInput.Value = RenderSettings.NumFrames;
            iterationCountInput.Value = RenderSettings.MaxIterations;

            xOffInput.Text = RenderSettings.offsetX.ToString();
            yOffInput.Text = RenderSettings.offsetY.ToString();

            threadCountInput.Value = RenderSettings.ThreadCount / 2;
            threadCountInput.Maximum = RenderSettings.ThreadCount - 1;

            RenderSettings.PalettePath = PalletePath;

            Width = RenderSettings.Width = 960;
            Height = RenderSettings.Height = 540;
        }

        private void InitializeRenderer()
        {
            Renderer = new MandelbrotMovieRenderer(RenderSettings.Width, RenderSettings.Height);
            Renderer.FrameStarted += FrameStart;
            Renderer.FrameFinished += FrameEnd;
        }

        #region Renderer Events
        private void FrameStart(object sender, EventArgs e)
        {
            currentFrameStartTime = DateTime.Now;

            BeginInvoke((Action)(() =>
            {
                startFrameInput.Value = Renderer.NumFrames;
                iterationCountInput.Value = Renderer.MaxIterations;
            }));

            // Calculate zoom... using math.pow to keep the zoom rate constant.
            if (Renderer.Magnification > ExtraPrecisionThreshold &&
                RenderSettings.AlgorithmType != perturbationAlgorithm)
            {
                RenderSettings.ArithmeticType = typeof(BigBinary);
                PrecisionSwitched = true;
            }
        }

        private void FrameEnd(object sender, FrameEventArgs e)
        {
            try
            {
                CurrentFrame.SetBits(e.Frame.CopyDataAsBits());
                CurrentFrame.Bitmap.Save(String.Format(VideoPath, Renderer.NumFrames), ImageFormat.Png);
                if (livePreviewCheckBox.Checked)
                {
                    var previousImage = pictureBox1.Image;
                    pictureBox1.Image = new Bitmap(CurrentFrame.Bitmap);
                    if (previousImage != null)
                        previousImage.Dispose();
                }
            }
            catch (NullReferenceException) { };

            Renderer.SetFrame(Renderer.NumFrames + 1);

            Renderer.StartRenderFrame();
        }

        public void Shutdown()
        {
            Rendering = false;
            BeginInvoke((Action)(() =>
            {
                intervalTimer.Stop();
                timeLabel.Text = "00:00:00.000";
                startToolStripMenuItem.Text = "Start";
                presicionStripMenuItem.Enabled = true;
                algorithmToolStripMenuItem.Enabled = true;
                stopToolStripMenuItem.Enabled = false;
                startToolStripMenuItem.Enabled = true;
                closeCurrentToolStripMenuItem.Enabled = true;
                tableLayoutPanel1.Enabled = true;
                threadCountInput.Enabled = true;
                coreCountLabel.Enabled = true;
                pictureBox1.Image = null;
                CurrentFrame.Dispose();
                if (PrecisionSwitched)
                {
                    standardPrecisionToolStripMenuItem.Checked = false;
                    extraPrescisionToolStripMenuItem.Checked = true;
                    RenderSettings.ArithmeticType = typeof(BigDecimal);
                }
            }));
        }

        #endregion

        #region Fractal Configuration Methods

        private bool LoadFractal()
        {
            bool success = false;
            FileLoadDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Renders");
            if (FileLoadDialog.ShowDialog() == DialogResult.OK)
            {
                SettingsPath = FileLoadDialog.FileName;
                string jsonData = File.ReadAllText(SettingsPath);

                JavaScriptSerializer js = new JavaScriptSerializer();
                js.RegisterConverters(new List<JavaScriptConverter>() { new BigDecimalConverter() });
                RenderSettings = js.Deserialize<ZoomMovieSettings>(jsonData);

                VideoPath = RenderSettings.VideoPath;

                x540ToolStripMenuItem.Checked = false;
                x720ToolStripMenuItem.Checked = false;
                x1080ToolStripMenuItem.Checked = false;

                Width = RenderSettings.Width;
                Height = RenderSettings.Height;

                if (RenderSettings.Height == 540)
                {
                    x540ToolStripMenuItem.Checked = true;
                }
                else if (RenderSettings.Height == 720)
                {
                    x720ToolStripMenuItem.Checked = true;
                }
                else if (RenderSettings.Height == 1080)
                {
                    x1080ToolStripMenuItem.Checked = true;
                }

                if (RenderSettings.ExtraPrecision)
                {
                    standardPrecisionToolStripMenuItem.Checked = false;
                    extraPrescisionToolStripMenuItem.Checked = true;
                    RenderSettings.ArithmeticType = typeof(BigDecimal);
                }
                else
                {
                    standardPrecisionToolStripMenuItem.Checked = true;
                    extraPrescisionToolStripMenuItem.Checked = false;
                    RenderSettings.ArithmeticType = typeof(double);
                }


                startFrameInput.Value = RenderSettings.NumFrames;
                iterationCountInput.Value = RenderSettings.MaxIterations;

                threadCountInput.Value = RenderSettings.ThreadCount;

                xOffInput.Text = RenderSettings.offsetX.ToString();
                yOffInput.Text = RenderSettings.offsetY.ToString();

                success = true;
            }
            return success;
        }

        private void SaveFractal()
        {
            string fractalName = Path.GetFileNameWithoutExtension(VideoPath).Replace("_{0}", String.Empty);
            string fractalDate = DateTime.Now.ToShortDateString().Replace('/', '-');
            string fileName = String.Format("{0}_{1}.fractal", fractalName, fractalDate);
            string filePath = Path.Combine(Application.StartupPath, "Renders", fileName);

            SettingsPath = filePath;

            JavaScriptSerializer js = new JavaScriptSerializer();
            js.RegisterConverters(new List<JavaScriptConverter>() { new BigDecimalConverter() });
            string jsonData = js.Serialize(RenderSettings);

            File.WriteAllText(filePath, jsonData);
        }

        private void UpdateFractal()
        {
            string jsonData = File.ReadAllText(SettingsPath);

            JavaScriptSerializer js = new JavaScriptSerializer();
            js.RegisterConverters(new List<JavaScriptConverter>() { new BigDecimalConverter() });
            RenderSettings.NumFrames = Renderer.NumFrames;

            RenderSettings.Version++;

            jsonData = js.Serialize(RenderSettings);

            File.WriteAllText(SettingsPath, jsonData);
        }

        #endregion

        #region UI Related Tasks

        private void newRenderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenderSettings.Version = 0;
            RenderSaveDialog.ShowDialog();
        }

        private void RenderSaveDialog_OK(object sender, CancelEventArgs e)
        {
            RenderSettings.OuterColors = Utils.LoadPallete(RenderSettings.PalettePath);

            InitializeRenderer();

            VideoPath = RenderSaveDialog.FileName;

            string videoDirectory = Path.GetDirectoryName(VideoPath);
            string videoName = Path.GetFileNameWithoutExtension(VideoPath);

            if (!videoName.EndsWith("_{0}.png"))
                videoName += "_{0}.png";

            VideoPath = Path.Combine(videoDirectory, videoName);

            RenderSettings.VideoPath = VideoPath;

            SequenceOpen = true;

            newRenderToolStripMenuItem.Enabled = false;
            loadRenderToolStripMenuItem.Enabled = false;
            ResolutionToolStripMenuItem.Enabled = false;
            closeCurrentToolStripMenuItem.Enabled = true;
            startToolStripMenuItem.Enabled = true;
        }

        private void loadRenderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (LoadFractal())
            {
                LoadedFile = true;

                SequenceOpen = true;

                newRenderToolStripMenuItem.Enabled = false;
                loadRenderToolStripMenuItem.Enabled = false;
                ResolutionToolStripMenuItem.Enabled = false;
                loadPaletteToolStripMenuItem.Enabled = false;
                closeCurrentToolStripMenuItem.Enabled = true;
                startToolStripMenuItem.Enabled = true;
            }
        }

        private void closeCurrentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startToolStripMenuItem.Enabled = false;
            ResolutionToolStripMenuItem.Enabled = true;
            closeCurrentToolStripMenuItem.Enabled = false;
            newRenderToolStripMenuItem.Enabled = true;
            loadRenderToolStripMenuItem.Enabled = true;
            ResolutionToolStripMenuItem.Enabled = true;
            loadPaletteToolStripMenuItem.Enabled = true;

            SequenceOpen = false;

            UpdateFractal();
        }

        private void loadPaletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileLoadDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Palettes");
            if (FileLoadDialog.ShowDialog() == DialogResult.OK)
            {
                PalletePath = FileLoadDialog.FileName;
            }
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Rendering)
            {
                CurrentFrame = new DirectBitmap(RenderSettings.Width, RenderSettings.Height);

                Rendering = true;
                RenderSettings.offsetX = BigDecimal.Parse(xOffInput.Text);
                RenderSettings.offsetY = BigDecimal.Parse(yOffInput.Text);

                RenderSettings.MaxIterations = (int)iterationCountInput.Value;
                RenderSettings.NumFrames = (int)startFrameInput.Value;
                RenderSettings.ThreadCount = (int)threadCountInput.Value;

                if (!LoadedFile)
                {
                    Renderer.Setup(RenderSettings);
                    if (!RenderHasBeenStarted)
                    {
                        Renderer.StartRenderFrame();
                        RenderHasBeenStarted = true;
                        SaveFractal();
                    }
                }
                else
                {
                    RenderSettings.OuterColors = Utils.LoadPallete(RenderSettings.PalettePath);
                    InitializeRenderer();
                    Renderer.Setup(RenderSettings);
                    Renderer.StartRenderFrame();
                }

                intervalTimer.Start();
                algorithmToolStripMenuItem.Enabled = false;
                presicionStripMenuItem.Enabled = false;
                stopToolStripMenuItem.Enabled = true;
                startToolStripMenuItem.Enabled = false;
                closeCurrentToolStripMenuItem.Enabled = false;
                loadPaletteToolStripMenuItem.Enabled = false;
                tableLayoutPanel1.Enabled = false;
                threadCountInput.Enabled = false;
                coreCountLabel.Enabled = false;
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Renderer.StopRenderFrame();
            Shutdown();
        }

        // Configuration Events

        private void standardPrecisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            standardPrecisionToolStripMenuItem.Checked = true;
            extraPrescisionToolStripMenuItem.Checked = false;
            RenderSettings.ArithmeticType = typeof(double);
            RenderSettings.ExtraPrecision = false;
        }

        private void extraPrescisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            standardPrecisionToolStripMenuItem.Checked = false;
            extraPrescisionToolStripMenuItem.Checked = true;
            RenderSettings.ArithmeticType = typeof(BigDecimal);
            RenderSettings.ExtraPrecision = true;
        }

        private void traditionalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            perturbationToolStripMenuItem.Checked = false;
            RenderSettings.AlgorithmType = traditionalAlgorithm;
        }

        private void perturbationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            traditionalToolStripMenuItem.Checked = false;
            RenderSettings.AlgorithmType = perturbationAlgorithm;
        }

        private void x540ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            x540ToolStripMenuItem.Checked = true;
            x720ToolStripMenuItem.Checked = false;
            x1080ToolStripMenuItem.Checked = false;
            RenderSettings.Width = Width = 960;
            RenderSettings.Height = Height = 540;
        }

        private void x720ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            x540ToolStripMenuItem.Checked = false;
            x720ToolStripMenuItem.Checked = true;
            x1080ToolStripMenuItem.Checked = false;
            RenderSettings.Width = Width = 1280;
            RenderSettings.Height = Height = 720;
        }

        private void x1080ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            x540ToolStripMenuItem.Checked = false;
            x720ToolStripMenuItem.Checked = false;
            x1080ToolStripMenuItem.Checked = true;
            RenderSettings.Width = Width = 1920;
            RenderSettings.Height = Height = 1080;
        }

        private void exploreFractalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var exploreWindow = new Explorer(
                PalletePath,
                BigDecimal.Parse(xOffInput.Text),
                BigDecimal.Parse(yOffInput.Text),
                RenderSettings.AlgorithmType,
                RenderSettings.ArithmeticType);
            exploreWindow.ShowDialog();

            if (!Rendering)
            {
                xOffInput.Text = exploreWindow.GetXOffset().ToString();
                yOffInput.Text = exploreWindow.GetYOffset().ToString();
            }
        }

        private void FractalApp_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SequenceOpen)
            {
                e.Cancel = true;
                TrayIcon.Visible = true;
                Hide();
            }
        }

        private void intervalTimer_Tick(object sender, EventArgs e)
        {
            timeLabel.Text = (DateTime.Now - currentFrameStartTime).ToString(@"hh\:mm\:ss\.fff");
        }

        private void livePreviewCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
        }

        private void TrayIcon_Click(object sender, EventArgs e)
        {
            TrayIcon.Visible = false;
            Show();
        }
        #endregion
    }
}

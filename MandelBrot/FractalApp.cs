using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

using Mandelbrot.Utilities;
using Mandelbrot.Rendering;
using Mandelbrot.Rendering.Imaging;

using Accord.Video.FFMPEG;

namespace Mandelbrot
{
    public partial class FractalApp : Form
    {
        // Video file properties
        private VideoFileReader videoReader = new VideoFileReader();
        private VideoFileWriter videoWriter = new VideoFileWriter();

        // Process statistics
        private DateTime currentFrameStartTime;
        private int coreCount = Environment.ProcessorCount;

        // Rendering related Properties
        private bool rendering = false;
        private bool extraPrecision = false;
        private double extraPrecisionThreshold = Math.Pow(500, 5);

        private MandelbrotRenderer Renderer = new MandelbrotRenderer();
        private FractalAppSettings RenderSettings = new FractalAppSettings();

        private Action ChosenMethod;

        // Fractal loading and saving properties.  
        private bool renderActive = false;
        private bool loadingFile = false;

        private string videoPath;
        private string settingsPath;
        private string palletePath = Path.Combine(
            Application.StartupPath,
            "Palettes", "blues.map");

        public FractalApp()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ChosenMethod = new Action(Renderer.RenderFrame<Double, DoubleMath>);

            startFrameInput.Value = RenderSettings.NumFrames;
            iterationCountInput.Value = RenderSettings.MaxIterations;

            xOffInput.Value = RenderSettings.offsetX;
            yOffInput.Value = RenderSettings.offsetY;

            threadCountInput.Value = RenderSettings.ThreadCount / 2;
            threadCountInput.Maximum = RenderSettings.ThreadCount - 1;

            Renderer.FrameStart += FrameStart;
            Renderer.FrameEnd += FrameEnd;

            Renderer.RenderHalted += Shutdown;

            RenderSettings.palettePath = palletePath;

            Width = 640;
            Height = 480;
        }

        #region Frame Setup and Cleanup Methods
        private void FrameStart()
        {
            currentFrameStartTime = DateTime.Now;

            BeginInvoke((Action)(() =>
            {
                startFrameInput.Value = Renderer.NumFrames;
                iterationCountInput.Value = Renderer.MaxIterations;
            }));

            // Calculate zoom... using math.pow to keep the zoom rate constant.
            if (Renderer.Magnification > extraPrecisionThreshold)
            {
                ChosenMethod = new Action(Renderer.RenderFrame<Quadruple, QuadrupleMath>);
                extraPrecision = true;
            }
        }

        private void FrameEnd(Bitmap frame)
        {
            try
            {
                videoWriter.WriteVideoFrame(frame);
                if (livePreviewCheckBox.Checked)
                {
                    pictureBox1.Image = frame;
                }
            }
            catch (NullReferenceException) { };

            Renderer.SetFrame(Renderer.NumFrames + 1);

            Task.Run(ChosenMethod);
        }

        public void Shutdown()
        {
            rendering = false;
            BeginInvoke((Action)(() =>
            {
                intervalTimer.Stop();
                timeLabel.Text = "00:00:00.000";
                startToolStripMenuItem.Text = "Start";
                presicionStripMenuItem.Enabled = true;
                stopToolStripMenuItem.Enabled = false;
                startToolStripMenuItem.Enabled = true;
                closeCurrentToolStripMenuItem.Enabled = true;
                tableLayoutPanel1.Enabled = true;
                threadCountInput.Enabled = true;
                coreCountLabel.Enabled = true;
                pictureBox1.Image = null;
                if (extraPrecision)
                {
                    standardPrecisionToolStripMenuItem.Checked = false;
                    extraPrescisionToolStripMenuItem.Checked = true;
                    ChosenMethod = new Action(Renderer.RenderFrame<Quadruple, QuadrupleMath>);
                }
            }));
        }

        #endregion

        #region Main-task Render Methods

        // Simple method that reads a frame 
        // from an old video file and saves it to a new one.  
        private void GrabFrame()
        {
            Renderer.SetFrame(Renderer.NumFrames + 1);

            if (Renderer.NumFrames < videoReader.FrameCount - 1)
            {
                Bitmap frame = videoReader.ReadVideoFrame();
                videoWriter.WriteVideoFrame(frame);
                Task.Run(new Action(GrabFrame));
            }
            else
            {
                loadingFile = false;
                videoReader.Close();
                Task.Run(ChosenMethod);
            }
        }

        #endregion

        #region Fractal Configuration Methods

        private bool LoadFractal()
        {
            bool loaded = false;
            fileLoadDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Renders");
            if (fileLoadDialog.ShowDialog() == DialogResult.OK)
            {
                settingsPath = fileLoadDialog.FileName;
                string jsonData = File.ReadAllText(settingsPath);

                JavaScriptSerializer js = new JavaScriptSerializer();
                RenderSettings = js.Deserialize<FractalAppSettings>(jsonData);

                videoPath = RenderSettings.videoPath;

                x480ToolStripMenuItem.Checked = false;
                x720ToolStripMenuItem.Checked = false;
                x960ToolStripMenuItem.Checked = false;

                Width = RenderSettings.Width;
                Height = RenderSettings.Height;

                if (RenderSettings.Height == 480)
                {
                    x480ToolStripMenuItem.Checked = true;
                }
                else if (RenderSettings.Height == 720)
                {
                    x720ToolStripMenuItem.Checked = true;
                }
                else if (RenderSettings.Height == 960)
                {
                    x960ToolStripMenuItem.Checked = true;
                }

                startFrameInput.Value = RenderSettings.NumFrames;
                iterationCountInput.Value = RenderSettings.MaxIterations;

                xOffInput.Value = RenderSettings.offsetX;
                yOffInput.Value = RenderSettings.offsetY;

                int bitrate = RenderSettings.Width * RenderSettings.Height * 32 * 3 * 8; // 80 percent quality, explained below
                videoReader.Open(String.Format(videoPath, RenderSettings.version));
                videoWriter.Open(String.Format(videoPath, RenderSettings.version + 1), RenderSettings.Width, RenderSettings.Height, 30, VideoCodec.MPEG4, bitrate);

                loaded = true;
            }
            return loaded;
        }

        private void SaveFractal()
        {
            string fractalName = Path.GetFileNameWithoutExtension(videoPath).Replace("_{0}", String.Empty);
            string fractalDate = DateTime.Now.ToShortDateString().Replace('/', '-');
            string fileName = String.Format("{0}_{1}.fractal", fractalName, fractalDate);
            string filePath = Path.Combine(Application.StartupPath, "Renders", fileName);

            JavaScriptSerializer js = new JavaScriptSerializer();
            string jsonData = js.Serialize(RenderSettings);

            File.WriteAllText(filePath, jsonData);
        }

        private void UpdateFractal()
        {
            string jsonData = File.ReadAllText(settingsPath);

            JavaScriptSerializer js = new JavaScriptSerializer();
            RenderSettings = js.Deserialize<FractalAppSettings>(jsonData);

            RenderSettings.version++;

            jsonData = js.Serialize(RenderSettings);

            File.WriteAllText(settingsPath, jsonData);
        }

        #endregion

        #region UI Related Tasks

        private void newRenderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenderSettings.version = 0;
            RenderSaveDialog.ShowDialog();
        }

        private void RenderSaveDialog_OK(object sender, CancelEventArgs e)
        {
            RGB[] palette = Utils.LoadPallete(RenderSettings.palettePath);

            Renderer.Initialize(RenderSettings, palette);

            videoPath = RenderSaveDialog.FileName;

            string videoDirectory = Path.GetDirectoryName(videoPath);
            string videoName = Path.GetFileNameWithoutExtension(videoPath);

            if (!videoName.EndsWith("_{0}.avi"))
                videoName += "_{0}.avi";

            videoPath = Path.Combine(videoDirectory, videoName);

            RenderSettings.videoPath = videoPath;

            // width and height multiplied by 32, 32 bpp
            // then multiply by framerate divided by ten, after multiplying by eight yeilds 80% of the normal amount of bits per second.  
            // Note: we're assuming that there is no audio in the video.  Otherwise we would have to accomidate for that as well.  
            int bitrate = RenderSettings.Width * RenderSettings.Height * 32 * 3 * 8; // 80 percent quality
            videoWriter.Open(String.Format(videoPath, RenderSettings.version), RenderSettings.Width, RenderSettings.Height, 30, VideoCodec.MPEG4, bitrate);

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
                loadingFile = true;

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

            videoWriter.Close();
        }

        private void loadPaletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileLoadDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Palettes");
            if (fileLoadDialog.ShowDialog() == DialogResult.OK)
            {
                palletePath = fileLoadDialog.FileName;
            }
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!rendering)
            {
                rendering = true;
                RenderSettings.offsetX = xOffInput.Value;
                RenderSettings.offsetY = yOffInput.Value;

                RenderSettings.MaxIterations = (int)iterationCountInput.Value;
                RenderSettings.NumFrames = (int)startFrameInput.Value;
                RenderSettings.ThreadCount = (int)threadCountInput.Value;

                Renderer.Setup(RenderSettings);

                if (!loadingFile)
                {
                    if (!renderActive)
                    {
                        renderActive = true;
                        SaveFractal();
                    }
                    Task.Run(ChosenMethod);
                }
                else
                {
                    UpdateFractal();
                    RGB[] palette = Utils.LoadPallete(RenderSettings.palettePath);
                    Renderer.Initialize(RenderSettings, palette);
                    Renderer.Setup(RenderSettings);
                    Task.Run(new Action(GrabFrame));
                }
                intervalTimer.Start();
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
            Renderer.StopRender();
        }

        // Configuration Events

        private void standardPrecisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            standardPrecisionToolStripMenuItem.Checked = true;
            extraPrescisionToolStripMenuItem.Checked = false;
            ChosenMethod = new Action(Renderer.RenderFrame<Double, DoubleMath>);
            extraPrecision = false;
        }

        private void extraPrescisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            standardPrecisionToolStripMenuItem.Checked = false;
            extraPrescisionToolStripMenuItem.Checked = true;
            ChosenMethod = new Action(Renderer.RenderFrame<Quadruple, QuadrupleMath>);
            extraPrecision = true;
        }

        private void x480ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            x480ToolStripMenuItem.Checked = true;
            x720ToolStripMenuItem.Checked = false;
            x960ToolStripMenuItem.Checked = false;
            RenderSettings.Width = Width = 640;
            RenderSettings.Height = Height = 480;
        }

        private void x720ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            x480ToolStripMenuItem.Checked = false;
            x720ToolStripMenuItem.Checked = true;
            x960ToolStripMenuItem.Checked = false;
            RenderSettings.Width = Width = 900;
            RenderSettings.Height = Height = 720;
        }

        private void x960ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            x480ToolStripMenuItem.Checked = false;
            x720ToolStripMenuItem.Checked = false;
            x960ToolStripMenuItem.Checked = true;
            RenderSettings.Width = Width = 1280;
            RenderSettings.Height = Height = 960;
        }

        private void FractalApp_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoWriter.IsOpen)
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

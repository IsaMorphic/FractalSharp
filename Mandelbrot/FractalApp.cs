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

using Mandelbrot.Imaging;
using Mandelbrot.Rendering;
using Mandelbrot.Utilities;
using Mandelbrot.Algorithms;

using Accord.Video.FFMPEG;
using Mandelbrot.Movies;
using System.Reflection;

namespace Mandelbrot
{
    public partial class FractalApp : Form
    {
        private Type traditionalAlgorithm =
            typeof(TraditionalAlgorithmProvider<>);

        private Type perturbationAlgorithm =
            typeof(PerturbationAlgorithmProvider<>);

        // Video file properties
        private VideoFileReader videoReader = new VideoFileReader();
        private VideoFileWriter videoWriter = new VideoFileWriter();

        // Process statistics
        private DateTime currentFrameStartTime;
        private int coreCount = Environment.ProcessorCount;

        // Rendering related Properties
        private bool Rendering = false;
        private bool PrecisionSwitched = false;
        private double ExtraPrecisionThreshold = Math.Pow(500, 5);

        private Type PreferredAlgorithm =
            typeof(TraditionalAlgorithmProvider<>);

        private GenericMathResolver MathResolver =
            new GenericMathResolver(new Assembly[] 
            { Assembly.GetExecutingAssembly() });

        private MandelbrotMovieRenderer Renderer = new MandelbrotMovieRenderer();
        private ZoomMovieSettings RenderSettings = new ZoomMovieSettings();

        private Action<Type> RenderMethod;

        // Fractal loading and saving properties.  
        private bool RenderActive = false;
        private bool LoadingFile = false;

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

            RenderMethod = Renderer.RenderFrame<double>;

            startFrameInput.Value = RenderSettings.NumFrames;
            iterationCountInput.Value = RenderSettings.MaxIterations;

            xOffInput.Value = RenderSettings.offsetX;
            yOffInput.Value = RenderSettings.offsetY;

            threadCountInput.Value = RenderSettings.ThreadCount / 2;
            threadCountInput.Maximum = RenderSettings.ThreadCount - 1;

            Renderer.FrameStart += FrameStart;
            Renderer.FrameEnd += FrameEnd;

            Renderer.RenderHalted += Shutdown;

            RenderSettings.PalettePath = PalletePath;

            Width = 960;
            Height = 540;
        }

        #region Renderer Events
        private void FrameStart()
        {
            currentFrameStartTime = DateTime.Now;

            BeginInvoke((Action)(() =>
            {
                startFrameInput.Value = Renderer.NumFrames;
                iterationCountInput.Value = Renderer.MaxIterations;
            }));

            // Calculate zoom... using math.pow to keep the zoom rate constant.
            if (Renderer.Magnification > ExtraPrecisionThreshold)
            {
                RenderMethod = Renderer.RenderFrame<decimal>;
                PrecisionSwitched = true;
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

            Task.Run(() => RenderMethod(PreferredAlgorithm));
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
                if (PrecisionSwitched)
                {
                    standardPrecisionToolStripMenuItem.Checked = false;
                    extraPrescisionToolStripMenuItem.Checked = true;
                    RenderMethod = Renderer.RenderFrame<decimal>;
                }
            }));
        }

        #endregion

        #region Main-task Render Methods

        // Simple method that reads a frame 
        // from an old video file and saves it to a new one.  
        private void GrabFrame()
        {
            FrameStart();

            Renderer.SetFrame(Renderer.NumFrames + 1);

            if (Renderer.NumFrames < videoReader.FrameCount - 1)
            {
                Bitmap frame = videoReader.ReadVideoFrame();
                videoWriter.WriteVideoFrame(frame);
                Task.Run(new Action(GrabFrame));
            }
            else
            {
                LoadingFile = false;
                videoReader.Close();
                Task.Run(() => RenderMethod(PreferredAlgorithm));
            }
        }

        #endregion

        #region Fractal Configuration Methods

        private bool LoadFractal()
        {
            bool loaded = false;
            FileLoadDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Renders");
            if (FileLoadDialog.ShowDialog() == DialogResult.OK)
            {
                SettingsPath = FileLoadDialog.FileName;
                string jsonData = File.ReadAllText(SettingsPath);

                JavaScriptSerializer js = new JavaScriptSerializer();
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
                    RenderMethod = Renderer.RenderFrame<decimal>;
                }
                else
                {
                    standardPrecisionToolStripMenuItem.Checked = true;
                    extraPrescisionToolStripMenuItem.Checked = false;
                    RenderMethod = Renderer.RenderFrame<decimal>;
                }


                startFrameInput.Value = RenderSettings.NumFrames;
                iterationCountInput.Value = RenderSettings.MaxIterations;

                xOffInput.Value = RenderSettings.offsetX;
                yOffInput.Value = RenderSettings.offsetY;

                int bitrate = RenderSettings.Width * RenderSettings.Height * 32 * 3 * 8; // 80 percent quality, explained below
                videoReader.Open(String.Format(VideoPath, RenderSettings.Version));
                videoWriter.Open(String.Format(VideoPath, RenderSettings.Version + 1), RenderSettings.Width, RenderSettings.Height, 30, VideoCodec.MPEG4, bitrate);

                loaded = true;
            }
            return loaded;
        }

        private void SaveFractal()
        {
            string fractalName = Path.GetFileNameWithoutExtension(VideoPath).Replace("_{0}", String.Empty);
            string fractalDate = DateTime.Now.ToShortDateString().Replace('/', '-');
            string fileName = String.Format("{0}_{1}.fractal", fractalName, fractalDate);
            string filePath = Path.Combine(Application.StartupPath, "Renders", fileName);

            JavaScriptSerializer js = new JavaScriptSerializer();
            string jsonData = js.Serialize(RenderSettings);

            File.WriteAllText(filePath, jsonData);
        }

        private void UpdateFractal()
        {
            string jsonData = File.ReadAllText(SettingsPath);

            JavaScriptSerializer js = new JavaScriptSerializer();
            RenderSettings = js.Deserialize<ZoomMovieSettings>(jsonData);

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
            RGB[] palette = Utils.LoadPallete(RenderSettings.PalettePath);

            Renderer.Initialize(RenderSettings, palette, MathResolver);

            VideoPath = RenderSaveDialog.FileName;

            string videoDirectory = Path.GetDirectoryName(VideoPath);
            string videoName = Path.GetFileNameWithoutExtension(VideoPath);

            if (!videoName.EndsWith("_{0}.avi"))
                videoName += "_{0}.avi";

            VideoPath = Path.Combine(videoDirectory, videoName);

            RenderSettings.VideoPath = VideoPath;

            // width and height multiplied by 32, 32 bpp
            // then multiply by framerate divided by ten, after multiplying by eight yeilds 80% of the normal amount of bits per second.  
            // Note: we're assuming that there is no audio in the video.  Otherwise we would have to accomidate for that as well.  
            int bitrate = RenderSettings.Width * RenderSettings.Height * 32 * 3 * 8; // 80 percent quality
            videoWriter.Open(String.Format(VideoPath, RenderSettings.Version), RenderSettings.Width, RenderSettings.Height, 30, VideoCodec.MPEG4, bitrate);

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
                LoadingFile = true;

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
                Rendering = true;
                RenderSettings.offsetX = xOffInput.Value;
                RenderSettings.offsetY = yOffInput.Value;

                RenderSettings.MaxIterations = (int)iterationCountInput.Value;
                RenderSettings.NumFrames = (int)startFrameInput.Value;
                RenderSettings.ThreadCount = (int)threadCountInput.Value;

                Renderer.Setup(RenderSettings);

                if (!LoadingFile)
                {
                    if (!RenderActive)
                    {
                        RenderActive = true;
                        SaveFractal();
                    }
                    Task.Run(() => RenderMethod(PreferredAlgorithm));
                }
                else
                {
                    UpdateFractal();
                    RGB[] palette = Utils.LoadPallete(RenderSettings.PalettePath);
                    Renderer.Initialize(RenderSettings, palette, MathResolver);
                    Renderer.Setup(RenderSettings);
                    Task.Run((Action)GrabFrame);
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
            Renderer.StopRender();
        }

        // Configuration Events

        private void standardPrecisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            standardPrecisionToolStripMenuItem.Checked = true;
            extraPrescisionToolStripMenuItem.Checked = false;
            RenderMethod = Renderer.RenderFrame<double>;
            RenderSettings.ExtraPrecision = false;
        }

        private void extraPrescisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            standardPrecisionToolStripMenuItem.Checked = false;
            extraPrescisionToolStripMenuItem.Checked = true;
            RenderMethod = Renderer.RenderFrame<decimal>;
            RenderSettings.ExtraPrecision = true;
        }

        private void traditionalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            perturbationToolStripMenuItem.Checked = false;
            PreferredAlgorithm = traditionalAlgorithm;
        }

        private void perturbationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            traditionalToolStripMenuItem.Checked = false;
            PreferredAlgorithm = perturbationAlgorithm;
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
            var exploreWindow = new Explorer(PalletePath, xOffInput.Value, yOffInput.Value);
            exploreWindow.ShowDialog();

            if (!Rendering)
            {
                xOffInput.Value = exploreWindow.GetXOffset();
                yOffInput.Value = exploreWindow.GetYOffset();
            }
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

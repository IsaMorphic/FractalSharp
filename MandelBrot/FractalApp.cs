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
using System.Threading;
using Quadruple;
using MandelBrot.Utilities;
using System.Numerics;
using System.IO;
using System.Web.Script.Serialization;
using Accord.Video.FFMPEG;

namespace MandelBrot
{
    public partial class FractalApp : Form
    {
        // Video file properties
        private VideoFileReader videoReader = new VideoFileReader();
        private VideoFileWriter videoWriter = new VideoFileWriter();

        // Multi-thread properties
        private DirectBitmap currentFrame;
        private DateTime currentFrameStartTime;
        private int coreCount = Environment.ProcessorCount;

        // Fractal Properties
        private int frameCount = 0;
        private bool rendering = false;
        private int max_iteration = 100;
        private decimal offsetXM = -0.743643887037158704752191506114774M;
        private decimal offsetYM = 0.131825904205311970493132056385139M;
        private Quad offsetXQ = Quad.Parse("-0.743643887037158704752191506114774");
        private Quad offsetYQ = Quad.Parse("0.131825904205311970493132056385139");
        private double offsetXD = -0.743643887037158704752191506114774;
        private double offsetYD = 0.131825904205311970493132056385139;
        private double extraPrecisionThreshold = Math.Pow(500, 5);
        private Size fractalSize = new Size(640, 480);
        private double scaleFactor;
        private Quad scaleFactorQuad;
        private RGB[] palette;
        private bool extraPrecision = false;
        private Action ChosenMethod;
        private int version = 0;


        private string palletePath = Path.Combine(Application.StartupPath, "Palettes", "blues.map");
        private string videoPath;
        private string settingsPath;
        private bool renderActive = false;
        private bool loadingFile = false;

        public FractalApp()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            palette = Utils.LoadPallete(palletePath);
            ChosenMethod = new Action(MandelBrot);
            startFrameInput.Value = frameCount;
            iterationCountInput.Value = max_iteration;
            xOffInput.Value = offsetXM;
            yOffInput.Value = offsetYM;
            threadCountInput.Value = coreCount / 2;
            threadCountInput.Maximum = coreCount - 1;
            Width = 640;
            Height = 480;
        }

        public void GrabFrame()
        {
            frameCount++;
            max_iteration += frameCount / Math.Max(5 - frameCount / 100, 1);
            BeginInvoke((Action)(() =>
            {
                startFrameInput.Value = frameCount;
                iterationCountInput.Value = max_iteration;
            }));
            long in_set = 0;
            // Calculate zoom... using math.pow to keep the zoom rate constant.
            double zoom = Math.Pow(frameCount, frameCount / 100.0);
            if (zoom > extraPrecisionThreshold)
            {
                ChosenMethod = new Action(MandelBrotQuad);
                extraPrecision = true;
            }

            if (frameCount < videoReader.FrameCount - 1)
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


        public void MandelBrot()
        {
            currentFrameStartTime = DateTime.Now;
            frameCount++;
            max_iteration += frameCount / Math.Max(5 - frameCount / 100, 1);
            BeginInvoke((Action)(() =>
            {
                startFrameInput.Value = frameCount;
                iterationCountInput.Value = max_iteration;
            }));
            long in_set = 0;
            // Calculate zoom... using math.pow to keep the zoom rate constant.
            double zoom = Math.Pow(frameCount, frameCount / 100.0);
            if (zoom > extraPrecisionThreshold)
            {
                ChosenMethod = new Action(MandelBrotQuad);
                extraPrecision = true;
            }

            var loop = Parallel.For(0, currentFrame.Width, new ParallelOptions { MaxDegreeOfParallelism = coreCount }, px =>
            {
                double x0 = Utils.Map(px, 0, currentFrame.Width, -scaleFactor / zoom + offsetXD, scaleFactor / zoom + offsetXD);

                for (int py = 0; py < currentFrame.Height; py++)
                {
                    if (!rendering) return;

                    double y0 = Utils.Map(py, 0, currentFrame.Height, -1 / zoom + offsetYD, 1 / zoom + offsetYD);

                    // Initialize some variables..
                    double x = 0.0;
                    double y = 0.0;

                    // Define x squared and y squared as their own variables
                    // To avoid unnecisarry multiplication.
                    double xx = 0.0;
                    double yy = 0.0;

                    // Initialize our iteration count.
                    int iteration = 0;

                    // Mandelbrot algorithm
                    while (xx + yy < 65536 && iteration < max_iteration)
                    {
                        var xtemp = xx - yy + x0;
                        var ytemp = 2 * x * y + y0;

                        if (x == xtemp && y == ytemp)
                        {
                            iteration = max_iteration;
                            break;
                        }

                        x = xtemp;
                        y = ytemp;
                        xx = x * x;
                        yy = y * y;

                        iteration++;
                    }
                    // If x squared plus y squared is outside the set, give it a fancy color.
                    if (xx + yy > 65536)
                    {
                        double temp_i = iteration;
                        // sqrt of inner term removed using log simplification rules.
                        double log_zn = Math.Log(xx + yy) / 2;
                        double nu = Math.Log(log_zn / Math.Log(2)) / Math.Log(2);
                        // Rearranging the potential function.
                        // Dividing log_zn by log(2) instead of log(N = 1<<8)
                        // because we want the entire palette to range from the
                        // center to radius 2, NOT our bailout radius.
                        temp_i = temp_i + 1 - nu;

                        // Grab two colors from the pallete
                        RGB color1 = palette[(int)temp_i % palette.Length];
                        RGB color2 = palette[(int)(temp_i + 1) % palette.Length];

                        // Linear interpolate red, green, and blue values.
                        int final_red = (int)Utils.lerp(color1.red, color2.red, temp_i % 1);

                        int final_green = (int)Utils.lerp(color1.green, color2.green, temp_i % 1);

                        int final_blue = (int)Utils.lerp(color1.blue, color2.blue, temp_i % 1);

                        // Construct a final color with the interpolated values.
                        RGB finalColor = new RGB(final_red, final_green, final_blue);

                        // Then set our pixel to that color.  
                        currentFrame.SetPixel(px, py, Color.FromArgb(finalColor.red, finalColor.green, finalColor.blue));
                    }
                    // Otherwise, make the pixel black, as it is in the set.  
                    else
                    {
                        currentFrame.SetPixel(px, py, Color.Black);
                        Interlocked.Increment(ref in_set);
                    }
                }
            });
            while (!loop.IsCompleted) { Thread.Sleep(300); }
            if (in_set < fractalSize.Width * fractalSize.Height && rendering)
            {
                try
                {
                    Bitmap newFrame = (Bitmap)currentFrame.Bitmap.Clone();
                    videoWriter.WriteVideoFrame(newFrame);
                    if (livePreviewCheckBox.Checked)
                    {
                        pictureBox1.Image = newFrame;
                    }
                }
                catch (ArgumentException) { };
                Task.Run(ChosenMethod);
            }
            else if (rendering)
            {
                Shutdown();
            }
        }



        public void MandelBrotQuad()
        {
            currentFrameStartTime = DateTime.Now;
            frameCount++;
            max_iteration += frameCount / Math.Max(5 - frameCount / 100, 1);
            BeginInvoke((Action)(() =>
            {
                startFrameInput.Value = frameCount;
                iterationCountInput.Value = max_iteration;
            }));
            long in_set = 0;
            // Calculate zoom... using math.pow to keep the zoom rate constant.
            Quad zoom = Math.Pow(frameCount, frameCount / 100.0);
            var loop = Parallel.For(0, currentFrame.Width, new ParallelOptions { MaxDegreeOfParallelism = coreCount }, px =>
            {

                // Map the x coordinate to Mandelbrot Space only once per outer loop.
                Quad x0 = Utils.MapQuad(px, 0, currentFrame.Width, -scaleFactorQuad / zoom + offsetXQ, scaleFactorQuad / zoom + offsetXQ);

                for (int py = 0; py < currentFrame.Height; py++)
                {
                    if (!rendering) return;

                    // Then map the y coordinate for every pixel.
                    Quad y0 = Utils.MapQuad(py, 0, currentFrame.Height, -1 / zoom + offsetYQ, 1 / zoom + offsetYQ);

                    // Initialize some variables..
                    Quad x = 0.0;
                    Quad y = 0.0;

                    // Define x squared and y squared as their own variables
                    // To avoid unnecisarry multiplication.
                    Quad xx = 0.0;
                    Quad yy = 0.0;

                    // Initialize our iteration count.
                    int iteration = 0;

                    // Mandelbrot algorithm
                    while (xx + yy < 65536 && iteration < max_iteration)
                    {
                        var xtemp = xx - yy + x0;
                        var ytemp = 2 * x * y + y0;

                        if (x == xtemp && y == ytemp)
                        {
                            iteration = max_iteration;
                            break;
                        }

                        x = xtemp;
                        y = ytemp;
                        xx = x * x;
                        yy = y * y;

                        iteration++;
                    }
                    // If x squared plus y squared is outside the set, give it a fancy color.
                    if (xx + yy > 65536)
                    {
                        double temp_i = iteration;
                        // sqrt of inner term removed using log simplification rules.
                        double log_zn = Math.Log((double)xx + (double)yy) / 2;
                        double nu = Math.Log(log_zn / Math.Log(2)) / Math.Log(2);
                        // Rearranging the potential function.
                        // Dividing log_zn by log(2) instead of log(N = 1<<8)
                        // because we want the entire palette to range from the
                        // center to radius 2, NOT our bailout radius.
                        temp_i = temp_i + 1 - nu;

                        // Grab two colors from the pallete
                        RGB color1 = palette[(int)temp_i % palette.Length];
                        RGB color2 = palette[(int)(temp_i + 1) % palette.Length];

                        // Linear interpolate red, green, and blue values.
                        int final_red = (int)Utils.lerp(color1.red, color2.red, temp_i % 1);

                        int final_green = (int)Utils.lerp(color1.green, color2.green, temp_i % 1);

                        int final_blue = (int)Utils.lerp(color1.blue, color2.blue, temp_i % 1);

                        // Construct a final color with the interpolated values.
                        RGB finalColor = new RGB(final_red, final_green, final_blue);

                        // Then set our pixel to that color.  
                        currentFrame.SetPixel(px, py, Color.FromArgb(finalColor.red, finalColor.green, finalColor.blue));
                    }
                    // Otherwise, make the pixel black, as it is in the set.  
                    else
                    {
                        currentFrame.SetPixel(px, py, Color.Black);
                        Interlocked.Increment(ref in_set);
                    }
                }
            });
            while (!loop.IsCompleted) { Thread.Sleep(300); }
            if (in_set < fractalSize.Width * fractalSize.Height && rendering)
            {
                try
                {
                    Bitmap newFrame = (Bitmap)currentFrame.Bitmap.Clone();
                    videoWriter.WriteVideoFrame(newFrame);
                    if (livePreviewCheckBox.Checked)
                    {
                        pictureBox1.Image = newFrame;
                    }
                }
                catch (ArgumentException) { };
                Task.Run(new Action(MandelBrotQuad));
            }
            else if (rendering)
            {
                Shutdown();
            }
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
                    doublePrecisionToolStripMenuItem.Checked = false;
                    extraPrescisionToolStripMenuItem.Checked = true;
                    ChosenMethod = new Action(MandelBrotQuad);
                }
            }));
        }

        private void SaveFractal()
        {
            FractalSettings fractal = new FractalSettings();
            fractal.frameCount = frameCount;
            fractal.offsetX = offsetXM;
            fractal.offsetY = offsetYM;
            fractal.max_iteration = max_iteration;
            fractal.palettePath = palletePath;
            fractal.videoPath = videoPath;
            fractal.fractalSize = fractalSize;
            fractal.version = version;
            string fractalName = Path.GetFileNameWithoutExtension(videoPath).Replace("_{0}", String.Empty);
            string fractalDate = DateTime.Now.ToShortDateString().Replace('/', '-');
            string fileName = String.Format("{0}_{1}.fractal", fractalName, fractalDate);
            string filePath = Path.Combine(Application.StartupPath, "Renders", fileName);

            JavaScriptSerializer js = new JavaScriptSerializer();
            string jsonData = js.Serialize(fractal);

            File.WriteAllText(filePath, jsonData);
        }

        private void UpdateFractal()
        {
            version++;

            string jsonData = File.ReadAllText(settingsPath);

            JavaScriptSerializer js = new JavaScriptSerializer();
            FractalSettings fractal = js.Deserialize<FractalSettings>(jsonData);
            
            fractal.version = version;

            jsonData = js.Serialize(fractal);

            File.WriteAllText(settingsPath, jsonData);
        }

        private bool LoadFractal()
        {
            bool loaded = false;
            openFileDialog1.InitialDirectory = Path.Combine(Application.StartupPath, "Renders");
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                settingsPath = openFileDialog1.FileName;
                string jsonData = File.ReadAllText(settingsPath);

                JavaScriptSerializer js = new JavaScriptSerializer();
                FractalSettings newFractal = js.Deserialize<FractalSettings>(jsonData);

                frameCount = newFractal.frameCount;
                max_iteration = newFractal.max_iteration;

                offsetXM = newFractal.offsetX;
                offsetYM = newFractal.offsetY;

                offsetXQ = Quad.Parse(offsetXM.ToString());
                offsetYQ = Quad.Parse(offsetYM.ToString());

                offsetXD = (double)offsetXQ;
                offsetYD = (double)offsetYQ;

                palletePath = newFractal.palettePath;
                videoPath = newFractal.videoPath;

                version = newFractal.version;

                x480ToolStripMenuItem.Checked = false;
                x720ToolStripMenuItem.Checked = false;
                x960ToolStripMenuItem.Checked = false;

                Size = fractalSize = newFractal.fractalSize;

                if (fractalSize.Height == 480)
                {
                    x480ToolStripMenuItem.Checked = true;
                }
                else if (fractalSize.Height == 720)
                {
                    x720ToolStripMenuItem.Checked = true;
                }
                else if (fractalSize.Height == 960)
                {
                    x960ToolStripMenuItem.Checked = true;
                }

                scaleFactor = (double)fractalSize.Width / (double)fractalSize.Height;

                scaleFactorQuad = scaleFactor;

                currentFrame = new DirectBitmap(fractalSize.Width, fractalSize.Height);

                palette = Utils.LoadPallete(palletePath);

                startFrameInput.Value = frameCount;
                iterationCountInput.Value = max_iteration;

                xOffInput.Value = offsetXM;
                yOffInput.Value = offsetYM;

                int bitrate = fractalSize.Width * fractalSize.Height * 32 * 3 * 8; // 80 percent quality, explained below
                videoReader.Open(String.Format(videoPath, newFractal.version));
                videoWriter.Open(String.Format(videoPath, newFractal.version + 1), fractalSize.Width, fractalSize.Height, 30, VideoCodec.MPEG4, bitrate);

                loaded = true;
            }
            return loaded;
        }


        private void newRenderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            scaleFactor = (double)fractalSize.Width / (double)fractalSize.Height;
            scaleFactorQuad = scaleFactor;

            currentFrame = new DirectBitmap(fractalSize.Width, fractalSize.Height);

            videoPath = saveFileDialog1.FileName;

            string videoDirectory = Path.GetDirectoryName(videoPath);
            string videoName = Path.GetFileNameWithoutExtension(videoPath);

            if (!videoName.EndsWith("_{0}.avi"))
                videoName += "_{0}.avi";

            videoPath = Path.Combine(videoDirectory, videoName);
            // width and height multiplied by 32, 32 bpp
            // then multiply by framerate divided by ten, after multiplying by eight yeilds 80% of the normal amount of bits per second.  
            // Note: we're assuming that there is no audio in the video.  Otherwise we would have to accomidate for that as well.  
            int bitrate = fractalSize.Width * fractalSize.Height * 32 * 3 * 8; // 80 percent quality
            videoWriter.Open(String.Format(videoPath, version), fractalSize.Width, fractalSize.Height, 30, VideoCodec.MPEG4, bitrate);

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
            currentFrame.Dispose();
        }

        private void loadPaletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Path.Combine(Application.StartupPath, "Palettes");
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                palletePath = openFileDialog1.FileName;
                palette = Utils.LoadPallete(palletePath);
            }
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!rendering)
            {
                rendering = true;
                offsetXM = xOffInput.Value;
                offsetYM = yOffInput.Value;

                offsetXQ = Quad.Parse(offsetXM.ToString());
                offsetYQ = Quad.Parse(offsetYM.ToString());

                offsetXD = (double)offsetXQ;
                offsetYD = (double)offsetYQ;

                max_iteration = (int)iterationCountInput.Value;
                frameCount = (int)startFrameInput.Value;
                coreCount = (int)threadCountInput.Value;

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
            Task.Run(new Action(Shutdown));
        }

        // Configuration Events

        private void doublePrecisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doublePrecisionToolStripMenuItem.Checked = true;
            extraPrescisionToolStripMenuItem.Checked = false;
            ChosenMethod = new Action(MandelBrot);
            extraPrecision = false;
        }

        private void decimalPrescisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doublePrecisionToolStripMenuItem.Checked = false;
            extraPrescisionToolStripMenuItem.Checked = true;
            ChosenMethod = new Action(MandelBrotQuad);
            extraPrecision = true;
        }

        private void x480ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            x480ToolStripMenuItem.Checked = true;
            x720ToolStripMenuItem.Checked = false;
            x960ToolStripMenuItem.Checked = false;
            fractalSize.Width = Width = 640;
            fractalSize.Height = Height = 480;
        }

        private void x720ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            x480ToolStripMenuItem.Checked = false;
            x720ToolStripMenuItem.Checked = true;
            x960ToolStripMenuItem.Checked = false;
            fractalSize.Width = Width = 900;
            fractalSize.Height = Height = 720;
        }

        private void x960ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            x480ToolStripMenuItem.Checked = false;
            x720ToolStripMenuItem.Checked = false;
            x960ToolStripMenuItem.Checked = true;
            fractalSize.Width = Width = 1280;
            fractalSize.Height = Height = 960;
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
    }
}

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
using Quadruple;
namespace MandelBrot
{
    public partial class FractalApp : Form
    {
        // Video file properties
        private AviWriter videoFile;
        private IAviVideoStream stream;


        // Multi-thread properties
        private DirectBitmap currentFrame;
        private DateTime currentFrameStartTime;

        // Fractal Properties
        private int frameCount = 0;
        private bool rendering = false;
        private int max_iteration = 100;
        private Quad offsetXQuad = -0.743643887037158704752191506114774;
        private Quad offsetYQuad = 0.131825904205311970493132056385139;
        private double offsetX = -0.743643887037158704752191506114774;
        private double offsetY = 0.131825904205311970493132056385139;
        private double extraPrecisionThreshold = Math.Pow(500, 5);
        private Size fractalSize = new Size(640, 480);
        private double scaleFactor;
        private Quad scaleFactorQuad;
        private RGB[] palette;
        private Action ChosenMethod;
        private bool shouldUseGPU = false;
        public FractalApp()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            palette = Utils.LoadPallete(@"Palettes\blues.map");
            ChosenMethod = new Action(MandelBrot);
            startFrameInput.Value = frameCount;
            iterationCountInput.Value = max_iteration;
            xOffInput.Value = (decimal)offsetX;
            yOffInput.Value = (decimal)offsetY;
            Width = 640;
            Height = 480;
        }

        public void MandelBrot()
        {
            currentFrameStartTime = DateTime.Now;
            frameCount++;
            max_iteration += Math.Min(frameCount, 40);
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
                BeginInvoke((Action)(() =>
                {
                    gPUAccelerationToolStripMenuItem.Checked = false;
                }));
                ChosenMethod = new Action(MandelBrotDecimal);
            }
            if (!shouldUseGPU)
            {
                var loop = Parallel.For(0, currentFrame.Width, px =>
                {
                    // Map our x coordinate to Mandelbrot space.
                    double x0 = Utils.Map(px, 0, currentFrame.Width, -scaleFactor / zoom + offsetX, scaleFactor / zoom + offsetX);

                    for (int py = 0; py < currentFrame.Height; py++)
                    {
                        if (!rendering) return;

                        double y0 = Utils.Map(py, 0, currentFrame.Height, -1 / zoom + offsetY, 1 / zoom + offsetY);

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
                        while (xx + yy < 16 && iteration < max_iteration)
                        {
                            var xtemp = xx - yy + x0;
                            y = 2 * x * y + y0;
                            x = xtemp;
                            xx = x * x;
                            yy = y * y;
                            iteration++;
                        }
                        // If x squared plus y squared is outside the set, give it a fancy color.
                        if (xx + yy > 16)
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
            }
            else
            {
                GPUFractal.RenderFrame(ref currentFrame, frameCount, max_iteration, palette);
            }
            if (in_set < fractalSize.Width * fractalSize.Height && rendering)
            {
                try
                {
                    byte[] frame = Utils.BitmapToByteArray(currentFrame.Bitmap);
                    stream.WriteFrame(true, frame, 0, frame.Length);
                    if (livePreviewCheckBox.Checked)
                    {
                        pictureBox1.Image = currentFrame.Bitmap;
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

        public void MandelBrotDecimal()
        {
            currentFrameStartTime = DateTime.Now;
            frameCount++;
            max_iteration += Math.Min(frameCount, 40);
            BeginInvoke((Action)(() =>
            {
                startFrameInput.Value = frameCount;
                iterationCountInput.Value = max_iteration;
            }));
            long in_set = 0;
            // Calculate zoom... using math.pow to keep the zoom rate constant.
            Quad zoom = Math.Pow(frameCount, frameCount / 100.0);
            var loop = Parallel.For(0, currentFrame.Width, px =>
            {

                // Map the x coordinate to Mandelbrot Space only once per outer loop.
                Quad x0 = Utils.MapQuad(px, 0, currentFrame.Width, -scaleFactorQuad / zoom + offsetXQuad, scaleFactorQuad / zoom + offsetXQuad);

                for (int py = 0; py < currentFrame.Height; py++)
                {
                    if (!rendering) return;

                    // Then map the y coordinate for every pixel.
                    Quad y0 = Utils.MapQuad(py, 0, currentFrame.Height, -1 / zoom + offsetYQuad, 1 / zoom + offsetYQuad);

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
                    while (xx + yy < 16 && iteration < max_iteration)
                    {
                        var xtemp = xx - yy + x0;
                        y = 2 * x * y + y0;
                        x = xtemp;
                        xx = x * x;
                        yy = y * y;
                        iteration++;
                    }
                    // If x squared plus y squared is outside the set, give it a fancy color.
                    if (xx + yy > 16)
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
                    byte[] frame = Utils.BitmapToByteArray(currentFrame.Bitmap);
                    stream.WriteFrame(true, frame, 0, frame.Length);
                    if (livePreviewCheckBox.Checked)
                    {
                        pictureBox1.Image = currentFrame.Bitmap;
                    }
                }
                catch (ArgumentException) { };
                Task.Run(new Action(MandelBrotDecimal));
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
                accelerationToolStripMenuItem.Enabled = true;
                tableLayoutPanel1.Enabled = true;
                pictureBox1.Image = null;
            }));
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            currentFrame = new DirectBitmap(fractalSize.Width, fractalSize.Height);
            videoFile = new AviWriter(saveFileDialog1.FileName);
            videoFile.FramesPerSecond = 30;
            stream = videoFile.AddMotionJpegVideoStream(fractalSize.Width, fractalSize.Height, 90);
            newRenderToolStripMenuItem.Enabled = false;
            closeCurrentToolStripMenuItem.Enabled = true;
            startToolStripMenuItem.Enabled = true;
        }

        private void newRenderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void closeCurrentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startToolStripMenuItem.Enabled = false;
            ResolutionToolStripMenuItem.Enabled = true;
            closeCurrentToolStripMenuItem.Enabled = false;
            newRenderToolStripMenuItem.Enabled = true;
            loadPaletteToolStripMenuItem.Enabled = true;
            videoFile.Close();
            currentFrame.Dispose();
        }

        private void loadPaletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            palette = Utils.LoadPallete(openFileDialog1.FileName);
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!rendering)
            {
                rendering = true;
                scaleFactor = (double)fractalSize.Width / (double)fractalSize.Height;
                scaleFactorQuad = scaleFactor;
                offsetX = (double)xOffInput.Value;
                offsetY = (double)yOffInput.Value;
                offsetXQuad = offsetX;
                offsetYQuad = offsetY;
                max_iteration = (int)iterationCountInput.Value;
                frameCount = (int)startFrameInput.Value;
                Task.Run(ChosenMethod);
                intervalTimer.Start();
                ResolutionToolStripMenuItem.Enabled = false;
                presicionStripMenuItem.Enabled = false;
                stopToolStripMenuItem.Enabled = true;
                startToolStripMenuItem.Enabled = false;
                closeCurrentToolStripMenuItem.Enabled = false;
                loadPaletteToolStripMenuItem.Enabled = false;
                tableLayoutPanel1.Enabled = false;
                accelerationToolStripMenuItem.Enabled = false;
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
            accelerationToolStripMenuItem.Enabled = true;
            ChosenMethod = new Action(MandelBrot);
        }

        private void decimalPrescisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doublePrecisionToolStripMenuItem.Checked = false;
            extraPrescisionToolStripMenuItem.Checked = true;
            accelerationToolStripMenuItem.Enabled = false;
            ChosenMethod = new Action(MandelBrotDecimal);
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
            if (videoFile != null)
            {
                e.Cancel = true;
                Task.Run(new Action(Shutdown));
                videoFile.Close();
                currentFrame.Dispose();
                e.Cancel = false;
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

        private void gPUAccelerationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!extraPrescisionToolStripMenuItem.Checked)
            {
                shouldUseGPU = gPUAccelerationToolStripMenuItem.Checked;
            }
            else
            {
                shouldUseGPU = gPUAccelerationToolStripMenuItem.Checked = false;
            }
        }
    }
}

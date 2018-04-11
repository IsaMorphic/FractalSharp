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
namespace MandelBrot
{
    public partial class FractalApp : Form
    {
        // Video file properties
        public AviWriter videoFile;
        public IAviVideoStream stream;


        // Multi-thread properties
        public DirectBitmap currentFrame;

        // Fractal Properties
        public int frameCount = 0;
        public bool rendering = false;
        public int max_iteration = 3500;
        public decimal offsetXDec = -0.743643887037158704752191506114774M;
        public decimal offsetYDec = 0.131825904205311970493132056385139M;
        public double offsetX = -0.743643887037158704752191506114774;
        public double offsetY = 0.131825904205311970493132056385139;
        public Size fractalSize = new Size(640, 480);
        public double scaleFactor;
        public decimal scaleFactorDec;
        public RGB[] palette;
        public Action ChosenMethod;


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
            xOffInput.Value = offsetXDec;
            yOffInput.Value = offsetYDec;
            Width = 640;
            Height = 480;
        }

        public void MandelBrot()
        {
            frameCount++;
            long in_set = 0;
            var loop = Parallel.For(0, currentFrame.Width, px =>
            {
                // Calculate zoom... using math.pow to keep the zoom rate constant.
                double zoom = Math.Pow(frameCount, frameCount / 100.0);

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
            if (in_set < fractalSize.Width * fractalSize.Height && rendering)
            {
                try
                {
                    byte[] frame = Utils.BitmapToByteArray(currentFrame.Bitmap);
                    stream.WriteFrame(true, frame, 0, frame.Length);
                }
                catch (ArgumentException) { };
                Task.Run(new Action(MandelBrot));
            }
            else if (rendering)
            {
                Shutdown();
            }
        }

        public void MandelBrotDecimal()
        {
            frameCount++;
            long in_set = 0;
            var loop = Parallel.For(0, currentFrame.Width, px =>
            {
                // Calculate zoom
                decimal zoom = (decimal)Math.Pow(frameCount, frameCount / 100.0);

                // Map the x coordinate to Mandelbrot Space only once per outer loop.
                decimal x0 = Utils.MapDecimal(px, 0, currentFrame.Width, -scaleFactorDec / zoom + offsetXDec, scaleFactorDec / zoom + offsetXDec);

                for (int py = 0; py < currentFrame.Height; py++)
                {
                    if (!rendering) return;

                    // Then map the y coordinate for every pixel.
                    decimal y0 = Utils.MapDecimal(py, 0, currentFrame.Height, -1.34M / zoom + offsetYDec, 1 / zoom + offsetYDec);

                    // Initialize some variables..
                    decimal x = 0.0M;
                    decimal y = 0.0M;

                    // Define x squared and y squared as their own variables
                    // To avoid unnecisarry multiplication.
                    decimal xx = 0.0M;
                    decimal yy = 0.0M;

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
                startToolStripMenuItem.Text = "Start";
                presicionStripMenuItem.Enabled = true;
                stopToolStripMenuItem.Enabled = false;
                startToolStripMenuItem.Enabled = true;
                closeCurrentToolStripMenuItem.Enabled = true;
                tableLayoutPanel1.Enabled = true;
                startFrameInput.Value = frameCount;
                intervalTimer.Stop();
            }));
            MessageBox.Show("Rendering Stopped.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                scaleFactorDec = (decimal)fractalSize.Width / (decimal)fractalSize.Height;
                offsetXDec = xOffInput.Value;
                offsetYDec = yOffInput.Value;
                offsetX = (double)offsetXDec;
                offsetY = (double)offsetYDec;
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
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rendering = false;
            Task.Run(new Action(Shutdown));
        }

        // Configuration Events

        private void doublePrecisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doublePrecisionToolStripMenuItem.Checked = true;
            decimalPrescisionToolStripMenuItem.Checked = false;
            ChosenMethod = new Action(MandelBrot);
        }

        private void decimalPrescisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doublePrecisionToolStripMenuItem.Checked = false;
            decimalPrescisionToolStripMenuItem.Checked = true;
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

        private void intervalTimer_Tick(object sender, EventArgs e)
        {
            if (livePreviewCheckBox.Checked)
            {
                Bitmap img = new Bitmap(fractalSize.Width, fractalSize.Height);
                var bits = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
                Marshal.Copy(currentFrame.Bits, 0, bits.Scan0, currentFrame.Bits.Length);
                img.UnlockBits(bits);
                pictureBox1.Image = img;
            }
        }
    }
}

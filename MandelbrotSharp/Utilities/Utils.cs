using MandelbrotSharp.Imaging;
using MandelbrotSharp.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MandelbrotSharp.Utilities
{
    public class Utils
    {
        public static RgbValue[] LoadPallete(string path)
        {
            List<RgbValue> pallete = new List<RgbValue>();
            StreamReader palleteData = new StreamReader(path);
            while (!palleteData.EndOfStream)
            {
                try
                {
                    string palleteString = palleteData.ReadLine();
                    string[] palleteTokens = palleteString.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int r = int.Parse(palleteTokens[0]);
                    int g = int.Parse(palleteTokens[1]);
                    int b = int.Parse(palleteTokens[2]);
                    RgbValue color = new RgbValue(r, g, b);
                    pallete.Add(color);
                }
                catch (FormatException) { }
            }
            return pallete.ToArray();
        }
        public static byte[] BitmapToByteArray(Bitmap img)
        {
            // and buffer of appropriate size for storing its bits
            var buffer = new byte[img.Width * img.Height * 4];

            // Now copy bits from bitmap to buffer
            var bits = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            Marshal.Copy(bits.Scan0, buffer, 0, buffer.Length);
            img.UnlockBits(bits);
            return buffer;
        }

        public static T Map<T>(GenericMath<T> TMath, T OldValue, T OldMin, T OldMax, T NewMin, T NewMax)
        {
            T OldRange = TMath.Subtract(OldMax, OldMin);
            T NewRange = TMath.Subtract(NewMax, NewMin);
            // (((OldValue - OldMin) * NewRange) / OldRange) + NewMin
            T NewValue = TMath.Add(TMath.Divide(TMath.Multiply(TMath.Subtract(OldValue, OldMin), NewRange), OldRange), NewMin);
            return NewValue;
        }

        public static double lerp(double v0, double v1, double t)
        {
            return (1 - t) * v0 + t * v1;
        }

        public static List<Type> GetAllImplementationsInAssemblies(Assembly[] assemblies, Type @interface)
        {
            List<Type> resolvedTypes = new List<Type>();

            foreach (Assembly assembly in assemblies)
            {
                var definedTypes = assembly.DefinedTypes;

                foreach (var type in definedTypes)
                {
                    if (type.BaseType == @interface)
                    {
                        resolvedTypes.Add(type);
                    }
                }
            }

            return resolvedTypes;
        }
    }
}

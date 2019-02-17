using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MandelbrotSharp.Imaging;
using MandelbrotSharp.Rendering;
using MandelbrotSharp.Mathematics;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Collections.ObjectModel;

namespace MandelbrotSharp.Utilities
{

    public class BigDecimalConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return new ReadOnlyCollection<Type>(new List<Type>(new Type[] { typeof(BigDecimal) }));
            }
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            if (type == typeof(BigDecimal))
                return BigDecimal.Parse((string)dictionary["value"]);
            else
                return null;
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            try
            {
                BigDecimal val = (BigDecimal)obj;
                return new Dictionary<string, object>() { { "value", val.ToString() } };
            }
            catch (InvalidCastException) {
                return new Dictionary<string, object>();
            }
        }
    }

    public class Utils
    {
        public static RGB[] LoadPallete(string path)
        {
            List<RGB> pallete = new List<RGB>();
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
                    RGB color = new RGB(r, g, b);
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

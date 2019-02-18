using MandelbrotSharp.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Mandelbrot
{
    public class Utils {
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
    }

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
            catch (InvalidCastException)
            {
                return new Dictionary<string, object>();
            }
        }
    }

}

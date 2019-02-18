using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Mandelbrot
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
            catch (InvalidCastException)
            {
                return new Dictionary<string, object>();
            }
        }
    }

}

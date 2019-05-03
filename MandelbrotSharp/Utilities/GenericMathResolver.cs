using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using MandelbrotSharp.Mathematics;

namespace MandelbrotSharp.Utilities
{
    public static class GenericMathResolver
    {
        public static List<Assembly> Assemblies = new List<Assembly> { Assembly.Load("MandelbrotSharp") };

        private static Dictionary<Type, Type> CachedTypes =
            new Dictionary<Type, Type>();

        public static object CreateMathObject(Type NumType)
        {

            Type NumResolved = null;

            if (CachedTypes.ContainsKey(NumType))
            {
                NumResolved = CachedTypes[NumType];
            }
            else
            {
                Type GenericInterface = typeof(GenericMath<>);
                Type NumInterface = GenericInterface.MakeGenericType(NumType);

                List<Type> ResolvedTypes = Utils.GetAllImplementationsInAssemblies(Assemblies.ToArray(), NumInterface);

                NumResolved = ResolvedTypes.Single();
                CachedTypes.Add(NumType, NumResolved);
            }

            object TMath = Activator.CreateInstance(NumResolved);

            return TMath;
        }

    }
}

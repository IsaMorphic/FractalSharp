using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Mandelbrot.Mathematics;

namespace Mandelbrot.Utilities
{
    class GenericMathResolver
    {
        Assembly[] Assemblies;

        private Dictionary<Type, Type> CachedTypes =
            new Dictionary<Type, Type>();

        public GenericMathResolver(Assembly[] assemblies)
        {
            Assemblies = assemblies;
        }

        public IGenericMath<T> CreateMathObject<T>()
        {
            Type NumType = typeof(T);

            Type NumResolved = null;

            if (CachedTypes.ContainsKey(NumType))
            {
                NumResolved = CachedTypes[NumType];
            }
            else
            {
                Type GenericInterface = typeof(IGenericMath<>);
                Type NumInterface = GenericInterface.MakeGenericType(NumType);

                List<Type> ResolvedTypes = Utils.GetAllImplementationsInAssemblies(Assemblies, NumInterface);

                NumResolved = ResolvedTypes.Single();
                CachedTypes.Add(NumType, NumResolved);
            }

            IGenericMath<T> TMath = (IGenericMath<T>)
                Activator.CreateInstance(NumResolved);

            return TMath;
        }

    }
}

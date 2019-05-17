using MandelbrotSharp.Numerics;
using MiscUtil;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MandelbrotSharp.Utilities
{
    public class Utils
    {
        public static Number<T> Map<T>(Number<T> OldValue, Number<T> OldMin, Number<T> OldMax, Number<T> NewMin, Number<T> NewMax) where T : struct
        {
            T OldRange = OldMax - OldMin;
            T NewRange = NewMax - NewMin;
            T NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
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

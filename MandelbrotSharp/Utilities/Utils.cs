using MiscUtil;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MandelbrotSharp.Utilities
{
    public class Utils
    {
        public static T Map<T>(T OldValue, T OldMin, T OldMax, T NewMin, T NewMax)
        {
            T OldRange = Operator.Subtract(OldMax, OldMin);
            T NewRange = Operator.Subtract(NewMax, NewMin);
            // (((OldValue - OldMin) * NewRange) / OldRange) + NewMin
            T NewValue = Operator.Add(Operator.Divide(Operator.Multiply(Operator.Subtract(OldValue, OldMin), NewRange), OldRange), NewMin);
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

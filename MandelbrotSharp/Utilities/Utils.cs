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

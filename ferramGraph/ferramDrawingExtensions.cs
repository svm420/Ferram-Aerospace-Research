/* Name:    FerramGraph (Graph GUI Plugin)
 * Version: 1.3   (KSP 0.22+)
Copyright 2018, Michael Ferrara, aka Ferram4, and Daumantas Kavolis, aka dkavolis

    This file is part of FerramGraph.

    FerramGraph is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    FerramGraph is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with FerramGraph.  If not, see <http://www.gnu.org/licenses/>. *

 * Disclaimer: You use this at your own risk; this is an alpha plugin for an alpha game; if your computer disintigrates, it's not my fault. :P
 *
 *
 */

using System;
using UnityEngine;

namespace ferram4
{
    public static class ferramDrawingExtensions
    {
        public static Color EmptyColor = new Color(0, 0, 0, 0);

        public static double[] replaceNaNs(this double[] array, double value = 0.0, string name = "double[]")
        {
            int elements = array.Length;
            double[] other = new double[elements];
            for (int i = 0; i < elements; i++)
            {
                if (double.IsNaN(array[i]))
                {
                    other[i] = value;
                    MonoBehaviour.print("[FAR] [ferramGraph] Warning: NaN in " + name + " array; value set to " + value);
                }
                else
                    other[i] = array[i];
            }
            return other;
        }

        public static int[] toPixels(this double[] raw, double zeroValue, double scale, double scaling = 1.0)
        {
            int elements = raw.Length;
            int[] pixels = new int[elements];
            double tmp;
            for (int i = 0; i < elements; i++)
            {
                tmp = raw[i] * scale;
                tmp -= zeroValue;
                tmp *= scaling;

                pixels[i] = (int)Math.Round(tmp);
            }

            return pixels;
        }

        public static double[] toPixelsF(this double[] raw, double zeroValue, double scale, double scaling = 1.0)
        {
            int elements = raw.Length;
            double[] pixels = new double[elements];
            double tmp;
            for (int i = 0; i < elements; i++)
            {
                tmp = raw[i] * scale;
                tmp -= zeroValue;
                tmp *= scaling;

                pixels[i] = tmp;
            }

            return pixels;
        }

        public static void Clear(this Texture2D tex, Color color)
        {
            for (int i = 0; i < tex.width; i++)
                for (int j = 0; j < tex.height; j++)
                    tex.SetPixel(i, j, color);
        }

        public static bool inBounds(this Texture2D tex, int i, int j)
        {
            return (i >= 0 && i < tex.width) && (j >= 0 && j < tex.height);
        }

        /// <summary>
        /// Ferram's implementation of line drawing
        /// </summary>
        public static void DrawLine(this Texture2D tex, int x0, int y0, int x1, int y1, Color color, int lw = 1)
        {
            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            int tmpLw = lw - 1;
            double slope = (double)(y1 - y0) / (double)(x1 - x0);
            int linear;
            int k;

            if (slope <= 1)
            {
                if (x1 == x0)
                    return;

                for (int i = x0; i < x1; i++)
                {
                    linear = (int)Math.Round(slope * (i - x0) + y0);
                    for (int j = -tmpLw; j < tmpLw + 1; j++)
                    {
                        k = j + linear;
                        if (tex.inBounds(i, k))
                            tex.SetPixel(i, k, color);
                    }
                }
            }
            else
            {
                if (y1 == y0)
                    return;

                slope = 1 / slope;
                for (int j = y0; j < y1; j++)
                {
                    linear = (int)Math.Round(slope * (j - y0) + x0);
                    for (int i = -tmpLw; i < tmpLw + 1; i++)
                    {
                        k = i + linear;
                        if (tex.inBounds(k, j))
                            tex.SetPixel(k, j, color);
                    }
                }
            }
        }

        private static void Swap(ref int x, ref int y)
        {
            int tmp = y;
            y = x;
            x = tmp;
        }
    }
}

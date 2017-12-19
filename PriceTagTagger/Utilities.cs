using System;
using System.Drawing;
using System.IO;
using PriceTagTagger;

static internal class Utilities
{
    public static string GetNextFile(string currentImage)
    {
        var files = Directory.GetFiles(Path.GetDirectoryName(currentImage));

        var next = false;

        foreach (var f in files)
        {
            if (next)
                return f;

            if (f == currentImage)
                next = true;
        }
        return files[0];

    }

    public static int ZOrderComparer(CascadeMatch x, CascadeMatch y)
    {
        return x.Cascade.ZOrder.CompareTo(y.Cascade.ZOrder);
    }

    public static long Map(long x, long in_min, long in_max, long out_min, long out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}
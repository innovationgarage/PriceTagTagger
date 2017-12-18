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

    public static int ZOrderComparer(Tuple<Rectangle, Cascade> x, Tuple<Rectangle, Cascade> y)
    {
        return x.Item2.ZOrder.CompareTo(y.Item2.ZOrder);
    }

    public static long Map(long x, long in_min, long in_max, long out_min, long out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}
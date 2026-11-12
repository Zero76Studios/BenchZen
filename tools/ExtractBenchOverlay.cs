// Copies existing bench pixels only. Does not edit the background or draw a bench.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
public static class BenchZenBenchOverlay {
    public static int Run(string root) {
        int count = 0;
        int[,] bands = {{737,757},{761,778},{783,800},{816,824}};
        using (var source = new Bitmap(Path.Combine(root,"assets/images/paris/paris-clean.png")))
        using (var output = new Bitmap(310,87,PixelFormat.Format32bppArgb)) {
            if (source.Width != 1672 || source.Height != 941) throw new Exception("Unexpected Paris dimensions");
            for (int i=0; i<4; i++) for (int y=bands[i,0]; y<bands[i,1]; y++) for (int x=690; x<1000; x++) {
                output.SetPixel(x-690,y-737,source.GetPixel(x,y)); count++;
            }
            output.Save(Path.Combine(root,"assets/images/paris/bench-preview/bench-center-occlusion.png"),ImageFormat.Png);
        }
        return count;
    }
}

// Deterministic pixel extraction. Uses only Windows' built-in System.Drawing.
// No generated pixels, rescaling, retouching, alpha normalization, or mirroring.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public class PoseRecord {
    public string id;
    public string file;
    public int[] sourceRegion;
    public int[] retainedSourceBounds;
    public int[] dimensions;
    public int[] groundAnchor;
    public int retainedPixels;
}

public static class BenchZenPoseExtraction {
    public static int Validate(string root, PoseRecord[] records) {
        int checkedPixels = 0;
        string folder = Path.Combine(root, "assets/characters/visitor-01");
        using (var source = new Bitmap(Path.Combine(root, "assets/characters/benchzen-character-round2c/character-master-reference.png"))) {
            foreach (PoseRecord record in records) {
                using (var pose = new Bitmap(Path.Combine(folder, record.file))) {
                    if (pose.Width != record.dimensions[0] || pose.Height != record.dimensions[1]) throw new Exception("Dimension mismatch: " + record.id);
                    int count = 0;
                    for (int y = 0; y < pose.Height; y++) for (int x = 0; x < pose.Width; x++) {
                        Color c = pose.GetPixel(x,y);
                        if (c.A == 0) continue;
                        if (x < 4 || y < 4 || x >= pose.Width-4 || y >= pose.Height-4) throw new Exception("Nontransparent border: " + record.id);
                        Color original = source.GetPixel(record.retainedSourceBounds[0]+x-4, record.retainedSourceBounds[1]+y-4);
                        if (original.ToArgb() != c.ToArgb()) throw new Exception("Invented pixel: " + record.id);
                        int max = Math.Max(c.R, Math.Max(c.G,c.B)), min = Math.Min(c.R, Math.Min(c.G,c.B));
                        if (c.A < 200 || max > 55 || max-min > 25) throw new Exception("Mask mismatch: " + record.id);
                        count++;
                    }
                    if (count != record.retainedPixels) throw new Exception("Pixel count mismatch: " + record.id);
                    if (record.groundAnchor[0] < 0 || record.groundAnchor[0] >= pose.Width || record.groundAnchor[1] != pose.Height-4) throw new Exception("Invalid anchor: " + record.id);
                    checkedPixels += count;
                }
            }
        }
        return checkedPixels;
    }

    public static PoseRecord[] Run(string root) {
        string folder = Path.Combine(root, "assets/characters/visitor-01");
        var records = new List<PoseRecord>();
        using (var source = new Bitmap(Path.Combine(root, "assets/characters/benchzen-character-round2c/character-master-reference.png"))) {
            // Regions are manually inspected isolation windows, not a uniform grid.
            records.Add(Extract(source, folder, "standing-front", new Rectangle(10, 108, 160, 479), 95));
            records.Add(Extract(source, folder, "standing-left", new Rectangle(173, 108, 98, 479), 225));
            records.Add(Extract(source, folder, "standing-rear", new Rectangle(273, 108, 151, 479), 346));
            records.Add(Extract(source, folder, "standing-right", new Rectangle(428, 108, 102, 479), 477));
            int[,] spans = {{729,844,789},{863,967,919},{988,1072,1039},{1106,1192,1158},
                           {1225,1331,1277},{1350,1456,1404},{1465,1581,1528},{1593,1714,1658}};
            for (int i = 0; i < 8; i++) {
                records.Add(Extract(source, folder, "walking-right-" + (i + 1).ToString("00"),
                    new Rectangle(spans[i,0], 10, spans[i,1] - spans[i,0], 168), spans[i,2]));
            }
        }
        using (var sheet = new Bitmap(760, 960))
        using (var graphics = Graphics.FromImage(sheet))
        using (var font = new Font(FontFamily.GenericSansSerif, 10)) {
            // Review artifact only: checkerboard and labels are never pose pixels.
            for (int y = 0; y < sheet.Height; y += 16)
                for (int x = 0; x < sheet.Width; x += 16)
                    graphics.FillRectangle(((x/16 + y/16)%2 == 0) ? Brushes.Gray : Brushes.DarkGray, x, y, 16, 16);
            for (int i = 0; i < records.Count; i++) {
                int x = (i % 4) * 190;
                int y = i < 4 ? 0 : 500 + ((i - 4) / 4) * 230;
                PoseRecord record = records[i];
                using (var pose = new Bitmap(Path.Combine(folder, record.file))) {
                    graphics.DrawImageUnscaled(pose, x + (190 - pose.Width)/2, y + 8);
                    graphics.FillRectangle(Brushes.Black, x, y + (i < 4 ? 485 : 200), 190, 24);
                    graphics.DrawString(record.id, font, Brushes.White, x + 4, y + (i < 4 ? 488 : 203));
                }
            }
            sheet.Save(Path.Combine(folder, "review/contact-sheet.png"), ImageFormat.Png);
        }
        return records.ToArray();
    }

    private static PoseRecord Extract(Bitmap source, string folder, string id, Rectangle region, int anchorX) {
        int w = region.Width, h = region.Height;
        var eligible = new bool[w*h];
        for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) {
            Color c = source.GetPixel(region.X + x, region.Y + y);
            int max = Math.Max(c.R, Math.Max(c.G, c.B));
            int min = Math.Min(c.R, Math.Min(c.G, c.B));
            eligible[y*w+x] = c.A >= 200 && max <= 55 && max-min <= 25;
        }
        // Keep the largest 8-connected dark component; discard detached fringe.
        var seen = new bool[w*h];
        var largest = new List<int>();
        for (int n = 0; n < eligible.Length; n++) {
            if (!eligible[n] || seen[n]) continue;
            var component = new List<int>();
            var queue = new Queue<int>(); queue.Enqueue(n); seen[n] = true;
            while (queue.Count > 0) {
                int p = queue.Dequeue(); component.Add(p);
                int px = p%w, py = p/w;
                for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++) {
                    int nx = px+dx, ny = py+dy;
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                    int next = ny*w+nx;
                    if (eligible[next] && !seen[next]) { seen[next] = true; queue.Enqueue(next); }
                }
            }
            if (component.Count > largest.Count) largest = component;
        }
        if (largest.Count == 0) throw new InvalidDataException("No character in " + id);
        int left=w, top=h, right=0, bottom=0;
        foreach (int p in largest) { left=Math.Min(left,p%w); right=Math.Max(right,p%w); top=Math.Min(top,p/w); bottom=Math.Max(bottom,p/w); }
        const int padding = 4;
        using (var output = new Bitmap(right-left+1+padding*2, bottom-top+1+padding*2, PixelFormat.Format32bppArgb)) {
            foreach (int p in largest) output.SetPixel(p%w-left+padding, p/w-top+padding, source.GetPixel(region.X+p%w, region.Y+p/w));
            output.Save(Path.Combine(folder, "poses/"+id+".png"), ImageFormat.Png);
            return new PoseRecord {id=id, file="poses/"+id+".png",
                sourceRegion=new int[]{region.X,region.Y,w,h},
                retainedSourceBounds=new int[]{region.X+left,region.Y+top,right-left+1,bottom-top+1},
                dimensions=new int[]{output.Width,output.Height},
                groundAnchor=new int[]{anchorX-region.X-left+padding,output.Height-padding}, retainedPixels=largest.Count};
        }
    }
}

// Local segmentation and edge matting of the supplied RGB image. No new artwork.
// Run from Windows PowerShell using built-in System.Drawing; see repair notes.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

public class SeatedRepairResult {
    public int width, height, opaque, partial, transparent, unchangedInterior;
    public int darkSourcePixels, darkSourcePixelsRetained, enclosedBackgroundRegions;
    public int repairedEdgePinholes;
    public int removedBackgroundSpeckPixels, removedBackgroundSpeckComponents;
    public double maxEdgeRecompositionError;
    public string worstEdge;
    public string enclosedRegionDetails;
    public int[] alphaBounds;
}

public static class BenchZenSeatedRepair {
    private static int Max(Color c) { return Math.Max(c.R, Math.Max(c.G,c.B)); }
    private static bool NearbyBackground(bool[] background,int x,int y,int w,int h) {
        for(int dy=-8;dy<=8;dy++)for(int dx=-8;dx<=8;dx++) {
            int nx=x+dx,ny=y+dy;
            if(nx>=0&&nx<w&&ny>=0&&ny<h&&background[ny*w+nx])return true;
        }
        return false;
    }
    private static Color Nearest(Color[] colors, bool[] eligible, int x, int y, int w, int h) {
        int best = Int32.MaxValue, selected = -1;
        // A narrow enclosed arm/body gap may contain only mixed edge pixels;
        // allow sampling the known pale background farther away in that case.
        foreach(int radius in new int[]{12,128}) {
            for (int dy=-radius; dy<=radius; dy++) for (int dx=-radius; dx<=radius; dx++) {
                int nx=x+dx, ny=y+dy, d=dx*dx+dy*dy;
                if (nx<0 || nx>=w || ny<0 || ny>=h || d>=best) continue;
                int p=ny*w+nx;
                if (eligible[p]) { best=d; selected=p; }
            }
            if(selected>=0)break;
        }
        if (selected<0) throw new InvalidDataException("Uncertain edge at "+x+","+y+"; extraction stopped.");
        return colors[selected];
    }

    public static SeatedRepairResult Run(string root) {
        string folder=Path.Combine(root,"assets/characters/visitor-01");
        string review=Path.Combine(folder,"review/round2f1");
        string sourcePath=Path.Combine(folder,"seated-rear.png.png");
        using(var sha=SHA256.Create()) {
            string hash=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(sourcePath))).Replace("-","");
            if(hash!="E5D5612F8D44EC4F2F3ABFE4CFBF8D2FE8DABCD7F42F92D3360D75887F86BD57")
                throw new InvalidDataException("Source changed; inspect before using this calibrated extraction.");
        }
        using (var source=new Bitmap(sourcePath)) {
            int w=source.Width,h=source.Height;
            if (w!=1024 || h!=1536) throw new InvalidDataException("Unexpected source dimensions.");
            var colors=new Color[w*h]; var binary=new bool[w*h];
            var core=new bool[w*h]; var foregroundSample=new bool[w*h]; var background=new bool[w*h];
            for(int y=0;y<h;y++) for(int x=0;x<w;x++) {
                int p=y*w+x; Color c=source.GetPixel(x,y);
                if(c.A!=255) throw new InvalidDataException("Source is no longer the inspected RGB artwork.");
                colors[p]=c; binary[p]=Max(c)<=127; background[p]=Max(c)>=246;
            }
            // A 5x5 erosion identifies confidently solid interior, NOT the output
            // silhouette. Thin hair and antialiased edges remain in the matte band.
            for(int y=2;y<h-2;y++) for(int x=2;x<w-2;x++) {
                int p=y*w+x; if(!binary[p]) continue;
                bool solid=true;
                for(int dy=-2;dy<=2 && solid;dy++) for(int dx=-2;dx<=2;dx++)
                    if(!binary[(y+dy)*w+x+dx]) { solid=false; break; }
                core[p]=solid;
                foregroundSample[p]=solid && Max(colors[p])<=40;
            }
            var result=new SeatedRepairResult { width=w,height=h };
            int left=w,top=h,right=0,bottom=0;
            var support=new bool[w*h];
            using(var output=new Bitmap(w,h,PixelFormat.Format32bppArgb)) {
                for(int y=0;y<h;y++) for(int x=0;x<w;x++) {
                    int p=y*w+x; Color c=colors[p];
                    if(Max(c)<=80) result.darkSourcePixels++;
                    if(background[p]) { result.transparent++; continue; }
                    Color repaired;
                    // Pale pixels embedded in tiny hair-edge clumps are still
                    // mixed background, even if a binary erosion reaches them.
                    bool copyInterior=core[p] && (Max(c)<=40 || !NearbyBackground(background,x,y,w,h));
                    if(copyInterior) { repaired=c; result.unchangedInterior++; }
                    else {
                        Color f=Nearest(colors,foregroundSample,x,y,w,h);
                        Color b=Nearest(colors,background,x,y,w,h);
                        double dr=b.R-f.R,dg=b.G-f.G,db=b.B-f.B;
                        double alpha=((b.R-c.R)*dr+(b.G-c.G)*dg+(b.B-c.B)*db)/(dr*dr+dg*dg+db*db);
                        int a=Math.Max(1,Math.Min(255,(int)Math.Round(alpha*255)));
                        // Recover soft coverage using local solid foreground color.
                        // Avoid unmixing noisy white RGB into a bright hair halo.
                        // If an edge sample is darker than the nearest interior
                        // sample, it is itself a better solid-color observation.
                        if(alpha>=1) { f=c; a=255; }
                        repaired=Color.FromArgb(a,f.R,f.G,f.B);
                        double qa=a/255.0;
                        double error=Math.Max(Math.Abs(c.R-(qa*f.R+(1-qa)*b.R)),
                            Math.Max(Math.Abs(c.G-(qa*f.G+(1-qa)*b.G)),Math.Abs(c.B-(qa*f.B+(1-qa)*b.B))));
                        if(error>result.maxEdgeRecompositionError) {
                            result.maxEdgeRecompositionError=error;
                            result.worstEdge=x+","+y+" C="+c+" F="+f+" B="+b;
                        }
                    }
                    output.SetPixel(x,y,repaired); support[p]=true;
                    if(repaired.A==255) result.opaque++; else result.partial++;
                    if(Max(c)<=80) result.darkSourcePixelsRetained++;
                    left=Math.Min(left,x);right=Math.Max(right,x);top=Math.Min(top,y);bottom=Math.Max(bottom,y);
                }
                result.alphaBounds=new int[]{left,top,right-left+1,bottom-top+1};
                if(result.darkSourcePixels!=result.darkSourcePixelsRetained) throw new InvalidDataException("Dark detail lost.");
                if(result.maxEdgeRecompositionError>8) throw new InvalidDataException("Matte uncertainty too high: "+result.maxEdgeRecompositionError+" "+result.worstEdge);
                // Source/background support is identical: no threshold removal
                // of dark pixels and no newly punched holes in the character.
                for(int p=0;p<support.Length;p++) if(support[p] == background[p]) throw new Exception("Support mismatch.");
                string details;
                CountHoles(support,w,h,out details);
                result.enclosedRegionDetails=details;
                result.repairedEdgePinholes=RepairPinholes(output,support,w,h);
                result.transparent-=result.repairedEdgePinholes;
                RemoveBackgroundSpecks(output,colors,support,w,h,result);
                result.transparent+=result.removedBackgroundSpeckPixels;
                // Pinholes use neighboring coverage and are counted by final alpha.
                result.opaque=0;result.partial=0;
                left=w;top=h;right=0;bottom=0;
                for(int y=0;y<h;y++)for(int x=0;x<w;x++) {
                    int a=output.GetPixel(x,y).A;
                    if(a==255)result.opaque++;else if(a>0)result.partial++;
                    if(a>0){left=Math.Min(left,x);right=Math.Max(right,x);top=Math.Min(top,y);bottom=Math.Max(bottom,y);}
                }
                result.alphaBounds=new int[]{left,top,right-left+1,bottom-top+1};
                result.enclosedBackgroundRegions=CountHoles(support,w,h,out details);
                if(result.enclosedBackgroundRegions!=0)throw new InvalidDataException("Unexpected enclosed background region; review required.");
                // Diagnostic candidate only; promotion follows visual validation.
                output.Save(Path.Combine(review,"candidate.png"),ImageFormat.Png);
                Comparison(source,output,review);
            }
            return result;
        }
    }

    private static void RemoveBackgroundSpecks(Bitmap output,Color[] source,bool[] support,int w,int h,SeatedRepairResult result) {
        var seen=new bool[support.Length];
        for(int n=0;n<support.Length;n++) {
            if(!support[n]||seen[n])continue;
            var region=new List<int>();var q=new Queue<int>();q.Enqueue(n);seen[n]=true;
            int minSource=255,maxAlpha=0;
            while(q.Count>0) {
                int p=q.Dequeue(),x=p%w,y=p/w;region.Add(p);
                minSource=Math.Min(minSource,Max(source[p]));maxAlpha=Math.Max(maxAlpha,output.GetPixel(x,y).A);
                for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++) {
                    int nx=x+dx,ny=y+dy;
                    if(nx<0||nx>=w||ny<0||ny>=h)continue;int np=ny*w+nx;
                    if(support[np]&&!seen[np]){seen[np]=true;q.Enqueue(np);}
                }
            }
            // Only detached, almost-white source noise with <=3% matte coverage.
            // Never discard a component containing any dark or substantial hair.
            if(region.Count<=16 && minSource>=240 && maxAlpha<=8) {
                foreach(int p in region){output.SetPixel(p%w,p/w,Color.FromArgb(0,0,0,0));support[p]=false;}
                result.removedBackgroundSpeckPixels+=region.Count;result.removedBackgroundSpeckComponents++;
            }
        }
    }

    private static int RepairPinholes(Bitmap output,bool[] support,int w,int h) {
        var seen=new bool[support.Length];int repaired=0;
        for(int n=0;n<support.Length;n++) {
            if(support[n]||seen[n])continue;
            bool border=false;var region=new List<int>();var q=new Queue<int>();q.Enqueue(n);seen[n]=true;
            while(q.Count>0) {
                int p=q.Dequeue(),x=p%w,y=p/w;region.Add(p);
                if(x==0||x==w-1||y==0||y==h-1)border=true;
                int[] next={x>0?p-1:-1,x<w-1?p+1:-1,y>0?p-w:-1,y<h-1?p+w:-1};
                foreach(int k in next)if(k>=0&&!support[k]&&!seen[k]){seen[k]=true;q.Enqueue(k);}
            }
            if(border||region.Count>8)continue;
            var replacement=new List<Color>();
            foreach(int p in region) {
                int x=p%w,y=p/w;double ar=0,ag=0,ab=0,aa=0;int count=0;
                for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++) {
                    int nx=x+dx,ny=y+dy;
                    if(nx<0||nx>=w||ny<0||ny>=h||!support[ny*w+nx])continue;
                    Color c=output.GetPixel(nx,ny);ar+=c.R*c.A;ag+=c.G*c.A;ab+=c.B*c.A;aa+=c.A;count++;
                }
                if(count==0)throw new InvalidDataException("Uncertain pinhole.");
                replacement.Add(Color.FromArgb(Math.Max(1,(int)Math.Round(aa/count)),(int)Math.Round(ar/aa),(int)Math.Round(ag/aa),(int)Math.Round(ab/aa)));
            }
            for(int i=0;i<region.Count;i++) { int p=region[i];output.SetPixel(p%w,p/w,replacement[i]);support[p]=true;repaired++; }
        }
        return repaired;
    }

    private static int CountHoles(bool[] support,int w,int h,out string details) {
        var seen=new bool[support.Length]; int enclosed=0;
        var regions=new List<string>();
        for(int n=0;n<support.Length;n++) {
            if(support[n]||seen[n])continue;
            bool border=false; int count=0,left=w,top=h,right=0,bottom=0; var q=new Queue<int>();q.Enqueue(n);seen[n]=true;
            while(q.Count>0) {
                int p=q.Dequeue(),x=p%w,y=p/w;
                count++;left=Math.Min(left,x);right=Math.Max(right,x);top=Math.Min(top,y);bottom=Math.Max(bottom,y);
                if(x==0||x==w-1||y==0||y==h-1)border=true;
                int[] next={x>0?p-1:-1,x<w-1?p+1:-1,y>0?p-w:-1,y<h-1?p+w:-1};
                foreach(int k in next)if(k>=0&&!support[k]&&!seen[k]){seen[k]=true;q.Enqueue(k);}
            }
            if(!border){enclosed++;regions.Add(count+" pixels at ["+left+","+top+","+(right-left+1)+","+(bottom-top+1)+"]");}
        }
        details=String.Join("; ",regions);
        return enclosed;
    }

    private static void Comparison(Bitmap source,Bitmap output,string review) {
        using(var sheet=new Bitmap(1368,1090))
        using(var g=Graphics.FromImage(sheet))
        using(var font=new Font(FontFamily.GenericSansSerif,11)) {
            g.Clear(Color.FromArgb(35,39,47));
            for(int row=0;row<2;row++)for(int col=0;col<2;col++) {
                int x=col*684,y=row*545;
                g.FillRectangle(row==0?Brushes.WhiteSmoke:Brushes.Black,x,y+32,684,513);
                g.DrawString((col==0?"ORIGINAL (opaque checkerboard)":"PROCESSED (genuine alpha)")+
                    (row==0?" / LIGHT":" / DARK"),font,Brushes.White,x+8,y+7);
                g.DrawImage(col==0?source:output,new Rectangle(x+171,y+32,342,513));
            }
            sheet.Save(Path.Combine(review,"comparison-light-dark.png"),ImageFormat.Png);
        }
        // Full-resolution critical edge crops (no smoothing/resampling).
        using(var sheet=new Bitmap(600,760))
        using(var g=Graphics.FromImage(sheet))
        using(var font=new Font(FontFamily.GenericSansSerif,10)) {
            g.Clear(Color.FromArgb(35,39,47));
            Rectangle[] crops={new Rectangle(370,48,300,310),new Rectangle(180,1210,300,360)};
            for(int row=0;row<2;row++)for(int col=0;col<2;col++) {
                int x=col*300,y=row*380;
                g.FillRectangle(col==0?Brushes.WhiteSmoke:Brushes.Black,x,y+24,300,356);
                g.DrawString((row==0?"HAIR":"SHOE / TROUSERS")+(col==0?" / LIGHT":" / DARK"),font,Brushes.White,x+5,y+3);
                Rectangle crop=crops[row];crop.Height=Math.Min(crop.Height,source.Height-crop.Y);
                g.DrawImage(output,new Rectangle(x,y+24,crop.Width,crop.Height),crop,GraphicsUnit.Pixel);
            }
            sheet.Save(Path.Combine(review,"edge-detail-light-dark.png"),ImageFormat.Png);
        }
    }
}

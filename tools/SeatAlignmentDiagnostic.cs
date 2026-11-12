// Offline diagnostic composites only. Never rewrites scene or character assets.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.IO;

public class SeatAlignmentMeasurements {
    public double[] baseline = {842,812,100};
    public int[][] localContour;
    public double centralClothY, minClothY, maxClothY, rejectedY800ClothY;
    public double gapAt812, gapAt800, seatToFrontRailStrip=4, projectedFeetY=881.19;
    public bool correctionApplied=false, physicalSupportPointVerified=false;
}

public static class BenchZenSeatDiagnostic {
    private static Color Cyan=Color.FromArgb(84,214,244), Red=Color.FromArgb(255,115,115),
        Amber=Color.FromArgb(255,200,87), Purple=Color.FromArgb(212,145,255);
    private static int[] xs={370,390,410,430,450,470,490,510,512,530,550,570,590,610,630,650};
    private static int[,] bands={{737,757},{761,778},{783,800},{816,824}};
    public static SeatAlignmentMeasurements Run(string root) {
        string folder=Path.Combine(root,"assets/characters/visitor-01");
        string review=Path.Combine(folder,"review/round2g");
        using(var background=new Bitmap(Path.Combine(root,"assets/images/paris/paris-clean.png")))
        using(var character=new Bitmap(Path.Combine(folder,"seated-rear-transparent.png")))
        using(var mask=new Bitmap(Path.Combine(root,"assets/images/paris/bench-preview/bench-center-occlusion.png"))) {
            var result=new SeatAlignmentMeasurements(); var contour=new List<int[]>();
            result.minClothY=Double.MaxValue;result.maxClothY=Double.MinValue;
            foreach(int x in xs) {
                int last=-1;
                for(int y=950;y<=1080;y++) if(character.GetPixel(x,y).A>=128)last=y;
                if(last<0 || last==1080)throw new InvalidDataException("Contact contour is ambiguous at x="+x);
                contour.Add(new int[]{x,last+1});
                double worldY=812+(last+1-1015)*0.17;
                result.minClothY=Math.Min(result.minClothY,worldY);result.maxClothY=Math.Max(result.maxClothY,worldY);
                if(x==512)result.centralClothY=worldY;
            }
            result.localContour=contour.ToArray();
            result.rejectedY800ClothY=result.centralClothY-12;
            result.gapAt812=Math.Max(0,806-result.centralClothY);
            result.gapAt800=Math.Max(0,806-result.rejectedY800ClothY);
            using(var baseline=Scene(background,character,mask,result.localContour,812,true))
            using(var off=Scene(background,character,mask,result.localContour,812,false))
            using(var rejected=Scene(background,character,mask,result.localContour,800,true)) {
                using(var sheet=new Bitmap(880,700))
                using(var g=Graphics.FromImage(sheet))
                using(var font=new Font(FontFamily.GenericSansSerif,11)) {
                    g.Clear(Color.FromArgb(17,21,28));
                    g.DrawString("R2G DIAGNOSTIC ONLY - X842 / Y812 / 100% - NO PLACEMENT APPROVED",font,Brushes.White,8,6);
                    Label(g,"Cyan: seat surface",8,28,Cyan);Label(g,"Red: cloth estimate",230,28,Red);
                    Label(g,"Amber: PNG bounds",450,28,Amber);Label(g,"Purple: mask bands",665,28,Purple);
                    g.DrawString("White: backrest. The red contour is an image edge, not an anatomical support plane.",font,Brushes.White,8,50);
                    g.DrawImage(baseline,new Rectangle(0,76,880,620),new Rectangle(630,610,440,310),GraphicsUnit.Pixel);
                    sheet.Save(Path.Combine(review,"baseline-alignment-guides.png"),ImageFormat.Png);
                }
                using(var sheet=new Bitmap(1290,250))
                using(var g=Graphics.FromImage(sheet))
                using(var font=new Font(FontFamily.GenericSansSerif,11)) {
                    g.Clear(Color.FromArgb(17,21,28));
                    Bitmap[] scenes={baseline,off,rejected};
                    string[] titles={"BASELINE Y812 / MASK ON","BASELINE Y812 / MASK OFF","REJECTED Y800 / MASK ON"};
                    string[] notes={"Cloth contour ~812, within seat 806-816.","Same position; only foreground copying off.","12px upward; contour ~800, 6px above seat."};
                    for(int i=0;i<3;i++) {
                        int x=i*430;g.DrawString(titles[i],font,Brushes.White,x+6,7);
                        g.DrawImage(scenes[i],new Rectangle(x+20,35,390,150),new Rectangle(780,790,130,50),GraphicsUnit.Pixel);
                        g.DrawString(notes[i],font,Brushes.White,x+6,195);
                    }
                    sheet.Save(Path.Combine(review,"seat-contact-closeups.png"),ImageFormat.Png);
                }
            }
            using(var sheet=new Bitmap(1140,540))
            using(var g=Graphics.FromImage(sheet))
            using(var font=new Font(FontFamily.GenericSansSerif,12))
            using(var pen=new Pen(Red,2)) {
                g.Clear(Color.WhiteSmoke);
                g.DrawString("SOURCE BACKSIDE CONTOUR - NOT A VERIFIED HIP JOINT / WEIGHT-BEARING POINT",font,Brushes.Black,8,6);
                g.DrawImage(character,new Rectangle(0,35,1140,480),new Rectangle(330,930,380,160),GraphicsUnit.Pixel);
                var pts=new List<PointF>();foreach(var p in result.localContour)pts.Add(new PointF((p[0]-330)*3,(p[1]-930)*3+35));
                g.DrawLines(pen,pts.ToArray());
                g.DrawEllipse(pen,(512-330)*3-8,(1015-930)*3+35-8,16,16);
                g.DrawString("Independent alpha >=128 scan: center lower clothing edge (512,1015); neighboring contour 1009-1018.",font,Brushes.Black,8,518);
                sheet.Save(Path.Combine(review,"source-contact-inspection.png"),ImageFormat.Png);
            }
            return result;
        }
    }

    private static void Label(Graphics g,string text,float x,float y,Color color) {
        using(var f=new Font(FontFamily.GenericSansSerif,10))using(var b=new SolidBrush(color))g.DrawString(text,f,b,x,y);
    }
    private static Bitmap Scene(Bitmap background,Bitmap character,Bitmap mask,int[][] contour,double hipY,bool occlusion) {
        var output=new Bitmap(background.Width,background.Height);
        using(var g=Graphics.FromImage(output)) {
            g.DrawImageUnscaled(background,0,0);
            g.InterpolationMode=InterpolationMode.HighQualityBicubic;
            float left=(float)(842-512*0.17),top=(float)(hipY-1015*0.17);
            g.DrawImage(character,new RectangleF(left,top,1024*0.17f,1536*0.17f));
            if(occlusion)g.DrawImageUnscaled(mask,690,737);
            using(var white=new Pen(Color.White,1))using(var purple=new Pen(Purple,1))
            using(var purpleFill=new SolidBrush(Color.FromArgb(30,Purple)))
            using(var cyan=new Pen(Cyan,1))using(var cyanFill=new SolidBrush(Color.FromArgb(56,Cyan)))
            using(var amber=new Pen(Amber,1))using(var red=new Pen(Red,1.5f)) {
                white.DashStyle=DashStyle.Dot;g.DrawRectangle(white,645,737,392,63);
                purple.DashStyle=DashStyle.Dash;g.DrawRectangle(purple,690,737,310,87);
                for(int i=0;i<4;i++){g.FillRectangle(purpleFill,690,bands[i,0],310,bands[i,1]-bands[i,0]);g.DrawRectangle(purple,690,bands[i,0],310,bands[i,1]-bands[i,0]);}
                Point[] board={new Point(675,806),new Point(1007,806),new Point(1013,816),new Point(670,816)};
                g.FillPolygon(cyanFill,board);g.DrawPolygon(cyan,board);
                amber.DashStyle=DashStyle.Dash;g.DrawRectangle(amber,left,top,1024*0.17f,1536*0.17f);
                var pts=new List<PointF>();foreach(var p in contour)pts.Add(new PointF((float)(842+(p[0]-512)*0.17),(float)(hipY+(p[1]-1015)*0.17)));
                g.DrawLines(red,pts.ToArray());g.FillEllipse(Brushes.Black,838,(float)hipY-4,8,8);g.DrawEllipse(red,838,(float)hipY-4,8,8);
                Label(g,"PNG bounds",left,top-14,Amber);
            }
        }
        return output;
    }
}

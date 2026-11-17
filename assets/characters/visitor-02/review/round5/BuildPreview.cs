using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public class R5Measurements {
 public int width,height,transparent,partial,opaque;
 public int[] alphaBounds,headBounds;
 public int contactY,soleY,shoulderWidth;
 public double scale,groundY,headY;
 public bool anatomicalContactVerified=false;
}
public static class R5Preview {
 static int[] Bounds(Bitmap b,int x0,int y0,int x1,int y1) {
  int l=x1,t=y1,r=-1,d=-1;
  for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(b.GetPixel(x,y).A>=128){l=Math.Min(l,x);t=Math.Min(t,y);r=Math.Max(r,x);d=Math.Max(d,y);}
  if(r<0)throw new Exception("Empty measurement ROI");return new[]{l,t,r-l+1,d-t+1};
 }
 public static R5Measurements Run(string root) {
  string dir=Path.Combine(root,"assets/characters/visitor-02/review/round5");
  using(var src=new Bitmap(Path.Combine(root,"assets/characters/visitor-02/source/visitor-02-seated-transparent.png")))
  using(var master=new Bitmap(Path.Combine(root,"assets/characters/visitor-02/source/approved-master-character.png")))
  using(var bg=new Bitmap(Path.Combine(root,"assets/images/paris/paris-clean.png"))) {
   var m=new R5Measurements{width=src.Width,height=src.Height};
   if(src.Width!=1048||src.Height!=1501)throw new Exception("Unexpected candidate size; review landmarks first");
   for(int y=0;y<src.Height;y++)for(int x=0;x<src.Width;x++){int a=src.GetPixel(x,y).A;if(a==0)m.transparent++;else if(a==255)m.opaque++;else m.partial++;}
   m.alphaBounds=Bounds(src,0,0,src.Width,src.Height);
   m.headBounds=Bounds(src,350,0,700,300);
   var shoulder=Bounds(src,200,398,850,403);m.shoulderWidth=shoulder[2];
   // Visible central underside of jeans, NOT a verified anatomical support point.
   var contact=Bounds(src,500,1000,541,1100);m.contactY=contact[1]+contact[3];
   m.soleY=m.alphaBounds[1]+m.alphaBounds[3];
   // Approximate approved-master rear hair envelope x870..1007, H=1000.
   m.scale=(138.0*600/1000)/m.headBounds[2];
   m.groundY=536+(m.soleY-m.contactY)*m.scale;
   m.headY=536+(m.headBounds[1]-m.contactY)*m.scale;
   using(var frame=new Bitmap(512,768,PixelFormat.Format32bppArgb)) {
    using(var g=Graphics.FromImage(frame)){g.Clear(Color.Transparent);g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.DrawImage(src,new RectangleF((float)(256-524*m.scale),(float)(536-m.contactY*m.scale),(float)(src.Width*m.scale),(float)(src.Height*m.scale)));}
    frame.Save(Path.Combine(dir,"seated-preview.png"),ImageFormat.Png);
    using(var overlay=new Bitmap(1672,941,PixelFormat.Format32bppArgb)) {
     int[][] solid={new[]{645,738,393,20},new[]{645,761,393,18},new[]{645,783,393,17},new[]{646,814,392,10}};
     int[][] dark={new[]{662,738,19,76},new[]{1004,738,17,76},new[]{660,824,36,70},new[]{990,824,44,70}};
     using(var og=Graphics.FromImage(overlay)){og.Clear(Color.Transparent);foreach(var r in solid)og.DrawImage(bg,new Rectangle(r[0],r[1],r[2],r[3]),new Rectangle(r[0],r[1],r[2],r[3]),GraphicsUnit.Pixel);}
     foreach(var r in dark)for(int y=r[1];y<r[1]+r[3];y++)for(int x=r[0];x<r[0]+r[2];x++){Color p=bg.GetPixel(x,y);if(Math.Max(p.R,Math.Max(p.G,p.B))<78)overlay.SetPixel(x,y,p);}
     overlay.Save(Path.Combine(dir,"bench-foreground.png"),ImageFormat.Png);
     using(var scene=new Bitmap(bg)) {
      using(var g=Graphics.FromImage(scene)){g.InterpolationMode=InterpolationMode.HighQualityBicubic;double s=280.0/600;g.DrawImage(frame,new RectangleF((float)(842-256*s),(float)(812-536*s),(float)(512*s),(float)(768*s)));g.DrawImageUnscaled(overlay,0,0);}
      scene.Save(Path.Combine(dir,"paris-seated-preview.png"),ImageFormat.Png);
      using(var sheet=new Bitmap(1640,920))using(var g=Graphics.FromImage(sheet))using(var font=new Font("Arial",12)) {
       g.Clear(Color.FromArgb(29,33,39));g.DrawString("ROUND 5 / UNAPPROVED - uniform scale only; contact is a clothing-edge estimate",font,Brushes.White,15,12);
       // Crop from opaque approved master, uniformly scale H1000 to H600.
       g.DrawImage(master,new RectangleF(20,60,216,614.4f),new RectangleF(760,0,360,1024),GraphicsUnit.Pixel);
       g.DrawString("Approved rear / H600\nOriginal backdrop retained",font,Brushes.White,15,840);
       using(var brush=new SolidBrush(Color.FromArgb(95,103,112)))g.FillRectangle(brush,250,50,512,768);
       g.DrawImageUnscaled(frame,250,50);
       g.DrawString("Seated / head envelope matched\nGold: y536 seat; cyan: y684 ground",font,Brushes.White,250,840);
       g.DrawLine(Pens.Gold,250,586,762,586);g.DrawLine(Pens.Cyan,250,734,762,734);
       g.DrawImage(scene,new Rectangle(790,50,840,700),new Rectangle(600,560,480,400),GraphicsUnit.Pixel);
       g.DrawString("Paris bench / foreground restored\nZoomed crop; browser has actual scene scale",font,Brushes.White,790,840);
       sheet.Save(Path.Combine(dir,"standing-seated-comparison.png"),ImageFormat.Png);
      }
     }
    }
   }
   return m;
  }
 }
}

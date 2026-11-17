using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;

public static class R7Perspective {
 public static void Run(string root) {
  string dir=Path.Combine(root,"assets/characters/visitor-02/review/round7");
  // Reuse the fixed Round 6 100% scene: no character transform or art change.
  using(var scene=new Bitmap(Path.Combine(root,"assets/characters/visitor-02/review/round6/scene-100.png")))
  using(var sheet=new Bitmap(1600,900))using(var g=Graphics.FromImage(sheet))
  using(var font=new Font("Arial",14))using(var small=new Font("Arial",12)) {
   g.Clear(Color.FromArgb(22,27,33));
   g.DrawString("ROUND 7 / FIXED X842 Y812 / 100% / PERSPECTIVE CHECK",font,Brushes.White,20,16);
   g.DrawString("4x close-up. Coordinates below are original scene pixels; no character scale or placement changed.",small,Brushes.White,20,49);
   // Scene x640..1040, y780..910. Magnify crop uniformly for inspection only.
   g.InterpolationMode=InterpolationMode.NearestNeighbor;
   g.DrawImage(scene,new Rectangle(0,105,1600,520),new Rectangle(640,780,400,130),GraphicsUnit.Pixel);
   Func<double,float> X=x=>(float)((x-640)*4);
   Func<double,float> Y=y=>(float)(105+(y-780)*4);
   using(var band=new SolidBrush(Color.FromArgb(35,80,220,255)))g.FillRectangle(band,X(785),Y(868),X(899)-X(785),Y(880)-Y(868));
   using(var cyan=new Pen(Color.Cyan,2))using(var gold=new Pen(Color.Gold,2))using(var red=new Pen(Color.Salmon,3))using(var gray=new Pen(Color.LightGray,2)) {
    cyan.DashStyle=DashStyle.Dash;g.DrawLine(cyan,X(785),Y(874),X(899),Y(874));
    gray.DashStyle=DashStyle.Dash;g.DrawLine(gray,X(645),Y(881),X(1038),Y(881));
    g.DrawLine(gold,X(645),Y(812),X(1038),Y(812));
    foreach(double x in new[]{812.0947265625,870.49884033203125})g.DrawLine(red,X(x-10),Y(871.07037353515625),X(x+10),Y(871.07037353515625));
    gold.DashStyle=DashStyle.Dash;
    g.DrawEllipse(gold,X(653),Y(881),26*4,12*4);g.DrawEllipse(gold,X(1003),Y(881),29*4,12*4);
   }
   g.DrawString("Seat target y812",small,Brushes.Gold,30,Y(812)-25);
   g.DrawString("Boot soles y871.07 (measured)",small,Brushes.Salmon,585,645);
   g.DrawString("Cyan band y868-880 / centre y874: ILLUSTRATIVE LOCAL DEPTH ESTIMATE, not a measured plane",font,Brushes.Cyan,20,690);
   g.DrawString("Gold ellipses: visually estimated near bench-foot contact around y883-890. Gray: documented y881 reference.",small,Brushes.White,20,732);
   g.DrawString("Assumption: the sitter's boots are farther away than the near bench feet; a 7-19px depth offset from y887 is plausible.",small,Brushes.White,20,773);
   g.DrawString("Pavement continues under the boots. Perspective can explain the apparent gap; exact depth and physical contact remain unverified.",small,Brushes.White,20,814);
   g.DrawString("No contact shadow was added. Retain the baseline for review; do not change artwork solely to meet a horizontal guide.",small,Brushes.White,20,855);
   sheet.Save(Path.Combine(dir,"bench-perspective-closeup.png"),ImageFormat.Png);
  }
 }
}

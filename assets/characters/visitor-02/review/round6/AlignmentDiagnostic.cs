using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

public class R6Row {
 public int percent;
 public double sourceToScene,headTopY,headWidth,estimatedHeadHeight,shoulderEnvelopeWidth;
 public double[] contact,leftSole,rightSole;
 public double leftGap,rightGap;
}
public class R6Result {
 public double[] sourceContact={524,1046},frameContact={256,536},sceneContact={842,812};
 public int groundY=881,seatY=812;
 public int[] headBounds,shoulderBounds,leftBootBounds,rightBootBounds;
 public int estimatedHairBaseY=296,estimatedHairBaseUncertainty=8;
 public int[][] contactContour;
 public double[] thresholdLevels={32,128,224};
 public int[][] sourceBootSoleSensitivity;
 public int[] registeredLeftBootBounds,registeredRightBootBounds;
 public R6Row[] comparisons;
 public bool anatomicalContactVerified=false;
}
public static class R6Diagnostic {
 static int[] Bounds(Bitmap b,int x0,int y0,int x1,int y1,int threshold) {
  int l=x1,t=y1,r=-1,d=-1;
  for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(b.GetPixel(x,y).A>=threshold){l=Math.Min(l,x);t=Math.Min(t,y);r=Math.Max(r,x);d=Math.Max(d,y);}
  if(r<0)throw new Exception("Empty ROI");return new[]{l,t,r-l+1,d-t+1};
 }
 // Screen-left/right boots; bottom boundary = lowest alpha>=threshold pixel + 1.
 static double[] Sole(Bitmap b,int[] box) {
  int end=box[1]+box[3],l=b.Width,r=-1;
  for(int y=end-4;y<end;y++)for(int x=box[0];x<box[0]+box[2];x++)if(b.GetPixel(x,y).A>=128){l=Math.Min(l,x);r=Math.Max(r,x);}
  return new[]{(l+r+1)/2.0,(double)end};
 }
 static PointF Map(double x,double y,double s){return new PointF((float)(842+(x-524)*s),(float)(812+(y-1046)*s));}
 static void Cross(Graphics g,Pen p,float x,float y){g.DrawLine(p,x-5,y,x+5,y);g.DrawLine(p,x,y-5,x,y+5);}
 public static R6Result Run(string root) {
  string dir=Path.Combine(root,"assets/characters/visitor-02/review/round6");
  using(var src=new Bitmap(Path.Combine(root,"assets/characters/visitor-02/source/visitor-02-seated-transparent.png")))
  using(var frame=new Bitmap(Path.Combine(root,"assets/characters/visitor-02/review/round5/seated-preview.png")))
  using(var bg=new Bitmap(Path.Combine(root,"assets/images/paris/paris-clean.png")))
  using(var fg=new Bitmap(Path.Combine(root,"assets/characters/visitor-02/review/round5/bench-foreground.png"))) {
   var result=new R6Result();
   result.headBounds=Bounds(src,350,0,700,300,128);
   result.shoulderBounds=Bounds(src,200,398,850,403,128);
   result.leftBootBounds=Bounds(src,150,1300,420,1490,128);
   result.rightBootBounds=Bounds(src,620,1300,880,1490,128);
   result.registeredLeftBootBounds=Bounds(frame,100,610,240,710,128);
   result.registeredRightBootBounds=Bounds(frame,270,610,390,710,128);
   var contour=new List<int[]>();
   for(int x=500;x<=540;x+=5){int[] b=Bounds(src,x,1000,x+1,1100,128);contour.Add(new[]{x,b[1]+b[3]});}
   result.contactContour=contour.ToArray();
   var sensitivity=new List<int[]>();
   foreach(int a in new[]{32,128,224}){int[] l=Bounds(src,150,1300,420,1490,a),r=Bounds(src,620,1300,880,1490,a);sensitivity.Add(new[]{a,l[1]+l[3],r[1]+r[3]});}
   result.sourceBootSoleSensitivity=sensitivity.ToArray();
   double[] ls=Sole(src,result.leftBootBounds),rs=Sole(src,result.rightBootBounds);
   var rows=new List<R6Row>();
   using(var sheet=new Bitmap(1600,1000))using(var sg=Graphics.FromImage(sheet))using(var font=new Font("Arial",13)) {
    sg.Clear(Color.FromArgb(22,27,33));
    sg.DrawString("ROUND 6 / SEAT HELD FIXED / UNAPPROVED DIAGNOSTIC",font,Brushes.White,20,12);
    sg.DrawString("Cyan: documented seat y812 | Gold cross: jeans underside anchor | Green: ground y881 | Red: boot soles / gaps",font,Brushes.White,20,38);
    int panel=0;
    foreach(int percent in new[]{100,103}) {
     double s=(0.3172413793103448)*(280.0/600)*(percent/100.0);
     PointF l=Map(ls[0],ls[1],s),r=Map(rs[0],rs[1],s),head=Map(524,result.headBounds[1],s);
     var row=new R6Row{percent=percent,sourceToScene=s,headTopY=head.Y,headWidth=result.headBounds[2]*s,estimatedHeadHeight=(296-result.headBounds[1])*s,shoulderEnvelopeWidth=result.shoulderBounds[2]*s,contact=new[]{842.0,812.0},leftSole=new[]{(double)l.X,(double)l.Y},rightSole=new[]{(double)r.X,(double)r.Y},leftGap=881-l.Y,rightGap=881-r.Y};rows.Add(row);
     using(var scene=new Bitmap(bg))using(var g=Graphics.FromImage(scene)) {
      // Same existing registered frame and display transform as Round 5.
      double fs=280.0/600*(percent/100.0);
      g.InterpolationMode=InterpolationMode.HighQualityBicubic;
      g.DrawImage(frame,new RectangleF((float)(842-256*fs),(float)(812-536*fs),(float)(512*fs),(float)(768*fs)));
      g.DrawImageUnscaled(fg,0,0);
      scene.Save(Path.Combine(dir,"scene-"+percent+".png"),ImageFormat.Png);
      using(var cyan=new Pen(Color.Cyan,0.8f))using(var gold=new Pen(Color.Gold,1))using(var green=new Pen(Color.LimeGreen,0.8f))using(var red=new Pen(Color.Salmon,1))using(var white=new Pen(Color.White,0.7f)) {
       g.DrawLine(cyan,645,812,1038,812);g.DrawLine(green,645,881,1038,881);Cross(g,gold,842,812);
       white.DashStyle=DashStyle.Dash;g.DrawLine(white,842,650,842,900);
       foreach(var sole in new[]{l,r}){g.DrawLine(red,sole.X-10,sole.Y,sole.X+10,sole.Y);g.DrawLine(red,sole.X,sole.Y,sole.X,881);}
       // Estimated head envelope ends at manually marked hair base, not ROI y300.
       PointF hb=Map(result.headBounds[0],result.headBounds[1],s);
       g.DrawRectangle(white,hb.X,hb.Y,(float)row.headWidth,(float)row.estimatedHeadHeight);
       PointF sh=Map(result.shoulderBounds[0],400,s);g.DrawLine(white,sh.X,sh.Y,sh.X+(float)row.shoulderEnvelopeWidth,sh.Y);
      }
      scene.Save(Path.Combine(dir,"annotated-scene-"+percent+".png"),ImageFormat.Png);
      int offset=panel*800;
      sg.DrawString(percent+"% | contact (842,812) | head top y"+row.headTopY.ToString("F2"),font,Brushes.White,offset+20,82);
      // Uniform 2x scene crop: x640..1040, y640..910.
      sg.DrawImage(scene,new Rectangle(offset,120,800,540),new Rectangle(640,640,400,270),GraphicsUnit.Pixel);
      string text=String.Format("Head W {0:F2} / estimated H {1:F2} scene px\nShoulder envelope W {2:F2} (not joint breadth)\nLeft sole ({3:F2}, {4:F2}) / gap {5:F2}px\nRight sole ({6:F2}, {7:F2}) / gap {8:F2}px\nContact-to-lowest-sole {9:F2}px / required 69px",row.headWidth,row.estimatedHeadHeight,row.shoulderEnvelopeWidth,l.X,l.Y,row.leftGap,r.X,r.Y,row.rightGap,Math.Max(l.Y,r.Y)-812);
      sg.DrawString(text,font,Brushes.White,offset+20,685);
     }
     panel++;
    }
    sg.DrawString("Measures: source alpha >=128 transformed analytically; raster rendering can differ <1 scene px. Left/right are screen sides.",font,Brushes.White,20,890);
    sg.DrawString("Head height uses manually estimated hair base y296 (+/-8 source px). Anchor is clothing edge, NOT verified skeletal support.",font,Brushes.White,20,922);
    sg.DrawString("Both scenes retain original foreground bench pixels. No character or scene artwork changed.",font,Brushes.White,20,954);
    sheet.Save(Path.Combine(dir,"alignment-100-vs-103.png"),ImageFormat.Png);
   }
   result.comparisons=rows.ToArray();return result;
  }
 }
}

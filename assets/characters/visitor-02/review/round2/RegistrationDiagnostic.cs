using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Linq;

public class V2Pose {
 public string id, source, method, note;
 public int[] bounds, headBounds;
 public double hipX, groundY, headY, shoulderY, shoulderWidth, neckY, hemY, torsoProjection, legPolyline, scale, dx, dy;
 public double[] knee, ankle, hipJoint;
 public double[][] contacts;
 public double[] seat;
 public double[] registeredHead, registeredFeet, registeredSeat;
 public double[] seatThresholdSensitivity;
 public double[] manualSupportingBootCentreX, registeredBootCentreOffsetX;
 public double registrationMargin;
}
public static class V2Registration {
 static string root, output;
 static Bitmap walk, rear;
 static int W,H;
 static int[] labels;
 static List<int[]> components = new List<int[]>();
 static Font font = new Font("Arial",18), small=new Font("Arial",13);
 static Pen cyan=new Pen(Color.Cyan,2), gold=new Pen(Color.Gold,2), red=new Pen(Color.OrangeRed,2), green=new Pen(Color.LimeGreen,2), white=new Pen(Color.White,1);
 static int Max(Color c) {return Math.Max(c.R,Math.Max(c.G,c.B));}
 static bool Fore(Bitmap b,int x,int y,int id) {return b==walk ? labels[y*W+x]==id : Max(b.GetPixel(x,y))>=40;}
 static int[] Box(Bitmap b,int id,int x0,int y0,int x1,int y1) {
  int l=x1,t=y1,r=x0,bot=y0;
  for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)if(Fore(b,x,y,id)){l=Math.Min(l,x);r=Math.Max(r,x);t=Math.Min(t,y);bot=Math.Max(bot,y);}
  return new int[]{l,t,r-l+1,bot-t+1};
 }
 static void Label() {
  W=walk.Width;H=walk.Height;labels=new int[W*H];int id=0;
  var q=new Queue<int>();
  for(int y=20;y<625;y++)for(int x=0;x<W;x++) {
   int p=y*W+x;if(labels[p]!=0||walk.GetPixel(x,y).A<128)continue;
   id++;labels[p]=id;q.Enqueue(p);int l=x,r=x,t=y,b=y,n=0;
   while(q.Count>0){int v=q.Dequeue(),vx=v%W,vy=v/W;n++;l=Math.Min(l,vx);r=Math.Max(r,vx);t=Math.Min(t,vy);b=Math.Max(b,vy);
    for(int yy=Math.Max(20,vy-1);yy<=Math.Min(624,vy+1);yy++)for(int xx=Math.Max(0,vx-1);xx<=Math.Min(W-1,vx+1);xx++){
     int k=yy*W+xx;if(labels[k]==0&&walk.GetPixel(xx,yy).A>=128){labels[k]=id;q.Enqueue(k);}
    }
   }
   if(n>10000)components.Add(new int[]{id,l,t,r-l+1,b-t+1});
  }
  components=components.OrderBy(c=>c[1]).ToList();if(components.Count!=8)throw new Exception("Expected eight walk components");
 }
 // Visible bottom-edge support band within a manually selected boot ROI.
 // Returns representative contact x/y and edge-band x extent; not the foot centre.
 static double[] Contact(Bitmap b,int id,int[] roi) {
  int bottom=-1;
  for(int y=roi[1];y<=roi[3];y++)for(int x=roi[0];x<=roi[2];x++)if(Fore(b,x,y,id))bottom=Math.Max(bottom,y);
  if(bottom<0)throw new Exception("Empty foot ROI");
  int l=roi[2],r=roi[0];
  for(int y=bottom-2;y<=bottom;y++)for(int x=roi[0];x<=roi[2];x++)if(Fore(b,x,y,id)){l=Math.Min(l,x);r=Math.Max(r,x);}
  return new double[]{(l+r)/2.0,bottom,l,r};
 }
 static double Distance(double[] a,double[] b){return Math.Sqrt(Math.Pow(a[0]-b[0],2)+Math.Pow(a[1]-b[1],2));}
 static void Measure(V2Pose p,Bitmap b,int id,int[] roi,int headDepth) {
  p.bounds=Box(b,id,roi[0],roi[1],roi[2],roi[3]);p.headY=p.bounds[1];
  p.headBounds=Box(b,id,p.bounds[0],(int)p.headY,p.bounds[0]+p.bounds[2]-1,(int)p.headY+headDepth);
  var shoulder=Box(b,id,p.bounds[0],(int)p.shoulderY-1,p.bounds[0]+p.bounds[2]-1,(int)p.shoulderY+1);
  p.shoulderWidth=shoulder[2];p.torsoProjection=p.hemY-p.neckY;
  if(p.knee!=null)p.legPolyline=Distance(p.hipJoint,p.knee)+Distance(p.knee,p.ankle);
 }
 static void Register(V2Pose p,double scale) {
  p.scale=scale;p.dx=256-scale*p.hipX;p.dy=684-scale*p.groundY;
  p.registeredHead=new double[]{p.headBounds[2]*scale, p.headY*scale+p.dy};
  p.registeredFeet=p.contacts.Select(c=>256+(c[0]-p.hipX)*scale).ToArray();
  if(p.manualSupportingBootCentreX!=null)p.registeredBootCentreOffsetX=p.manualSupportingBootCentreX.Select(x=>(x-p.hipX)*scale).ToArray();
  if(p.seat!=null)p.registeredSeat=new double[]{256+(p.seat[0]-p.hipX)*scale,684+(p.seat[1]-p.groundY)*scale};
  double left=p.bounds[0]*scale+p.dx,right=(p.bounds[0]+p.bounds[2]-1)*scale+p.dx;
  p.registrationMargin=Math.Min(left,511-right);
  if(p.registrationMargin<12)throw new Exception("Canvas clips "+p.id);
 }
 static PointF Pt(V2Pose p,double x,double y){return new PointF((float)(x*p.scale+p.dx),(float)(y*p.scale+p.dy));}
 static void Cross(Graphics g,Pen pen,float x,float y){g.DrawLine(pen,x-7,y,x+7,y);g.DrawLine(pen,x,y-7,x,y+7);}
 static void Mark(Graphics g,V2Pose p,bool source) {
  Func<double,double,PointF> map=(x,y)=> source?new PointF((float)x,(float)y):Pt(p,x,y);
  PointF a=map(p.bounds[0],p.bounds[1]),z=map(p.bounds[0]+p.bounds[2],p.bounds[1]+p.bounds[3]);
  g.DrawRectangle(cyan,a.X,a.Y,z.X-a.X,z.Y-a.Y);
  foreach(var c in p.contacts){var q=map(c[0],c[1]);Cross(g,red,q.X,q.Y);}
  var hip=map(p.hipX,p.hemY);Cross(g,gold,hip.X,hip.Y);
  var ground=map(p.hipX,p.groundY);Cross(g,green,ground.X,ground.Y);
  if(p.seat!=null){var s=map(p.seat[0],p.seat[1]);Cross(g,gold,s.X,s.Y);}
 }
 static Bitmap Isolated(V2Pose p) {
  var b=new Bitmap(p.bounds[2],p.bounds[3],PixelFormat.Format32bppArgb);int id=components[int.Parse(p.id.Substring(1))-1][0];
  for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++)if(labels[(y+p.bounds[1])*W+x+p.bounds[0]]==id)b.SetPixel(x,y,walk.GetPixel(x+p.bounds[0],y+p.bounds[1]));
  return b;
 }
 static void Sheet(List<V2Pose> poses,string file,int columns,string title) {
  int cellH=900,header=100;
  using(var b=new Bitmap(columns*512,header+((poses.Count+columns-1)/columns)*cellH))using(var g=Graphics.FromImage(b)) {
   g.Clear(Color.FromArgb(37,43,51));g.DrawString(title,font,Brushes.White,12,8);
   g.DrawString("Cyan bounds | red sole edge | green ground origin | gold hip/seat | white rear foot targets",small,Brushes.White,12,40);
   g.DrawString("Uniform scale + translation only. Ground y684; seat y536. Diagnostic copies, not approved sprites.",small,Brushes.White,12,65);
   for(int i=0;i<poses.Count;i++){
    var p=poses[i];int ox=(i%columns)*512,oy=header+(i/columns)*cellH;
    var save=g.Save();g.TranslateTransform(ox,oy);
    g.FillRectangle(Brushes.SlateGray,0,0,512,768);
    if(p.id.StartsWith("W")){
     using(var cut=Isolated(p)){g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.DrawImage(cut,(float)(p.bounds[0]*p.scale+p.dx),(float)(p.bounds[1]*p.scale+p.dy),(float)(cut.Width*p.scale),(float)(cut.Height*p.scale));}
    } else {
     var rect=new Rectangle(p.bounds[0]-2,p.bounds[1]-2,p.bounds[2]+4,p.bounds[3]+4);
     // Keep opaque source rectangle. Do not manufacture a matte from dark pixels.
     g.DrawImage(rear,new RectangleF((float)(rect.X*p.scale+p.dx),(float)(rect.Y*p.scale+p.dy),(float)(rect.Width*p.scale),(float)(rect.Height*p.scale)),rect,GraphicsUnit.Pixel);
    }
    g.DrawRectangle(white,12,12,487,743);g.DrawLine(green,0,684,511,684);g.DrawLine(gold,0,536,511,536);g.DrawLine(cyan,256,0,256,767);
    Cross(g,gold,256,536);Cross(g,green,256,684);
    if(p.id.StartsWith("R")){Cross(g,white,196,684);Cross(g,white,316,684);}
    Mark(g,p,false);
    g.DrawString(p.id+"  scale "+p.scale.ToString("F4"),font,Brushes.White,12,780);
    g.DrawString("Head y "+p.registeredHead[1].ToString("F1")+" | shoulder "+(p.shoulderWidth*p.scale).ToString("F1"),small,Brushes.White,12,813);
    g.DrawString("Sole x "+string.Join(" / ",p.registeredFeet.Select(v=>v.ToString("F1"))),small,Brushes.White,12,840);
    g.DrawString(p.seat==null?"Hip cross = shirt-hem centre (estimated)":"Seat y "+p.registeredSeat[1].ToString("F1")+" | target 536",small,Brushes.White,12,867);
    g.Restore(save);
   }
   b.Save(Path.Combine(output,file),ImageFormat.Png);
  }
 }
 public static V2Pose[] Run(string folder) {
  root=folder;output=Path.Combine(root,"review/round2");
  walk=new Bitmap(Path.Combine(root,"source/walk-cycle-candidate.png"));rear=new Bitmap(Path.Combine(root,"source/rear-sit-motion-reference.png"));Label();
  var all=new List<V2Pose>();var ws=new List<V2Pose>();
  double[] hips={169,441,708,976,1242,1512,1789,2055};
  double[] neckWalk={126,126,126,127,130,129,131,129};
  double[] hemWalk={330,331,331,330,334,334,335,333};
  // Manual heel-to-toe bounding midpoints, +/-5 source px. Keep distinct from support-band points.
  double[][] bootCentre={new double[]{71.5,294.5},new double[]{476.5},new double[]{716},new double[]{957},new double[]{1126,1341.5},new double[]{1456.5},new double[]{1838.5},new double[]{2044}};
  double[][] knee={new double[]{234,466},new double[]{462,463},new double[]{718,459},new double[]{1008,447},new double[]{1297,456},new double[]{1585,453},new double[]{1796,457},new double[]{2100,445}};
  double[][] ankle={new double[]{268,569},new double[]{449,572},new double[]{692,571},new double[]{939,543},new double[]{1327,581},new double[]{1619,540},new double[]{1709,533},new double[]{2033,547}};
  int[][][] boots={
   new int[][]{new int[]{15,570,130,624},new int[]{240,580,350,624}},
   new int[][]{new int[]{424,580,532,624}},
   new int[][]{new int[]{664,580,775,624}},
   new int[][]{new int[]{905,600,1010,624}},
   new int[][]{new int[]{1080,565,1177,624},new int[]{1280,580,1398,624}},
   new int[][]{new int[]{1400,580,1514,624}},
   new int[][]{new int[]{1785,580,1898,624}},
   new int[][]{new int[]{1989,603,2106,624}}
  };
  for(int i=0;i<8;i++){
   var c=components[i];var p=new V2Pose{id="W"+(i+1).ToString("D2"),source="walk-cycle-candidate.png",method="alpha>=128 connected component; boot ROIs; manual shirt/limb landmarks",hipX=hips[i],shoulderY=160,neckY=neckWalk[i],hemY=hemWalk[i],hipJoint=new double[]{hips[i],350},knee=knee[i],ankle=ankle[i],note="Contact is visible lowest sole band, NOT foot centre; left/right limb identity uncertain. Manual neck/hem +/-4, hip centre +/-5, joint estimates +/-8..12 source px. Leg polyline is projected evidence, not true bone length."};
   Measure(p,walk,c[0],new int[]{c[1],c[2],c[1]+c[3]-1,c[2]+c[4]-1},82);
   p.manualSupportingBootCentreX=bootCentre[i];
   p.contacts=boots[i].Select(r=>Contact(walk,c[0],r)).ToArray();p.groundY=p.contacts.Max(v=>v[1]);ws.Add(p);
  }
  double walkScale=600.0/(ws[2].groundY-ws[2].headY);
  foreach(var p in ws){Register(p,walkScale);all.Add(p);}
  var rs=new List<V2Pose>();
  int[] ids={1,4,5,6,7,8};double[] cx={208,972,1218,1471,1722,1978};double[] neck={89,101,124,144,158,160},hem={209,208,224,254,274,277};
  int[][] regions={new int[]{145,42,272,332},new int[]{905,44,1040,332},new int[]{1148,73,1288,332},new int[]{1398,93,1545,332},new int[]{1655,105,1790,332},new int[]{1910,105,2048,332}};
  int[][][] rearBoots={
   new int[][]{new int[]{150,300,195,332},new int[]{222,300,268,332}},
   new int[][]{new int[]{940,305,973,332},new int[]{974,305,1010,332}},
   new int[][]{new int[]{1170,305,1211,332},new int[]{1220,305,1266,332}},
   new int[][]{new int[]{1423,305,1465,332},new int[]{1480,305,1526,332}},
   new int[][]{new int[]{1670,305,1712,332},new int[]{1736,305,1780,332}},
   new int[][]{new int[]{1925,305,1968,332},new int[]{1991,305,2038,332}}
  };
  for(int i=0;i<6;i++){
   var p=new V2Pose{id="R"+ids[i].ToString("D2"),source="rear-sit-motion-reference.png (top row)",method="RGB max>=40 bounds/sole within ROI; seat contour max>=8; opaque source retained; manual neck/hem",hipX=cx[i],neckY=neck[i],hemY=hem[i],shoulderY=neck[i]+15,note="RGB threshold is measurement proxy, not a matte. Contours +/-2..4 px; clothing landmarks +/-4 px; R01 oblique view excluded from width test. True seated leg length cannot be recovered from this rear projection."};
   Measure(p,rear,0,regions[i],33);p.contacts=rearBoots[i].Select(r=>Contact(rear,0,r)).ToArray();p.groundY=p.contacts.Max(v=>v[1]);
   if(ids[i]>=6){
    var results=new List<double>();
    foreach(int threshold in new int[]{6,8,12}){
     var ys=new List<int>();for(int x=(int)p.hipX-5;x<=(int)p.hipX+5;x++){int last=-1;for(int y=220;y<300;y++)if(Max(rear.GetPixel(x,y))>=threshold)last=y;if(last>=0)ys.Add(last);}
     if(ys.Count==0)throw new Exception("Seat contour not observable: "+p.id);
     ys.Sort();results.Add(ys[ys.Count/2]);
    }
    p.seatThresholdSensitivity=results.ToArray();p.seat=new double[]{p.hipX,results[1]};
   }
   rs.Add(p);
  }
  double rearScale=600.0/(rs[1].groundY-rs[1].headY);foreach(var p in rs){Register(p,rearScale);all.Add(p);}
  Sheet(ws,"registered-walk.png",4,"WALK / one scale derived from W03, hips centred, visible soles at ground");
  Sheet(rs,"registered-rear.png",3,"REAR / one scale derived from R04; opaque source rectangles preserved");
  using(var b=new Bitmap(2172,1120))using(var g=Graphics.FromImage(b)){
   g.Clear(Color.FromArgb(37,43,51));g.DrawString("SOURCE LANDMARKS / original pixel coordinates",font,Brushes.White,12,8);
   g.DrawString("Cyan bounds | red sole-band points | gold shirt-hem centre / seated underside | green ground origin",small,Brushes.White,12,40);
   g.FillRectangle(Brushes.SlateGray,0,75,2172,650);g.DrawImageUnscaled(walk,0,75);var save=g.Save();g.TranslateTransform(0,75);
   foreach(var p in ws){Mark(g,p,true);g.DrawString(p.id,small,Brushes.Cyan,p.bounds[0],p.bounds[1]-25);}g.Restore(save);
   g.FillRectangle(Brushes.Black,0,760,2172,360);g.DrawImage(rear,new Rectangle(0,760,2172,360),new Rectangle(0,0,2172,360),GraphicsUnit.Pixel);
   save=g.Save();g.TranslateTransform(0,760);foreach(var p in rs)Mark(g,p,true);g.Restore(save);
   b.Save(Path.Combine(output,"annotated-source.png"),ImageFormat.Png);
  }
  walk.Dispose();rear.Dispose();return all.ToArray();
 }
}

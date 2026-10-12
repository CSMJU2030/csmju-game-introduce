using UnityEngine;

// UVs belong to the complete edited campus image, not a separate overlay.
public static class CampusStadiumLayout
{
    public static readonly Rect MapUv=new Rect(.615f,.07f,.385f,.40f);
    private static Vector2 Pixel(float x,float y)=>new Vector2(x/1672f,1-y/941f);
    private static Rect Box(float x,float y,float w,float h)=>new Rect(x/1672f,1-(y+h)/941f,w/1672f,h/941f);
    // Trace the visible inner edge of the outer running track. Its perspective
    // slopes down to the right, so an axis-aligned ellipse would block the lanes.
    private static readonly Vector2[] Track={
        Pixel(1048,680),Pixel(1051,658),Pixel(1068,637),Pixel(1096,619),
        Pixel(1135,605),Pixel(1176,600),Pixel(1211,602),Pixel(1370,630),
        Pixel(1538,659),Pixel(1591,675),Pixel(1630,698),Pixel(1651,724),
        Pixel(1658,750),Pixel(1648,782),Pixel(1626,809),Pixel(1593,829),
        Pixel(1552,839),Pixel(1514,845),Pixel(1466,845),Pixel(1370,826),
        Pixel(1202,796),Pixel(1119,771),Pixel(1084,752),Pixel(1061,727),Pixel(1052,706)
    };
    public static readonly Rect[] Solids={
        Box(1281,507,278,105), // Covered stand: roof, steps and supports.
        Box(1136,647,39,42),Box(1538,716,40,49), // Visible goal frames.
        Box(1158,513,29,81),Box(1577,548,23,88), // Northern floodlights.
        Box(1624,785,24,75),Box(975,669,23,78), // Southern and western lamps.
        Box(1190,771,20,62),Box(1367,809,23,65)
    };
    public static readonly Vector2[] Targets={Pixel(1083,705),Pixel(1350,716),Pixel(1230,614),Pixel(1320,803)};
    public static readonly Vector2 HostUv=new Vector2(.681129f,.3266094f);
    public static bool IsSolid(Vector2 uv)
    {
        foreach(var r in Solids)if(r.Contains(uv))return true;
        return false;
    }
    public static bool OnTrack(Vector2 uv)
    {
        bool inside=false;
        for(int i=0,j=Track.Length-1;i<Track.Length;j=i++) {
            var a=Track[i];var b=Track[j];
            if((a.y>uv.y)!=(b.y>uv.y) && uv.x<(b.x-a.x)*(uv.y-a.y)/(b.y-a.y)+a.x)inside=!inside;
        }
        return inside;
    }
    public static Vector2 At(Vector2 uv,Vector2 min,Vector2 max)=>min+Vector2.Scale(uv,max-min);
}

using UnityEngine;

public sealed class CampusGroundMask : ScriptableObject
{
    public int width, height;
    public byte[] blocked;
    public bool Blocks(Vector2 point, Vector2 min, Vector2 max) {
        if(point.x<min.x || point.y<min.y || point.x>=max.x || point.y>=max.y)return true;
        int x=Mathf.FloorToInt((point.x-min.x)/(max.x-min.x)*width);
        int y=Mathf.FloorToInt((point.y-min.y)/(max.y-min.y)*height);
        return blocked==null || blocked.Length!=width*height || blocked[y*width+x]!=0;
    }
    public bool Blocks(Rect footprint,Vector2 min,Vector2 max) {
        if(footprint.xMin<min.x || footprint.yMin<min.y || footprint.xMax>max.x || footprint.yMax>max.y)return true;
        if(width<=0 || height<=0 || blocked==null || blocked.Length!=width*height)return true;
        int left=Mathf.Clamp(Mathf.FloorToInt((footprint.xMin-min.x)/(max.x-min.x)*width),0,width-1);
        int right=Mathf.Clamp(Mathf.CeilToInt((footprint.xMax-min.x)/(max.x-min.x)*width)-1,0,width-1);
        int bottom=Mathf.Clamp(Mathf.FloorToInt((footprint.yMin-min.y)/(max.y-min.y)*height),0,height-1);
        int top=Mathf.Clamp(Mathf.CeilToInt((footprint.yMax-min.y)/(max.y-min.y)*height)-1,0,height-1);
        for(int y=bottom;y<=top;y++)for(int x=left;x<=right;x++)if(blocked[y*width+x]!=0)return true;
        return false;
    }
}

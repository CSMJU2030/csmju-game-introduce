using UnityEngine;

public sealed class CampusGroundMask : ScriptableObject
{
    public int width, height;
    public byte[] blocked;
    public bool Blocks(Vector2 point, Vector2 min, Vector2 max) {
        float u=Mathf.InverseLerp(min.x,max.x,point.x), v=Mathf.InverseLerp(min.y,max.y,point.y);
        int x=Mathf.Clamp((int)(u*width),0,width-1), y=Mathf.Clamp((int)(v*height),0,height-1);
        return blocked!=null && blocked.Length==width*height && blocked[y*width+x]!=0;
    }
}

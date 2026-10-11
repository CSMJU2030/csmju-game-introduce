using UnityEngine;

// Normalized coordinates share the same lower-left origin as the map texture.
// The drawn pitch and track are a rounded rectangle, not a full ellipse.
public static class CampusStadiumLayout
{
    public static readonly Rect MapUv=new Rect(.685f,.018f,.305f,.432f);
    public static readonly Rect EntranceUv=new Rect(.673f,.268f,.041f,.035f);
    public static readonly Vector2[] EntranceRoute={new Vector2(.50f,.446f),new Vector2(.657f,.446f),new Vector2(.69f,.334f),new Vector2(.71f,.286f)};
    public static bool OnEntrance(Vector2 uv)
    {
        // Measure in aspect-correct normalized space: a 1.6-world-unit-wide path.
        var aspect=new Vector2(1,.5627f);var p=Vector2.Scale(uv,aspect);
        for(int i=1;i<EntranceRoute.Length;i++) {
            var a=Vector2.Scale(EntranceRoute[i-1],aspect);var b=Vector2.Scale(EntranceRoute[i],aspect);var d=b-a;
            float t=Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude);
            if(Vector2.Distance(p,a+d*t)<.013f)return true;
        }
        return EntranceUv.Contains(uv);
    }
    public static readonly Rect Track=new Rect(.105f,.146f,.866f,.565f);
    public static readonly Rect[] Solids={
        new Rect(.203f,.735f,.659f,.18f), // Roofed stand and bleachers.
        new Rect(.196f,.393f,.033f,.108f),new Rect(.847f,.393f,.033f,.108f), // Goal frames.
        new Rect(.107f,.717f,.029f,.045f),new Rect(.922f,.697f,.04f,.055f), // Floodlight bases.
        new Rect(.102f,.09f,.035f,.052f),new Rect(.925f,.075f,.04f,.052f),
        new Rect(.332f,.73f,.024f,.031f),new Rect(.435f,.73f,.024f,.031f),
        new Rect(.609f,.73f,.024f,.031f),new Rect(.694f,.73f,.025f,.031f)
    };
    public static bool Walkable(Vector2 uv)
    {
        if(OnEntrance(uv))return true;
        if(!MapUv.Contains(uv))return false;
        var p=new Vector2((uv.x-MapUv.x)/MapUv.width,(uv.y-MapUv.y)/MapUv.height);
        foreach(var solid in Solids)if(solid.Contains(p))return false;
        if(new Rect(.06f,.07f,.046f,.84f).Contains(p) || new Rect(0,.44f,.17f,.075f).Contains(p)
            || new Rect(.095f,.711f,.845f,.042f).Contains(p))return true;
        if(!Track.Contains(p))return false;
        float cx=Mathf.Clamp(p.x,Track.xMin+.20f,Track.xMax-.20f);
        float cy=Mathf.Clamp(p.y,Track.yMin+.16f,Track.yMax-.16f);
        return Mathf.Pow((p.x-cx)/.20f,2)+Mathf.Pow((p.y-cy)/.16f,2)<=1;
    }
    public static Vector2 At(Vector2 local,Vector2 min,Vector2 max)
    {
        var uv=MapUv.position+Vector2.Scale(local,MapUv.size);
        return min+Vector2.Scale(uv,max-min);
    }
}

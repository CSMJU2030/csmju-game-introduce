using UnityEngine;
using System.Linq;

public sealed partial class DigitalCampusGame
{
    public CampusStudent[] students=System.Array.Empty<CampusStudent>();
    public CampusGroundMask groundMask;
    public Texture2D campusMapTexture;
    public bool AmbientMoving => insideBuilding && state==ScreenState.Playing && !mapOpen && !inventoryOpen;
    public bool AmbientCanWalk(Vector2 p) {
        foreach(var d in new[]{Vector2.zero,new Vector2(.18f,.18f),new Vector2(-.18f,-.18f),new Vector2(-.18f,.18f),new Vector2(.18f,-.18f)}) {
            var sample=p+d;
            if(!interiorWalkableFloors.Any(r=>r.Contains(sample))) return false;
            if(interiorColliders.Any(c=>c!=null && c.enabled && c.gameObject.activeInHierarchy && c.OverlapPoint(sample))) return false;
        }
        return true;
    }
    private void DrawLifeOverlay() {
        if(state!=ScreenState.Playing || mapOpen || inventoryOpen) return;
        foreach(var student in students) if(student!=null && student.gameObject.activeInHierarchy && !string.IsNullOrEmpty(student.bubble)) {
            var p=gameCamera.WorldToScreenPoint(student.transform.position+Vector3.up*.9f);
            if(p.x<20 || p.x>Screen.width-20 || p.y<30 || p.y>Screen.height-100) continue;
            Rect r=new Rect(p.x-110,Screen.height-p.y-35,220,44);
            Panel(r); CampusOutlinedText.Label(new Rect(r.x+8,r.y+5,r.width-16,r.height-10),student.bubble,smallStyle);
        }
        var npc=npcs.Where(n=>n!=null && n.gameObject.activeInHierarchy && n.npcId!="bug")
            .OrderBy(n=>Vector2.Distance(player.position,n.transform.position)).FirstOrDefault();
        if(npc==null || Vector2.Distance(player.position,npc.transform.position)>2f) return;
        var point=gameCamera.WorldToScreenPoint(npc.transform.position+Vector3.up*1.45f);
        Rect prompt=new Rect(point.x-105,Screen.height-point.y-22,210,35);
        Panel(prompt);
        CampusOutlinedText.Label(new Rect(prompt.x+6,prompt.y+3,198,30),completed.Contains(npc.npcId)?"กด E เพื่อคุย":"กด E เพื่อทำเควส",badgeStyle);
    }
    private void DrawMiniMap() {
        if(insideBuilding || campusMapTexture==null || inventoryOpen) return;
        Rect r=new Rect(viewWidth-256,viewHeight-194,240,136);
        Panel(new Rect(r.x-4,r.y-24,r.width+8,r.height+28));
        CampusOutlinedText.Label(new Rect(r.x+7,r.y-22,220,22),"MINI MAP · จุดสีฟ้าคือคุณ",smallStyle);
        DrawCampusMap(r,true);
    }
    private void DrawCampusMap(Rect r,bool cameraFrame) {
        CampusOutlinedText.Picture(r,campusMapTexture);
        Vector2 At(Vector2 world)=>new Vector2(r.x+Mathf.InverseLerp(mapMin.x,mapMax.x,world.x)*r.width,r.y+(1-Mathf.InverseLerp(mapMin.y,mapMax.y,world.y))*r.height);
        foreach(var npc in npcs) if(npc!=null && npc.gameObject.activeInHierarchy && npc.npcId!="bug") {
            var p=At(npc.transform.position); Fill(new Rect(p.x-3,p.y-3,6,6),new Color(1,.8f,.2f));
        }
        if(cameraFrame) {
            var extent=new Vector2(gameCamera.orthographicSize*gameCamera.aspect,gameCamera.orthographicSize);
            Vector2 a=At((Vector2)gameCamera.transform.position-extent), b=At((Vector2)gameCamera.transform.position+extent);
            Color edge=new Color(.4f,1,.9f,.7f);
            Fill(new Rect(a.x,b.y,b.x-a.x,1.5f),edge); Fill(new Rect(a.x,a.y,b.x-a.x,1.5f),edge);
            Fill(new Rect(a.x,b.y,1.5f,a.y-b.y),edge); Fill(new Rect(b.x,b.y,1.5f,a.y-b.y),edge);
        }
        var marker=At(player.position); Disc(new Rect(marker.x-4,marker.y-4,8,8),Color.cyan);
    }
}

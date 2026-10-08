using UnityEngine;

public sealed class CampusStudent : MonoBehaviour
{
    public DigitalCampusGame game;
    public Sprite[] frames;
    public Vector2[] route;
    public string studentName;
    public string[] chat;
    public string bubble;
    private int target;
    private float wait, nextChat, speed=.8f, blockedTime;
    private SpriteRenderer sprite;
    public void SetPatrol(Vector2[] points) { route=points;target=Random.Range(0,route.Length);transform.position=route[target];wait=Random.Range(.4f,2f);bubble="";nextChat=Time.time+Random.Range(3f,9f); }
    public void Reply() { bubble="ได้เลย ไปด้วยกันนะ!"; wait=3f; nextChat=Time.time+12f; }
    void ChooseDestination() {
        blockedTime=0; speed=Random.Range(.55f,1.05f);
        for(int attempt=0;attempt<12;attempt++) {
            int candidate=Random.Range(0,route.Length);
            if(Vector2.Distance(transform.position,route[candidate])<.2f) continue;
            bool clear=true;
            for(int step=1;step<=24;step++) if(!game.AmbientCanWalk(Vector2.Lerp(transform.position,route[candidate],step/24f))) { clear=false; break; }
            if(clear) { target=candidate; return; }
        }
    }
    void Awake() { sprite=GetComponent<SpriteRenderer>(); nextChat=Time.time+Random.Range(2f,6f); }
    void Update() {
        if(game==null || !game.AmbientMoving || route==null || route.Length<2) return;
        if(Time.time>nextChat) {
            bubble=chat.Length>0?chat[Random.Range(0,chat.Length)]:"เจอกันที่ห้องแล็บนะ";
            wait=3f; nextChat=Time.time+Random.Range(8f,13f);
            foreach(var other in game.students) if(other!=this && other!=null && Vector2.Distance(transform.position,other.transform.position)<3.5f) { other.Reply(); break; }
        }
        if(wait>0) { wait-=Time.deltaTime; if(wait<=0) bubble=""; return; }
        Vector2 current=transform.position, delta=route[target]-current;
        if(delta.magnitude<.09f) { ChooseDestination(); wait=Random.Range(.4f,3.5f); return; }
        Vector2 move=Vector2.MoveTowards(current,route[target],Time.deltaTime*speed);
        if(!game.AmbientCanWalk(move) || Vector2.Distance(move,game.player.position)<.65f) { blockedTime+=Time.deltaTime; if(blockedTime>1.5f) ChooseDestination(); return; }
        blockedTime=0;
        foreach(var other in game.students) if(other!=this && other!=null && Vector2.Distance(move,other.transform.position)<.5f) return;
        transform.position=move;
        if(frames.Length>=24) {
            int facing=Mathf.Abs(delta.x)>Mathf.Abs(delta.y)?(delta.x<0?1:2):(delta.y<0?0:3);
            sprite.sprite=frames[facing*6+Mathf.FloorToInt(Time.time*6)%6];
        }
        sprite.sortingOrder=1;
    }
}

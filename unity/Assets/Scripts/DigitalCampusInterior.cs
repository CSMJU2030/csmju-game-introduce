using UnityEngine;

public sealed partial class DigitalCampusGame
{
    public GameObject buildingInterior;
    public Vector2 interiorSpawn = new Vector2(110.5f,-6.5f);
    public Vector2 interiorCameraCenter = new Vector2(102.5f,1f);
    public Collider2D[] interiorColliders = System.Array.Empty<Collider2D>();
    public BoxCollider2D playerCollider;
    public Rigidbody2D playerBody;
    public Vector2 buildingEntrance = new Vector2(-9.2f, 1f);
    public Rect[] interiorFurniture = System.Array.Empty<Rect>();
    private bool insideBuilding;
    private Vector2 outsideMin, outsideMax;
    private float outsideCameraSize;
    public Rect[] interiorWalkableFloors = System.Array.Empty<Rect>();
    private Vector3 outsidePlayerScale;
    // Footprint matches the visible feet, rather than the transparent sprite frame.
    private Vector2[] FootOffsets {
        get {
            var size=playerCollider != null ? playerCollider.size : new Vector2(.56f,.36f);
            var offset=playerCollider != null ? playerCollider.offset : Vector2.zero;
            var scale=player != null ? (Vector2)player.lossyScale : Vector2.one;
            var half=Vector2.Scale(size,scale)*.5f;
            var center=Vector2.Scale(offset,scale);
            return new[]{center,center+new Vector2(half.x,0),center-new Vector2(half.x,0),
                center+new Vector2(0,half.y),center-new Vector2(0,half.y),
                center+half,center-half,center+new Vector2(half.x,-half.y),center+new Vector2(-half.x,half.y)};
        }
    }
    private bool InsideFloor(Vector2 point)
    {
        var offsets=FootOffsets;
        var footprint = new Bounds(point+offsets[0], new Vector3(Mathf.Abs(offsets[1].x-offsets[2].x),Mathf.Abs(offsets[3].y-offsets[4].y),1));
        foreach(var collider in interiorColliders)
            if(collider != null && collider.enabled && collider.gameObject.activeInHierarchy
                && collider.bounds.Intersects(footprint)) return false;
        foreach (var offset in offsets)
        {
            Vector2 sample = point + offset;
            foreach(var collider in interiorColliders) if(collider != null && collider.OverlapPoint(sample)) return false;
            foreach (var obstacle in interiorFurniture) if (obstacle.Contains(sample)) return false;
            bool onFloor = false;
            foreach (var floor in interiorWalkableFloors) onFloor |= floor.Contains(sample);
            if (!onFloor) return false;
        }
        return true;
    }
    private bool NearBuildingPortal()
    {
        return player != null && Vector2.Distance(player.position, insideBuilding ? interiorSpawn : buildingEntrance) < (insideBuilding ? 1.1f : 3.4f);
    }
    private bool TryBuildingPortal()
    {
        if (buildingInterior == null || !NearBuildingPortal()) return false;
        insideBuilding = !insideBuilding;
        if (insideBuilding)
        {
            outsideMin=mapMin;outsideMax=mapMax;outsideCameraSize=cameraViewSize;
            mapMin=new Vector2(65,-5);mapMax=new Vector2(95,13);cameraViewSize=8.4f;
            outsidePlayerScale=player.localScale;player.localScale=Vector3.one;
            player.position=interiorSpawn;
        }
        else
        {
            mapMin=outsideMin;mapMax=outsideMax;cameraViewSize=outsideCameraSize;player.localScale=outsidePlayerScale;
            player.position=new Vector3(buildingEntrance.x,buildingEntrance.y-.5f,0);
        }
        buildingInterior.SetActive(insideBuilding);
        Physics2D.SyncTransforms();
        foreach(var npc in npcs) if(npc!=null) npc.gameObject.SetActive(npc.npcId != "bug" && ((npc.npcId=="curriculum" || npc.npcId=="data") ? insideBuilding : !insideBuilding));
        FollowCamera(true);
        ShowToast(insideBuilding ? "พบอาจารย์ Algorithm ฝั่งซ้าย และพี่ดาต้าในห้องฝั่งขวา" : "กลับสู่หน้าคณะวิทยาศาสตร์",4);
        return true;
    }
    private void DrawBuildingHint()
    {
        if(mapOpen || inventoryOpen) return;

        if(!NearBuildingPortal()) return;
        Panel(insideBuilding ? new Rect(985,625,275,46) : new Rect(430,535,420,48));
        CampusOutlinedText.Label(insideBuilding ? new Rect(999,634,251,30) : new Rect(447,544,395,32),insideBuilding ? "[E] ออกจากอาคาร" : "[E] เข้าอาคารแม่โจ้ 60 ปี",badgeStyle);
    }
    private void DrawInteriorHud()
    {
        Panel(new Rect(16,16,viewWidth-32,58));
        CampusOutlinedText.Label(new Rect(32,28,430,32),"ชั้น 6 · อาคารแม่โจ้ 60 ปี",headingStyle);
        CampusOutlinedText.Label(new Rect(450,30,220,30),"Skill Badges "+badges.Count+" / 6",badgeStyle);
        CampusOutlinedText.Label(new Rect(660,30,280,30),"WASD เดิน · E คุย / เข้า–ออก",smallStyle);
        if(CampusOutlinedText.Button(new Rect(viewWidth-318,27,140,36),"แบดจ์ [I]",buttonStyle))inventoryOpen=!inventoryOpen;
        if(CampusOutlinedText.Button(new Rect(viewWidth-164,27,132,36),"หยุด [Esc]",buttonStyle))Pause();
        if(inventoryOpen)
        {
            Panel(new Rect(16,90,310,228));
            for(int i=0;i<BadgeNames.Length;i++)CampusOutlinedText.Label(new Rect(30,103+i*32,280,30),(badges.Contains(BadgeNames[i])?"◆ ":"◇ ")+BadgeNames[i],smallStyle);
        }
    }

}

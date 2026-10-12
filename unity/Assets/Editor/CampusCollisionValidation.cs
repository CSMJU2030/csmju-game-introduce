using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class CampusCollisionValidation
{
    public static void ValidateInterior(DigitalCampusGame game,Tilemap[] maps,HashSet<Vector2> reachable)
    {
        int checkedProps=0;
        foreach(var map in maps.Where(m=>m.name!="layer0" && m.name!="layer1"))
            foreach(var cell in map.cellBounds.allPositionsWithin)if(map.HasTile(cell)) {
                var rect=CampusCollisionBuilder.SpriteBounds(map,cell);
                // Include the lower half of tall sprites, outside their anchor cell.
                foreach(var foot in new[]{rect.center,new Vector2(rect.center.x,rect.yMin+.06f)})
                    if(game.AmbientCanWalk(foot-game.playerCollider.offset))throw new Exception("Furniture can be crossed: "+cell);
                checkedProps++;
            }
        foreach(var npc in game.npcs.Where(n=>n!=null && (n.npcId=="data" || n.npcId=="curriculum")))
            if(!reachable.Any(p=>Vector2.Distance(p,npc.transform.position)<.6f))throw new Exception("Unreachable quest NPC: "+npc.npcId);
        foreach(var student in game.students)if(student.route.Any(p=>!game.AmbientCanWalk(p)))throw new Exception("Student route crosses furniture");
        if(!reachable.Any(p=>p.x<99 && p.y>5) || !reachable.Any(p=>p.x>110 && p.y>3) || !reachable.Contains(game.interiorExit))throw new Exception("Interior room or exit disconnected");
        Debug.Log("COLLISION INTERIOR PASSED: "+checkedProps+" furniture sprites, lower edges, both quest NPCs, student routes and exit; "+reachable.Count+" accessible samples.");
    }
    public static void ValidateOutdoor(DigitalCampusGame game)
    {
        var method=typeof(DigitalCampusGame).GetMethod("CanWalk",BindingFlags.NonPublic|BindingFlags.Instance);
        bool Can(Vector2 p)=>(bool)method.Invoke(game,new object[]{p});
        foreach(var c in game.outdoorSolids)if(Can((Vector2)c.bounds.center-game.playerCollider.offset))throw new Exception("Outdoor prop can be crossed: "+c.name);
        Debug.Log("COLLISION OUTDOOR PASSED: "+game.outdoorSolids.Length+" solid prop footprints.");
    }
}

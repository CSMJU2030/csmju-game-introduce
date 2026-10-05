using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CampusActivitiesBuild
{
    [MenuItem("CSMJU/Build Surprise Battle and Puzzles")]
    public static void Build()
    {
        // Keep the older menu functional without restoring the retired slime or NPC art.
        CampusExpansionBuild.Build();
    }
    public static void CheckRules()
    {
        var wire=new WirePuzzle(); Require(wire.Connected(),"Solved wire must connect");
        for(int seed=0;seed<30;seed++)
        {
            wire=new WirePuzzle(); wire.Shuffle(new System.Random(seed)); Require(!wire.Connected(),"Shuffled wire must be unsolved");
            // Every shuffle uses legal slides: verify even inversion parity for a 3x3 board.
            int inversions=0;
            for(int i=0;i<9;i++) for(int j=i+1;j<9;j++) if(wire.Cells[i]!=0 && wire.Cells[j]!=0 && wire.Cells[i]>wire.Cells[j]) inversions++;
            Require(inversions%2==0,"Wire shuffle must be solvable");
        }
        wire=new WirePuzzle(); Require(!wire.Move(0),"Nonadjacent slide must fail"); Require(wire.Move(7),"Adjacent slide must work");
        wire.Remaining=0; Require(!wire.Move(8),"Expired board must reject movement");
        wire.Shuffle(new System.Random(88)); Require(wire.Remaining == 90 && !wire.Connected(), "An expired puzzle can be retried without freezing");
        for(int shift=1;shift<26;shift++) { var cipher=new CipherPuzzle{Shift=shift}; cipher.Rotate(shift); Require(cipher.Solved,"Cipher rotation must decode"); cipher.Rotate(1); Require(!cipher.Solved,"Wrong shift must fail"); }
        var battle=new BugBattle(4); battle.Energy=0; int hp=battle.BugHp; battle.Act(0); Require(battle.BugHp==hp && battle.Turn==0,"Empty energy cannot attack");
        battle.Act(2); Require(battle.Energy==3 && battle.PlayerHp==BugBattle.PlayerMaxHp-1,"Pair restores energy and guards");
        battle=new BugBattle(8);
        for(int i=0;i<40 && !battle.Won && !battle.Lost;i++) battle.Act(battle.Energy<2 || battle.Intent==1?2:battle.PlayerHp<=10 && battle.Patches>0?3:battle.Intent==2?1:0);
        Require(battle.Won,"Battle can be won with tactical actions");
        Debug.Log("ACTIVITY RULE CHECKS PASSED: 30 solvable boards, cipher shifts, battle energy / guard / win");
    }
    private static void Require(bool result,string message) { if(!result) throw new Exception(message); }
}

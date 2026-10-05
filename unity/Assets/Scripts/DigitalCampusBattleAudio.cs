using UnityEngine;

public sealed partial class DigitalCampusGame
{
    private AudioSource battleMusic;
    private AudioClip battleHitSound, battleSupportSound, battleVictorySound;
    private bool musicStarted;
    private void InitializeBattleAudio()
    {
        battleMusic=gameObject.AddComponent<AudioSource>();
        battleMusic.loop=true; battleMusic.playOnAwake=false; battleMusic.spatialBlend=0; battleMusic.volume=.42f;
        battleMusic.clip=CreateBattleMusic();
        battleHitSound=MakeTone("debug-hit",880,.14f,.2f);
        battleSupportSound=MakeTone("team-support",440,.20f,.17f);
        battleVictorySound=MakeTone("battle-victory",1046.5f,.42f,.2f);
    }
    private void SyncBattleMusic()
    {
        if(battleMusic==null)return;
        bool session=state==ScreenState.Combat || state==ScreenState.Surprise
            || state==ScreenState.Paused && (resumeState==ScreenState.Combat || resumeState==ScreenState.Surprise);
        bool playing=session && state!=ScreenState.Paused && audioOn && battle!=null && ((!battle.Won && !battle.Lost) || turnAnimating);
        if(playing)
        {
            if(!musicStarted){battleMusic.Play();musicStarted=true;}
            else if(!battleMusic.isPlaying)battleMusic.UnPause();
        }
        else if(session){if(battleMusic.isPlaying)battleMusic.Pause();}
        else if(musicStarted){battleMusic.Stop();musicStarted=false;}
    }
    private void PlayBattleAction(int action,bool victory)
    {
        if(audioOn)audioSource.PlayOneShot(victory?battleVictorySound:action<2?battleHitSound:battleSupportSound);
        SyncBattleMusic();
    }
    // Original 144 BPM chiptune: eight bars, bass pulse, lead, kick, snare and hats.
    // Synthesized once; no external downloads or per-frame audio allocations.
    private static AudioClip CreateBattleMusic()
    {
        const int rate=22050; const float beatSeconds=60f/144;
        int length=Mathf.RoundToInt(32*beatSeconds*rate); var samples=new float[length];
        int[] roots={45,41,43,40,45,41,43,40};
        int[] lead={81,76,79,76,84,81,79,76,81,84,88,84,79,76,79,83};
        uint noise=17;
        for(int i=0;i<length;i++)
        {
            float time=i/(float)rate,beat=time/beatSeconds,beatPhase=(beat-Mathf.Floor(beat))*beatSeconds;
            float eighth=beat*2-Mathf.Floor(beat*2),local=eighth*beatSeconds*.5f;
            int bar=Mathf.FloorToInt(beat/4)%8,step=Mathf.FloorToInt(beat*2);
            float bassHz=440*Mathf.Pow(2,(roots[bar]-69)/12f),leadHz=440*Mathf.Pow(2,(lead[step%16]-(bar%4==1?5:bar%4==2?2:0)-69)/12f);
            float bass=(Mathf.Sin(2*Mathf.PI*bassHz*time)+.28f*Mathf.Sin(4*Mathf.PI*bassHz*time))*.18f*Mathf.Exp(-local*9);
            float envelope=Mathf.Min(1,local*160)*Mathf.Exp(-local*7);
            float melody=(Mathf.Sin(2*Mathf.PI*leadHz*time)+.3f*Mathf.Sin(6*Mathf.PI*leadHz*time))*.14f*envelope;
            noise=unchecked(noise*1664525u+1013904223u); float white=(noise>>8)/8388607.5f-1;
            float kick=Mathf.Sin(2*Mathf.PI*(52*beatPhase+3*(1-Mathf.Exp(-beatPhase*28))))*.30f*Mathf.Exp(-beatPhase*19);
            float snare=(Mathf.FloorToInt(beat)%4==1 || Mathf.FloorToInt(beat)%4==3)?white*.19f*Mathf.Exp(-beatPhase*28):0;
            float hats=white*.065f*Mathf.Exp(-local*75);
            samples[i]=Mathf.Clamp(bass+melody+kick+snare+hats,-.85f,.85f);
        }
        var clip=AudioClip.Create("CS Debug Rush - original battle loop",length,1,rate,false);clip.SetData(samples,0);return clip;
    }
}

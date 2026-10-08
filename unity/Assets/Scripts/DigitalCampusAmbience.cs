using UnityEngine;

public sealed partial class DigitalCampusGame
{
    private AudioSource ambience;
    private AudioClip campusMusic, classroomMusic;
    private void InitializeCampusAudio() {
        ambience=gameObject.AddComponent<AudioSource>(); ambience.loop=true; ambience.playOnAwake=false; ambience.volume=.24f;
        campusMusic=CreateChillMusic(false); classroomMusic=CreateChillMusic(true);
        footstepClip=CreateFootstep();
    }
    private void SyncCampusAudio() {
        if(ambience==null) return;
        var wanted=insideBuilding?classroomMusic:campusMusic;
        if(ambience.clip!=wanted) { ambience.Stop(); ambience.clip=wanted; }
        bool play=audioOn && state!=ScreenState.Paused && state!=ScreenState.Combat && state!=ScreenState.Surprise;
        if(play && !ambience.isPlaying) ambience.Play();
        if(!play && ambience.isPlaying) ambience.Pause();
    }
    private static AudioClip CreateFootstep() {
        const int rate=22050; var data=new float[2600]; uint random=91; float filtered=0;
        for(int i=0;i<data.Length;i++) {
            float t=i/(float)rate; random=unchecked(random*1664525u+1013904223u);
            filtered=Mathf.Lerp(filtered,(random>>8)/8388607.5f-1,.16f);
            data[i]=filtered*Mathf.Exp(-t*42)*.23f+Mathf.Sin(2*Mathf.PI*72*t)*Mathf.Exp(-t*65)*.12f;
        }
        var clip=AudioClip.Create("Soft campus shoe step",data.Length,1,rate,false); clip.SetData(data,0); return clip;
    }
    // Original music: campus acoustic plucks vs classroom electric piano.
    private static AudioClip CreateChillMusic(bool indoor) {
        const int rate=22050; float beat=60f/(indoor?72:86); var data=new float[Mathf.RoundToInt(32*beat*rate)];
        int[] roots=indoor?new[]{48,53,50,55}:new[]{48,45,53,55};
        int[] notes=indoor?new[]{72,76,79,83,74,77,81,84}:new[]{76,79,81,79,74,72,69,72};
        for(int i=0;i<data.Length;i++) {
            float t=i/(float)rate, b=t/beat, phase=(b*2-Mathf.Floor(b*2))*beat*.5f;
            int step=Mathf.FloorToInt(b*2), root=roots[(int)(b/8)%4];
            float hz=440*Mathf.Pow(2,(notes[step%8]-69)/12f), bass=440*Mathf.Pow(2,(root-69)/12f);
            float envelope=Mathf.Min(1,phase*140)*Mathf.Exp(-phase*(indoor?3.5f:5f));
            float lead=(Mathf.Sin(t*hz*2*Mathf.PI)+.2f*Mathf.Sin(t*hz*4*Mathf.PI))*envelope*.12f;
            float pad=Mathf.Sin(t*bass*2*Mathf.PI)*.075f+Mathf.Sin(t*bass*3*Mathf.PI)*.035f;
            data[i]=(lead+pad)*Mathf.Min(1,t*3)*Mathf.Min(1,(data.Length/(float)rate-t)*3);
        }
        var clip=AudioClip.Create(indoor?"Classroom piano lounge":"Green campus afternoon",data.Length,1,rate,false);clip.SetData(data,0);return clip;
    }
}

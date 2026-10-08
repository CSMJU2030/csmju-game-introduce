using UnityEngine;

public sealed partial class DigitalCampusGame
{
    private Texture2D binaryGlyphs;
    private void DrawCodeRain(Rect viewport) {
        Fill(viewport,new Color(.005f,.012f,.009f));
        if(binaryGlyphs==null) {
            binaryGlyphs=new Texture2D(12,9,TextureFormat.RGBA32,false); binaryGlyphs.filterMode=FilterMode.Point;
            var pixels=new Color[108];
            int[][] glyphs={new[]{14,17,19,21,25,17,14},new[]{4,12,4,4,4,4,14}};
            for(int g=0;g<2;g++) for(int row=0;row<7;row++) for(int col=0;col<5;col++)
                if((glyphs[g][row]&(1<<(4-col)))!=0) pixels[(7-row)*12+g*6+col]=Color.white;
            binaryGlyphs.SetPixels(pixels);binaryGlyphs.Apply();
        }
        // Point-filtered 5x7 binary glyphs remain pixel art; other UI stays Prompt.
        float time=Time.unscaledTime;
        for(int col=0;col<34;col++) {
            float x=viewport.x+(col+.5f)*viewport.width/34;
            float speed=35+(col*31%65), offset=Mathf.Repeat(time*speed+col*97,viewport.height+250);
            for(int row=0;row<13;row++) {
                float y=viewport.y+offset-row*20;
                if(y<viewport.y || y+18>viewport.yMax) continue;
                int digit=((col*73+row*19+(int)(time*2))%5)<2?1:0;
                GUI.color=row==0?new Color(.6f,1,.7f,.8f):new Color(.04f,.65f,.2f,(1-row/14f)*.36f);
                CampusOutlinedText.PictureUV(new Rect(x,y,10,18),binaryGlyphs,new Rect(digit*.5f,0,.5f,1));
            }
        }
        GUI.color=Color.white;
    }
}

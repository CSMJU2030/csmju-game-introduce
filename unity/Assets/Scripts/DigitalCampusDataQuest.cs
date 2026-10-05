using UnityEngine;

public sealed partial class DigitalCampusGame
{
    private int dataStage, selectedDataset = -1, selectedPipeline = -1;
    private readonly int[] datasetBins = {-1,-1,-1,-1,-1,-1};
    private readonly int[] pipelineSlots = {-1,-1,-1,-1};
    private readonly string[] datasetNames = {"ตารางชั่วโมงกิจกรรม CSV","ข้อความรีวิวค่าย CS","รูปถ่ายโครงงาน","ตารางคะแนนแบบประเมิน","ภาพใบประกาศนียบัตร","ความคิดเห็นจากแบบฟอร์ม"};
    private readonly string[] pipelineNames = {"ฝึกโมเดล","ประเมินชุดทดสอบ","ทำความสะอาดข้อมูล","แบ่ง Train / Test"};
    private void ResetDataQuest()
    {
        dataStage=0;selectedDataset=selectedPipeline=-1;
        for(int i=0;i<6;i++) datasetBins[i]=-1;
        for(int i=0;i<4;i++) pipelineSlots[i]=-1;
    }
    private void DrawDataQuest()
    {
        if(dataStage==0) { DrawCipher();return; }
        CampusOutlinedText.Label(new Rect(175,115,900,38),dataStage==1 ? "DATA LAB  2/3 · จัดข้อมูลลงตะกร้า" : "DATA LAB  3/3 · ประกอบ ML Pipeline",headingStyle);
        CampusOutlinedText.Label(new Rect(175,164,900,65),dataStage==1 ? "ลากการ์ดลงตะกร้า หรือคลิกการ์ดแล้วคลิกตะกร้า\nตารางมีแถว/คอลัมน์ · ข้อความต้องตีความภาษา · รูปภาพใช้ Computer Vision" : "ทำความสะอาด → แบ่ง Train/Test → ฝึกโมเดล → ประเมิน\nแบ่งข้อมูลก่อนฝึกเสมอ เพื่อป้องกันข้อมูลทดสอบรั่วเข้าโมเดล",bodyStyle);
        if(dataStage==1)
        {
            for(int i=0;i<6;i++)
            {
                Rect r=new Rect(175+(i%2)*221,250+(i/2)*100,208,85);
                GUI.color=selectedDataset==i?new Color(1,.85f,.4f):Color.white;
                CampusOutlinedText.Box(r,datasetNames[i]+(datasetBins[i]<0?"":"\n→ "+new[]{"ตาราง","ข้อความ","รูปภาพ"}[datasetBins[i]]),buttonStyle);GUI.color=Color.white;
                if(Event.current.type==EventType.MouseDown && r.Contains(Event.current.mousePosition)){selectedDataset=i;Event.current.Use();}
            }
            for(int bin=0;bin<3;bin++)
            {
                Rect r=new Rect(675,250+bin*100,390,85);CampusOutlinedText.Box(r,new[]{"01  TABLE · ตาราง","02  TEXT · ข้อความ","03  IMAGE · รูปภาพ"}[bin],buttonStyle);
                if(Event.current.type==EventType.MouseUp && r.Contains(Event.current.mousePosition) && selectedDataset>=0){datasetBins[selectedDataset]=bin;selectedDataset=-1;PlayUiSfx();Event.current.Use();}
            }
            if(CampusOutlinedText.Button(new Rect(760,560,300,45),"ตรวจประเภทข้อมูล",buttonStyle))
            {
                int[] correct={0,1,2,0,2,1};bool valid=true;for(int i=0;i<6;i++) valid &= datasetBins[i]==correct[i];
                if(valid){dataStage=2;feedback="จัดข้อมูลครบแล้ว! วางลำดับ pipeline ต่อได้";}else feedback="บางการ์ดยังอยู่ผิดตะกร้าหรือยังไม่ถูกจัด ลองตรวจรูปแบบข้อมูล";
            }
        }
        else
        {
            for(int i=0;i<4;i++)
            {
                Rect card=new Rect(175+i*224,255,210,80);CampusOutlinedText.Box(card,pipelineNames[i],buttonStyle);
                if(Event.current.type==EventType.MouseDown && card.Contains(Event.current.mousePosition)){selectedPipeline=i;Event.current.Use();}
                Rect slot=new Rect(175+i*224,390,210,95);CampusOutlinedText.Box(slot,(i+1)+". "+(pipelineSlots[i]<0?"วางขั้นตอน":pipelineNames[pipelineSlots[i]]),buttonStyle);
                if(Event.current.type==EventType.MouseUp && slot.Contains(Event.current.mousePosition) && selectedPipeline>=0)
                {
                    for(int j=0;j<4;j++)if(pipelineSlots[j]==selectedPipeline)pipelineSlots[j]=-1;
                    pipelineSlots[i]=selectedPipeline;selectedPipeline=-1;PlayUiSfx();Event.current.Use();
                }
            }
            if(CampusOutlinedText.Button(new Rect(760,560,300,45),"รัน Pipeline",buttonStyle))
            {
                if(pipelineSlots[0]==2 && pipelineSlots[1]==3 && pipelineSlots[2]==0 && pipelineSlots[3]==1)FinishNpc("data");
                else feedback="Pipeline ยังไม่ปลอดภัย: แบ่งข้อมูลก่อนฝึก และประเมินบน Test ที่แยกไว้";
            }
        }
        if(Event.current.type==EventType.Repaint)
        {
            string text=selectedDataset>=0?datasetNames[selectedDataset]:selectedPipeline>=0?pipelineNames[selectedPipeline]:null;
            if(text!=null){Vector2 m=Event.current.mousePosition;CampusOutlinedText.Box(new Rect(m.x-100,m.y-32,200,62),text,buttonStyle);}
        }
    }
}

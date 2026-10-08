using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

// Retains IMGUI input handling while rendering every caption with the same outlined font.
public static class CampusOutlinedText
{
    static Canvas canvas;
    static TMP_FontAsset font;
    static readonly List<TextMeshProUGUI> labels=new List<TextMeshProUGUI>();
    static int used;
    static readonly List<RawImage> images=new List<RawImage>();
    static int imageUsed;
    public static void Begin(){if(Event.current.type==EventType.Repaint){used=0;imageUsed=0;}}
    public static void End(){if(Event.current.type!=EventType.Repaint)return;for(int i=used;i<labels.Count;i++)labels[i].gameObject.SetActive(false);for(int i=imageUsed;i<images.Count;i++)images[i].gameObject.SetActive(false);}
    static void EnsureCanvas(){
        if(canvas!=null)return;
        labels.Clear();images.Clear();used=imageUsed=0;
        canvas=new GameObject("Campus UI · Prompt TMP",typeof(Canvas)).GetComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
        font=Resources.Load<TMP_FontAsset>("Fonts/Prompt TMP");
    }
    static void Place(RectTransform rt,Rect rect){
        var p=GUIUtility.GUIToScreenPoint(rect.position);
        var right=GUIUtility.GUIToScreenPoint(rect.position+Vector2.right)-p;
        var down=GUIUtility.GUIToScreenPoint(rect.position+Vector2.up)-p;
        rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(0,1);
        rt.anchoredPosition=new Vector2(p.x,-p.y);rt.sizeDelta=new Vector2(rect.width*right.magnitude,rect.height*down.magnitude);
        rt.localRotation=Quaternion.Euler(0,0,-Mathf.Atan2(right.y,right.x)*Mathf.Rad2Deg);rt.SetAsLastSibling();
    }
    public static void Picture(Rect rect,Texture texture,ScaleMode scale=ScaleMode.StretchToFill){
        var uv=new Rect(0,0,1,1);
        if(scale==ScaleMode.ScaleAndCrop){float source=(float)texture.width/texture.height,target=rect.width/rect.height;if(source>target){uv.width=target/source;uv.x=(1-uv.width)/2;}else{uv.height=source/target;uv.y=(1-uv.height)/2;}}
        PictureUV(rect,texture,uv);
    }
    public static void PictureUV(Rect rect,Texture texture,Rect uv){
        if(Event.current.type!=EventType.Repaint)return;EnsureCanvas();
        if(imageUsed==images.Count){var go=new GameObject("Panel",typeof(RectTransform));go.transform.SetParent(canvas.transform,false);var im=go.AddComponent<RawImage>();im.raycastTarget=false;images.Add(im);}
        var image=images[imageUsed++];image.gameObject.SetActive(true);Place(image.rectTransform,rect);image.texture=texture;image.uvRect=uv;image.color=GUI.color;
    }
    public static bool Button(Rect rect,GUIContent content,GUIStyle style)=>Button(rect,content.text,style);
    public static void Label(Rect rect,string text,GUIStyle style){
        if(Event.current.type!=EventType.Repaint || string.IsNullOrEmpty(text))return;
        EnsureCanvas();
        if(used==labels.Count){var go=new GameObject("Text",typeof(RectTransform));go.transform.SetParent(canvas.transform,false);var t=go.AddComponent<TextMeshProUGUI>();t.raycastTarget=false;t.font=font;t.fontSharedMaterial=font.material;labels.Add(t);}
        var label=labels[used++];label.gameObject.SetActive(true);
        float scale=GUI.matrix.GetColumn(0).magnitude;
        Place(label.rectTransform,rect);
        label.text="<line-height=125%>"+text;label.fontSize=style.fontSize*scale;label.color=style.normal.textColor*GUI.color*(GUI.enabled?Color.white:new Color(1,1,1,.45f));
        label.enableAutoSizing=true;label.fontSizeMax=style.fontSize*scale;label.fontSizeMin=style.fontSize*scale*.68f;
        label.alignment=style.alignment==TextAnchor.MiddleCenter?TextAlignmentOptions.Center:TextAlignmentOptions.TopLeft;
        label.textWrappingMode=style.wordWrap?TextWrappingModes.Normal:TextWrappingModes.NoWrap;label.overflowMode=TextOverflowModes.Truncate;
    }
    public static bool Button(Rect rect,string text,GUIStyle style){
        var hit=GUI.Button(rect,GUIContent.none,GUIStyle.none);Box(rect,text,style);return hit;
    }
    public static void Box(Rect rect,string text,GUIStyle style){
        var saved=GUI.color;GUI.color=saved*new Color(.3f,.55f,.45f,1);Picture(rect,Texture2D.whiteTexture);
        GUI.color=saved*new Color(.04f,.18f,.13f,1);Picture(new Rect(rect.x+2,rect.y+2,rect.width-4,rect.height-4),Texture2D.whiteTexture);
        GUI.color=saved;Label(rect,text,style);
    }
}

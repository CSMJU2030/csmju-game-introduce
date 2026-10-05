using UnityEngine;

public sealed class CampusNpc : MonoBehaviour
{
    public string npcId = "";
    public string displayName = "";
    [TextArea] public string role = "";
    public SpriteRenderer spriteRenderer;
}

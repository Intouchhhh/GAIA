using UnityEngine;

[CreateAssetMenu(menuName = "Scene Data")]
public class StageData : ScriptableObject
{
    [Header("Camera")]
    public float camSize;

    [Header("Colors")]
    public Color backgroundColor;
    public Color foregroundColor;
}

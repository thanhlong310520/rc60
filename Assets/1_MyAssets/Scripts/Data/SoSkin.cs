using Raccoon.EnumHolder;
using UnityEngine;

[CreateAssetMenu(fileName = "SoSkin", menuName = "Data/SoSkin")]
public class SoSkin : ScriptableObject
{
    public string id;
    public TypeSkin typeSkin;
    public Sprite icon;

    public bool isAds;
    public bool tagNew;
}

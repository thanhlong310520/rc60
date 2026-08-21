using Raccoon.EnumHolder;
using UnityEngine;
[CreateAssetMenu(fileName = "SoDailyReward", menuName = "Data/SoDailyReward")]

public class SoDailyReward : ScriptableObject
{
    public int day;
    public long income;
    public Sprite icon;
    public TypeCurrency type;
}

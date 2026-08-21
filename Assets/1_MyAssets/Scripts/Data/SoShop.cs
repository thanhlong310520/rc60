using Raccoon.EnumHolder;
using Raccoon.Purchase;
using System.Collections.Generic;
using UnityEngine;
using Raccoon;


[CreateAssetMenu(fileName = "shop", menuName = "Data/Shop data")]

public class SoShop : ScriptableObject
{
    public EShopType type;
    public List<IncomeData> incomeDatas;
    [Header("Currency")]
    public TypeCurrency typeCurrency;
    public long price;
    [Header("IAP")]
    public IAPProductData data;

}

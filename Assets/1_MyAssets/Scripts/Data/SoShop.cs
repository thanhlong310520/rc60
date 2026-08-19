using Raccoon.EnumHolder;
using Raccoon.Purchase;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "shop", menuName = "Data/Shop data")]

public class SoShop : ScriptableObject
{
    public EShopType type;
    public List<IncomeData> incomeDatas;
    [Header("Currency")]
    public TypeCurrency typeCurrency;
    public int price;
    [Header("IAP")]
    public IAPProductData data;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public enum EShopType
    {
        None,
        Ads,
        Currencies,
        IAP,
    }

    [System.Serializable]
    public struct IncomeData
    {
        public TypeCurrency typeCurrency;
        public Sprite icon;
        public int amount;
    }
}

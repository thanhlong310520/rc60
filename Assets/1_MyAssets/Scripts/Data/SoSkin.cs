using Raccoon.EnumHolder;
using Raccoon.Purchase;
using UnityEngine;

[CreateAssetMenu(fileName = "SoSkin", menuName = "Data/SoSkin")]
public class SoSkin : ScriptableObject
{
    public string id;
    public TypeSkin typeSkin;
    public Sprite icon;
    public GameObject prefab;
    public EShopType typePay;
    public TypeCurrency typeCurrency;
    public long price;
    public bool tagNew;

    public IAPProductData productData;
}

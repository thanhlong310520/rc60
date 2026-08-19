using Raccoon;
using Raccoon.Controller;
using Raccoon.Purchase;
using Raccoon.Store;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RemoveAdsPopup : MonoBehaviour
{
    [SerializeField] Button btnBuy;

    public IAPProductData productData;

    //public Text txtPrice;

    //[TextArea(3, 10)]
    //public string startText;
    //[TextArea(3, 10)]
    //public string remainText;


    void Start()
    {
        //txtPrice.text = $"{SetPriceStore()}";

        productData.OnClick = OnBuySuccess;

        //InvokeRepeating(nameof(DelayLoadPrice), 5f, 5f);
    }

    private void DelayLoadPrice()
    {
        var priceStore = GameStoreController.GetPriceProductById(productData.id);
        if (string.IsNullOrEmpty(priceStore))
        {

        }
        else
        {
            CancelInvoke();
            var price = SetPriceStore();
            //txtPrice.text = $"{price}";
        }
    }

    private void OnEnable()
    {
        if (btnBuy != null)
            btnBuy.interactable = !GameStoreController.Get.IsSubscribedTo(productData.id);
    }

    private string SetPriceStore()
    {
        var price = GameStoreController.GetPriceProductById(productData.id);
        if (string.IsNullOrEmpty(price))
            price = productData.price_default + "$";

        return price;
    }

    private void OnBuySuccess(bool success)
    {
        print("onbuy Success " + success);
        if (success)
        {
            GameData.Get.BuyRemoveAdsIAP();
            productData.OnCheckButton?.Invoke(success);
            if (btnBuy != null)
                btnBuy.interactable = !GameStoreController.Get.IsSubscribedTo(productData.id);
        }
    }

}

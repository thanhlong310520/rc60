using Raccoon;
using Raccoon.EnumHolder;
using Raccoon.Purchase;
using Raccoon.Store;
using Raccoon.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn lên prefab 1 ô shop. Nhận 1 SoShop, tự đổi UI theo EShopType
/// và xử lý hành vi khi bấm nút.
/// </summary>
public class ShopItem : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private SoShop shop;

    [Header("Common")]
    [SerializeField] private Button button;

    [Header("Group theo type")]
    [SerializeField] private GameObject adsGroup;
    [SerializeField] private GameObject currencyGroup;
    [SerializeField] private GameObject iapGroup;

    [Header("Currency")]
    [SerializeField] private Image priceIcon;
    [SerializeField] private TMP_Text priceText;

    [Header("IAP")]
    [SerializeField] private TMP_Text iapPriceText;
    [SerializeField] private List<IncomeView> _incomeViews = new List<IncomeView>();
    /// <summary>Bắn ra khi mua / nhận thưởng thành công.</summary>
    public event Action<ShopItem> OnPurchaseSuccess;
    public event Action<ShopItem, string> OnPurchaseFailed;

    public SoShop Shop => shop;
    private bool _isProcessing;

    #region Unity

    private void Reset()
    {
        button = GetComponentInChildren<Button>();
    }

    private void Awake()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
        }
    }

    private void OnEnable()
    {
        Refresh();
    }

    #endregion

    #region Setup / Refresh

    /// <summary>Gọi từ ShopController khi spawn item.</summary>
    public void Setup(SoShop data)
    {
        shop = data;
        Refresh();
    }

    public void Refresh()
    {
        if (shop == null)
        {
            gameObject.SetActive(false);
            return;
        }

        BuildIncomes();

        SetActiveSafe(adsGroup, shop.type == EShopType.Ads);
        SetActiveSafe(currencyGroup, shop.type == EShopType.Currencies);
        SetActiveSafe(iapGroup, shop.type == EShopType.IAP);

        switch (shop.type)
        {
            case EShopType.Ads:
                if (button != null)
                {
                    SetInteractable(IsAdReady());
                }
                break;

            case EShopType.Currencies:
                if (priceText != null)
                {
                    priceText.text = shop.price.ConvertCurrencyToString();
                }
                if (priceIcon != null)
                {
                    priceIcon.sprite = GetCurrencySprite(shop.typeCurrency);
                }
                if (button != null)
                {
                    SetInteractable(CanAfford());
                }
                break;

            case EShopType.IAP:
                if (iapPriceText != null)
                {
                    iapPriceText.text = GetIapPriceString();
                }
                if (button != null)
                {
                    SetInteractable(true);
                }
                break;

            default:
                if (button != null)
                {
                    SetInteractable(false);
                }
                break;
        }
    }

    public void SetInteractable(bool value)
    {
        button.interactable = value;
    }

    private void BuildIncomes()
    {
        // Bật/tắt lại view đã có thay vì Destroy liên tục
        int needed = shop.incomeDatas != null ? shop.incomeDatas.Count : 0;

        for (int i = 0; i < _incomeViews.Count; i++)
        {
            bool used = i < needed;

            if (used)
            {
                var data = shop.incomeDatas[i];
                _incomeViews[i].Bind(data);
            }
        }
    }

    private static void SetActiveSafe(GameObject go, bool value)
    {
        if (go != null && go.activeSelf != value)
        {
            go.SetActive(value);
        }
    }

    #endregion

    #region Click

    private void OnClick()
    {
        if (shop == null || _isProcessing)
        {
            return;
        }

        _isProcessing = true;

        switch (shop.type)
        {
            case EShopType.Ads:
                ShowRewardedAd(success =>
                {
                    if (success)
                    {
                        GrantIncomes();
                        Success();
                    }
                    else
                    {
                        Fail("Ads chưa sẵn sàng hoặc bị bỏ giữa chừng");
                    }
                });
                break;

            case EShopType.Currencies:
                if (TrySpend(shop.typeCurrency, shop.price))
                {
                    GameData.Get.PayCurrency(shop.typeCurrency, shop.price);
                    GrantIncomes();
                    Success();
                }
                else
                {
                    Fail("Không đủ tiền");
                }
                break;

            case EShopType.IAP:
                Purchase(shop.data, success =>
                {
                    if (success)
                    {
                        GrantIncomes();
                        Success();
                    }
                    else
                    {
                        Fail("Mua thất bại");
                    }
                });
                break;

            default:
                _isProcessing = false;
                break;
        }
    }

    private void Success()
    {
        _isProcessing = false;
        Refresh();
        OnPurchaseSuccess?.Invoke(this);
    }

    private void Fail(string reason)
    {
        _isProcessing = false;
        Refresh();
        OnPurchaseFailed?.Invoke(this, reason);
    }

    private void GrantIncomes()
    {
        if (shop.incomeDatas == null)
        {
            return;
        }

        foreach (var income in shop.incomeDatas)
        {
            AddCurrency(income.typeCurrency, income.amount);
        }
    }

    #endregion

    #region Hook vào hệ thống của bạn — thay code thật vào đây

    private Sprite GetCurrencySprite(TypeCurrency type)
    {
        return GameData.Get.GetIconCurrencyByType(type);
    }

    private bool CanAfford()
    {
        // return CurrencyManager.Instance.Get(shop.typeCurrency) >= shop.price;
        return true;
    }

    private bool TrySpend(TypeCurrency type, long amount)
    {
        // return CurrencyManager.Instance.TrySpend(type, amount);
        long c = GameData.Get.GetCurrency(type);

        return c >= amount;
    }

    private void AddCurrency(TypeCurrency type, long amount)
    {
        GameData.Get.AddIncome(type, amount);
    }

    private bool IsAdReady()
    {
        // return AdsManager.Instance.IsRewardedReady;
        return true;
    }

    private void ShowRewardedAd(Action<bool> callback)
    {
        // AdsManager.Instance.ShowRewarded(callback);
        callback?.Invoke(true);
    }

    private string GetIapPriceString()
    {
        if(shop.data == null)
        {
            return "N/A";
        }

        var price = GameStoreController.GetPriceProductById(shop.data.id);
        if (string.IsNullOrEmpty(price))
            price = shop.data.price_default + "$";

        return price;

    }

    private void Purchase(IAPProductData product, Action<bool> callback)
    {
        //if (!GameStoreController.Get.IsSubscribedTo(product.id))
        //{
        //    GameStoreController.Get.OnBuyProduct(product, callback);
        //}
        GameStoreController.Get.OnBuyProduct(product, callback);
    }

    [System.Serializable]
    struct IncomeView
    {
        public Image icon;
        public TMP_Text amountText;
        public void Bind(IncomeData data)
        {
            if (icon != null)
            {
                icon.sprite = data.icon;
            }
            if (amountText != null)
            {
                amountText.text = data.amount.ConvertCurrencyToString();
            }
        }
    }
    #endregion
}

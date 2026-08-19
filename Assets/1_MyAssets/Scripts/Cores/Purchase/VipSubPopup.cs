using Raccoon.Store;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Raccoon.Purchase
{
    public class VipSubPopup : PopupCanvas
    {
        [SerializeField] Button btnBack;
        [SerializeField] Button btnBuy;

        public IAPProductData productData;

        public Text txtPrice;

        [TextArea(3, 10)]
        public string startText;
        [TextArea(3, 10)]
        public string remainText;

        public TextMeshProUGUI txtPolicy;

        void Start()
        {
            btnBack.onClick.AddListener(() => Hide());
            
            txtPrice.text = $"Only {SetPriceStore()} / week";
            txtPolicy.text = startText + " " + SetPriceStore() + remainText;

            productData.OnClick = OnBuySuccess;


            InvokeRepeating(nameof(DelayLoadPrice), 5f, 5f);
        }
        public override void DoneShow()
        {
            base.DoneShow();
            //GameAds.Get.HideBanner();
            Time.timeScale = 0;
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
                txtPrice.text = $"Only {price} / week";
                txtPolicy.text = startText + " " + price + remainText;
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
                GameData.Get.BuyVipIAP();
                productData.OnCheckButton?.Invoke(success);
                Hide();
            }
        }

        public void OnClickRestore()
        {
            GameStoreController.Get.CheckRestoreProductById(productData.id, (restore, id) =>
            {
                if (restore)
                {
                    //restore item
                    Hide();
                    // GameUtils.OnBuyPackSuccess(productData, btnBuy.transform);
                }
            });
        }


        public override void Hide()
        {
            Time.timeScale = 1;
            base.Hide();
            //GamePlayController.instance.ctsv.StartCountShow();
            GameData.Get.isShowVip = false;
            //GameAds.Get.ShowBanner();
        }

    }
}

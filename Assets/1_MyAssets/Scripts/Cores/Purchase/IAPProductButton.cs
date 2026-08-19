using Raccoon.Store;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Raccoon.Purchase
{
    public class IAPProductButton : MonoBehaviour
    {
        public IAPProductData productData;

        public Button btnClick;
        public Text txtPriceStore;
        public Action OnIAPBuySuccess;
        public bool hideWhenBought;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (btnClick == null)
                btnClick = gameObject.GetComponentInChildren<Button>();
            AddListener();

            SetPriceStore();

            CheckShowButton();
        }
        
        //check HideWhenBought
        private void CheckShowButton()
        {
            if(productData == null) return;
            if (!hideWhenBought) return;
            
            GameStoreController.Get.CheckRestoreProductById(productData.id, ((restore, id) =>
            {
                if (restore)
                    gameObject.SetActive(false);
            }));
        }

        private void AddListener()
        {
            if (btnClick != null)
                btnClick.onClick.AddListener(OnClickBuy);
        }

        private void SetPriceStore()
        {
            if (txtPriceStore == null) return;

            var price = GameStoreController.GetPriceProductById(productData.id);
            if (string.IsNullOrEmpty(price))
                price = "$" + productData.price_default.ToString();

            txtPriceStore.text = price;
        }

        public void OnClickBuy()
        {
            if (productData == null) return;
            //GameAds.Get.HideBanner();

            GameStoreController.BuyProductById(productData.id, (success, id) =>
            {
                Debug.Log("on buy done ");
                //GameAds.Get.ShowBanner();
                if (success)
                {
                    OnBuySuccess();
                }

                productData.OnClick?.Invoke(success);
                productData.OnCheckButton?.Invoke(success);
            });

        }

        private void OnBuySuccess()
        {
            if (productData == null) return;

            if (OnIAPBuySuccess != null)
                OnIAPBuySuccess?.Invoke();

            if (hideWhenBought)
            {
                gameObject.SetActive(false);
            }
        }

        public void OnClickRestore()
        {
            GameStoreController.Get.CheckRestoreProductById(productData.id, (restore, id) =>
            {
                if (restore)
                {
                    //restore item
                    OnBuySuccess();
                }
            });
            
        }
    }
}
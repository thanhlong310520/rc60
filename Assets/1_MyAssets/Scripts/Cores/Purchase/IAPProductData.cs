using System.Collections.Generic;
using UnityEngine;

#if USE_PURCHASH
using UnityEngine.Purchasing;
#endif

namespace Raccoon.Purchase
{
    [CreateAssetMenu(fileName = "IAP_product_", menuName = "Nami/Other/ProductIAP")]
    public class IAPProductData : ScriptableObject
    {
        public string id;
#if USE_PURCHASH
        public ProductType productType;
#endif

        public float price_default;
        public Subscribe_reward_time subscribe_Reward_Time;
        public bool has_noads;
        public bool vip;

        public System.Action<bool> OnClick;
        public System.Action<bool> OnCheckButton;

        public void OnSendCheckButton(bool success)
        {
            if (OnCheckButton != null)
                OnCheckButton?.Invoke(success);
        }
    }

    public enum Subscribe_reward_time
    {
        None = 0,
        Per_day,
        Per_Week,
        Per_Month,
        Per_Year,
    }
}
using UnityEngine;
using UnityEngine.Events;
using Raccoon.Store;

public class PopupShop : PopupCanvas
{
    public ShopItem statedPack;
    public override void Show(PopupType p, UnityAction afterPopup, object obj)
    {
        base.Show(p, afterPopup, obj);
        CheckOnButtonStartedPack();
    }

    void CheckOnButtonStartedPack()
    {
        if (statedPack == null) return;
        if(GameStoreController.Get.IsSubscribedTo(statedPack.Shop.data.id))
        {
            statedPack.SetInteractable(false);
        }
        else
        {
            statedPack.SetInteractable(true);
        }
    }
}

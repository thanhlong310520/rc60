
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class ItemSkin : MonoBehaviour
{
    public Image icon;
    public Image bg;
    public GameObject selected;
    public GameObject tagNew;
    public GameObject adsGO;
    public GameObject equipGO;
    public SoSkin data;
    UnityAction<ItemSkin> clickAciton;

    bool isUnlock;
    bool isUse;

    public void SetEventClick(UnityAction<ItemSkin> aciton)
    {
        clickAciton = aciton;
    }
    public virtual void SetData(SoSkin data, bool isUnlock, bool isUse)
    {
        this.data = data;
        this.isUnlock = isUnlock;
        this.isUse = isUse;
        SetSelected(isUse);
        SetIcon(data);
        tagNew.SetActive(data.tagNew);
    }
    void SetIcon(SoSkin data)
    {
        icon.sprite = data.icon;
    }
    public virtual void SetSelected(bool selected)
    {
        this.selected.SetActive(selected);
        isUse = selected;
        SetUnlock();
    }

   void SetUnlock()
    {
        if (isUse)
        {
            equipGO.SetActive(false);
            adsGO.SetActive(false);
        }
        else
        {
            bool offAds = !isUnlock && data.isAds;
            equipGO.SetActive(!offAds);
            adsGO.SetActive(offAds);
        }

    }
    public void OnClick()
    {
        clickAciton?.Invoke(this);
    }
}

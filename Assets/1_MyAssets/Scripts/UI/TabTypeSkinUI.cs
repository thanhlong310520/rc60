using Raccoon.EnumHolder;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TabTypeSkinUI : MonoBehaviour
{
    public TypeSkin type;
    public GameObject select;
    public GameObject unselect;
    Button button;

    public Action<TabTypeSkinUI> ActionClick;
    private void Start()
    {
        if(button == null) button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        ActionClick?.Invoke(this);
    }

    public void SetSelect(bool isselect)
    {
        select.SetActive(isselect);
        unselect.SetActive(!isselect);
    }
}

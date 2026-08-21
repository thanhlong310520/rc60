using Raccoon;
using UnityEngine;
using UnityEngine.Events;

public class PopupSetting : PopupCanvas
{

    public SoundType SoundType;
    [SerializeField] ButtonState sound;
    [SerializeField] ButtonState music;

    public override void Show(PopupType p, UnityAction afterPopup, object obj)
    {
        base.Show(p, afterPopup, obj);
        sound.SetSelect(PlayerData.Get.onSound);
        music.SetSelect(PlayerData.Get.onMusic);
    }
    public void OnClickButtonSound()
    {
        GameData.Get.ChangeOnSound(sound.SetSelect);
    }
    public void OnClickButtonMusic()
    {
        GameData.Get.ChangeOnMusic(SoundType, music.SetSelect);
    }
}
[System.Serializable]
struct ButtonState
{
    public GameObject selected;
    public GameObject unselected;

    public void SetSelect(bool isSelected)
    {
        selected.SetActive(isSelected);
        unselected.SetActive(!isSelected);
    }

}

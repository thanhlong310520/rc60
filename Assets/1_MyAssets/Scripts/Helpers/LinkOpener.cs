using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


public class LinkOpener : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text textComponent;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, eventData.position, null);

        if (linkIndex != -1)
        {
            string linkID = textComponent.textInfo.linkInfo[linkIndex].GetLinkID();

            Application.OpenURL(linkID);
            Debug.Log("Link opened: " + linkID);
        }
    }
}
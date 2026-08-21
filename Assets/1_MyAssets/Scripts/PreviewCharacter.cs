using Raccoon;
using Raccoon.EnumHolder;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PreviewCharacter : MonoBehaviour
{
    public Transform pivot;
    Dictionary<TypeSkin, SkinGO> characterInstances;

    private void Start()
    {
        GameData.Get.currentSkinSOs.ForEach(soSkin =>
        {
            SetCharacter(soSkin);
        });
    }

    public void SetCharacter(SoSkin soSkin)
    {
        if(characterInstances == null)
        {
            characterInstances = new Dictionary<TypeSkin, SkinGO>();
            foreach (TypeSkin val in Enum.GetValues(typeof(TypeSkin)))
            {
                characterInstances[val] = null;
            }
        }
        foreach (TypeSkin val in Enum.GetValues(typeof(TypeSkin)))
        {
            if(val == soSkin.typeSkin)
            {
                if (characterInstances[val] != null) Destroy(characterInstances[val].gameObject);

                GameObject characterInstance = Instantiate(soSkin.prefab, pivot);
                characterInstance.transform.localPosition = Vector3.zero;
                characterInstance.transform.localRotation = Quaternion.identity;
                characterInstances[val] = characterInstance.GetComponent<SkinGO>();
                SetLayerRecursively(characterInstance, LayerMask.NameToLayer("PreviewCharacter"));
            }
            if (characterInstances[val] != null) characterInstances[val].animator.Play("Dance", 0, 0);
        }
    }
    void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.transform) SetLayerRecursively(t.gameObject, layer);
    }
}

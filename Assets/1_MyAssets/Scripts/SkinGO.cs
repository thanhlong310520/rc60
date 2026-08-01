using System.Collections.Generic;
using UnityEngine;

public class SkinGO : MonoBehaviour
{
    public Animator animator;

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }
}

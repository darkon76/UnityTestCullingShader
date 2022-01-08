using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepVisible : MonoBehaviour
{
    private void OnBecameVisible()
    {
        CullingEvent.OnVisibilityChanged?.Invoke(gameObject, true);
    }

    private void OnBecameInvisible()
    {
        CullingEvent.OnVisibilityChanged?.Invoke(gameObject, false);
    }
}

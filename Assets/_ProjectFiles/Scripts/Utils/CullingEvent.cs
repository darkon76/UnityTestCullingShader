using UnityEngine;
using System;

public class CullingEvent
{
    public static Action<GameObject, bool> OnVisibilityChanged;
}

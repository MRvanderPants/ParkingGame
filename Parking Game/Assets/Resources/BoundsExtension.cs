using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoundsExtension {
    public static bool ContainBounds(this Bounds bounds, Bounds target) {
        return bounds.Contains(target.min) && bounds.Contains(target.max);
    }
}

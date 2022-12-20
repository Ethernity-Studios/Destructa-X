using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetUnloader
{
#if UNITY_EDITOR

    [UnityEditor.MenuItem("Assets/Unload Assets")]
    static void UnloadAssets()
    {
        Resources.UnloadUnusedAssets();
    }

#endif
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decal : MonoBehaviour
{
    private void Start()
    {
        Invoke(nameof(Destroy),7f);
    }

    void Destroy()
    {
        Destroy(gameObject);
    }
}

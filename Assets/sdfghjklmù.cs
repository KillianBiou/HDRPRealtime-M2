using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sdfghjklmù : MonoBehaviour
{
    [SerializeField]
    private Transform toLook;

    private void Update()
    {
        transform.rotation = toLook.rotation;
    }
}

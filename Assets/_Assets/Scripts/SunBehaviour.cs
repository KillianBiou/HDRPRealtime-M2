using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SunBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject itemToLook;

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(itemToLook.transform);
    }
}

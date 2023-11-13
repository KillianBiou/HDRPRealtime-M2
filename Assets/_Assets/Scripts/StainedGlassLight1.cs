using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class StainedGlassLight : MonoBehaviour
{

    [Header("Light Reference")]
    [SerializeField]
    private Light incomingReference;
    [SerializeField]
    private Light outgoingReference;

    private Vector2 scaleFactor;

    private void Start()
    {
        scaleFactor = Vector2.one;
    }

    // Update is called once per frame
    void Update()
    {
    }
}

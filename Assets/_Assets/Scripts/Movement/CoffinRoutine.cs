using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffinRoutine : MonoBehaviour
{
    [Header("Direction")]
    [SerializeField]
    private bool inLocalSpace;
    [SerializeField]
    private Vector3 direction;

    [Header("Movement Parameters")]
    [SerializeField]
    private float offsetMultiplier;
    [SerializeField]
    private float movementSpeed;

    private Vector3 basePosition;

    private void Start()
    {
        if (inLocalSpace)
            basePosition = transform.localPosition;
        else
            basePosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float movementOffset = Mathf.Sin(movementSpeed * Time.time) * offsetMultiplier;
        Vector3 newPos = basePosition + direction * movementOffset;
        if (inLocalSpace)
            transform.localPosition = newPos;
        else
            transform.position = newPos;
    }
}

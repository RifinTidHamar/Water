using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pingPongLerp : MonoBehaviour
{
    [Header("Target Transform to move")]
    public Transform target;

    [Header("Motion Settings")]
    public float distance = 2f;    // Total distance to move back and forth
    public float speed = 1f;       // Speed of motion

    private Vector3 startPos;

    void Start()
    {
        if (target == null)
            target = transform; // default to self if no target set

        startPos = target.position;
    }

    void Update()
    {
        // PingPong moves from 0 → distance → 0 → distance, etc.
        float offset = Mathf.PingPong(Time.time * speed, distance) - distance / 2f;

        // Apply movement on X-axis
        target.position = startPos + new Vector3(offset, 0f, 0f);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform player;
    public Rigidbody rb;
    public Vector3 offset;

    public float period = 5f, intensity = 0.1f;

    private void Start()
    {
        transform.position = offset;
    }

    private void Update()
    {
        PlayerController pc = rb.GetComponent<PlayerController>();

        float theta = 0;

        float speedMult = 1;

        if(pc.sprinting)
        {
            speedMult = (pc.sprintMaxSpeed / pc.walkMaxSpeed) * 0.75f;
        }
        else { speedMult = 1; }

        if (!pc.crouching && pc.InputControls!= Vector2.zero && pc.grounded)
        {
            theta = Time.timeSinceLevelLoad / (period / pc.InputControls.magnitude / speedMult);
        }
        
        float distance = rb.velocity.normalized.magnitude * Mathf.Sin(theta) * intensity;

        Vector3 _offset = offset + Vector3.up * distance;

        transform.position = player.transform.position + _offset;
    }
}

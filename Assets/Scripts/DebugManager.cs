using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    public GameObject player;

    public Text velTxt;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    public void Update()
    {
        velTxt.text = "Velocity: " + new Vector3(player.GetComponent<Rigidbody>().velocity.x, 0, player.GetComponent<Rigidbody>().velocity.y).magnitude;
    }
}
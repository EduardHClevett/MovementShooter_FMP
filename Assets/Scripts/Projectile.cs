using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float travelSpeed = 5f;
    public float damage = 10f;



    public void SetStats(float projectileDamage, float projectileVelocity)
    {
        damage = projectileDamage;  travelSpeed = projectileVelocity;
    }

    private void Update()
    {
        transform.Translate(transform.forward * travelSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(this.gameObject);
    }
}

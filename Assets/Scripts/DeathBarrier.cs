using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBarrier : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        IEntity entity = collision.gameObject.GetComponent<IEntity>();
        if (entity != null)
        {
            collision.gameObject.GetComponent<IEntity>().KillEntity();
        }
    }
}

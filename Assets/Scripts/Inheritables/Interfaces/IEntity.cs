using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntity
{
    public float maxHealth { get; set; }
    public float currentHealth { get; set; }

    public void Initialize();

    public void TakeDamage(float damage);

    public void KillEntity();
}
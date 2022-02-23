using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Data Object", menuName = "Scriptable Objects/Weapon Data Object", order = 1)]
public class WeaponDataObject : ScriptableObject
{
    public string weaponName;
    public float damage, headshotMultiplier, fireRate, bulletVelocity, spread, recoil, reloadTime;
    public int magCapacity, extraMags, startReserves, burstCount;
}

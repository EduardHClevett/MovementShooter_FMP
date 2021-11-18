using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Data", menuName = "Scriptable Objects/Weapon Data Scriptable Object", order = 1)]
public class WeaponSO : ScriptableObject
{
    #region Weapon Stats
    public string weaponName;
    public WeaponEnums.WeaponType weaponClass;
    [Space]
    public float damage;
    public float headshotMultiplier;
    public WeaponEnums.FireMode fireMode;
    public float fireRate;
    public int burstCount;
    [Space]
    public int maxMagAmmo;
    public int maxReserveAmmo;
    public WeaponEnums.ReloadType reloadMode;
    [Tooltip("Timing works differently between reload modes. \nMagazine and recharge uses the time as the full duration. \nInsertion uses it as time per cartridge of ammunition.")]
    public float reloadTime;
    [Space]
    public Mesh weaponMesh;
    public Vector3 meshScale, meshRotation, meshPosition;
    [Space]
    public GameObject projectile;
    public Vector3 muzzlePointOffset;
    public float projectileVelocity;
    #endregion

    #region Runtime Stats
    public int currentMagAmmo;
    public int currentReserveAmmo;
    #endregion
}
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
    [Space]
    public int maxMagAmmo;
    public int maxReserveAmmo;
    public WeaponEnums.ReloadType reloadMode;
    [Space]
    public Mesh weaponMesh;
    public Vector3 meshScale, meshRotation, meshPosition;
    #endregion
}
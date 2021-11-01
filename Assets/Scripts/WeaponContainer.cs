using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponContainer : MonoBehaviour
{
    public WeaponSO weaponOne;
    public WeaponSO weaponTwo;
    public WeaponSO currentWeapon;

    public MeshFilter localMesh;

    private void Awake()
    {
        EquipWeapon(weaponOne);
    }

    public void EquipWeapon(WeaponSO weapon)
    {
        currentWeapon = weapon;

        MeshUpdate();
    }
    public void MeshUpdate()
    {
        localMesh.mesh = currentWeapon.weaponMesh;

        transform.localPosition = currentWeapon.meshPosition;
        transform.localRotation = Quaternion.Euler(currentWeapon.meshRotation);
        transform.localScale = currentWeapon.meshScale;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            EquipWeapon(weaponOne);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            EquipWeapon(weaponTwo);
    }
}
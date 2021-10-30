using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponContainer : MonoBehaviour
{
    public WeaponSO weaponSO;

    public MeshFilter localMesh;

    public void OnValidate()
    {
        localMesh.mesh = weaponSO.weaponMesh;

        transform.position = weaponSO.meshPosition;
        transform.rotation = Quaternion.Euler(weaponSO.meshRotation);
        transform.localScale = weaponSO.meshScale;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponContainer_AI : MonoBehaviour
{
    public WeaponSO AI_Weapon;

    public MeshFilter localMesh;

    public Transform muzzlePoint;

    [SerializeField]
    float fireTimer = 0;

    [SerializeField]
    public bool isFiring;

    private void Awake()
    {
        EquipWeapon(AI_Weapon);
    }

    public void EquipWeapon(WeaponSO weapon)
    {
        muzzlePoint.localPosition = AI_Weapon.muzzlePointOffset;
        fireTimer = 1 / AI_Weapon.fireRate;
        MeshUpdate();
    }
    public void MeshUpdate()
    {
        localMesh.mesh = AI_Weapon.weaponMesh;

        transform.localPosition = AI_Weapon.meshPosition;
        transform.localRotation = Quaternion.Euler(AI_Weapon.meshRotation);
        transform.localScale = AI_Weapon.meshScale;
    }

    private void Update()
    {
        if (fireTimer < (1 / AI_Weapon.fireRate)) fireTimer += Time.deltaTime;

        if(isFiring)
        {
            switch(AI_Weapon.fireMode)
            {
                case WeaponEnums.FireMode.FullAuto:
                    {
                        StartCoroutine(Fire());
                        break;
                    }
                case WeaponEnums.FireMode.BurstFire:
                    {
                        StartCoroutine(Fire(AI_Weapon.burstCount));
                        break;
                    }
                case WeaponEnums.FireMode.SemiAuto:
                    {
                        StartCoroutine(Fire());
                        break;
                    }
            }
        }
    }

    IEnumerator Fire(int repeats = 1)
    {
        while(true)
        {
            if (fireTimer < (1 / AI_Weapon.fireRate) || !isFiring) yield break;

            for (int i = 0; i < repeats; i++)
            {
                Debug.Log("Weapon fired! Weapon shot: " + AI_Weapon.weaponName);

                GameObject bullet = Instantiate(AI_Weapon.projectile, muzzlePoint.position, Quaternion.identity);

                Vector3 dir = muzzlePoint.forward;

                bullet.GetComponent<Projectile>().SetStats(dir, AI_Weapon.damage, AI_Weapon.projectileVelocity);

                fireTimer = 0f;

                if (AI_Weapon.fireMode == WeaponEnums.FireMode.BurstFire) yield return new WaitForSeconds(0.1f);
            }
            if (AI_Weapon.fireMode != WeaponEnums.FireMode.FullAuto)
            {
                isFiring = false;
                yield break;
            }
        }
    }

    public bool HasWeapon(WeaponSO weapon)
    {
        if (AI_Weapon == weapon) return true;

        return false;
    }
}
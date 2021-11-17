using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponContainer : MonoBehaviour
{
    public WeaponSO weaponOne;
    public WeaponSO weaponTwo;
    public WeaponSO currentWeapon;

    public MeshFilter localMesh;

    public Transform muzzlePoint;

    public PlayerInputs input;
    [SerializeField]
    float fireTimer = 0;
    int shotsFired;

    bool isReloading;
    [SerializeField]
    bool isFiring;

    private void Awake()
    {
        EquipWeapon(weaponOne);

        input = new PlayerInputs();
        input.InGame.Fire.started += _ => isFiring = true;
        input.InGame.Fire.canceled += _ => isFiring = false;

        input.InGame.SwapWeapon.performed += _ => SwapWeapon();
        input.InGame.Reload.performed += _ => StartCoroutine(StartReload());

        weaponOne.currentMagAmmo = weaponOne.maxMagAmmo;
        weaponOne.currentReserveAmmo = weaponOne.maxReserveAmmo;
        weaponTwo.currentMagAmmo = weaponTwo.maxMagAmmo;
        weaponTwo.currentReserveAmmo = weaponTwo.maxReserveAmmo;
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    public void EquipWeapon(WeaponSO weapon)
    {
        currentWeapon = weapon;
        muzzlePoint.localPosition = currentWeapon.muzzlePointOffset;
        fireTimer = 1 / currentWeapon.fireRate;
        shotsFired = currentWeapon.maxMagAmmo - currentWeapon.currentMagAmmo;
        MeshUpdate();
    }
    public void MeshUpdate()
    {
        localMesh.mesh = currentWeapon.weaponMesh;

        transform.localPosition = currentWeapon.meshPosition;
        transform.localRotation = Quaternion.Euler(currentWeapon.meshRotation);
        transform.localScale = currentWeapon.meshScale;
    }

    void SwapWeapon()
    {
        if (currentWeapon == weaponOne)
        {
            EquipWeapon(weaponTwo);
            return;
        }
        if (currentWeapon == weaponTwo)
        {
            EquipWeapon(weaponOne);
            return;
        }
    }

    private void Update()
    {
        if (fireTimer < (1 / currentWeapon.fireRate)) fireTimer += Time.deltaTime;

        if(isFiring)
        {
            switch(currentWeapon.fireMode)
            {
                case WeaponEnums.FireMode.FullAuto:
                    {
                        StartCoroutine(Fire());
                        break;
                    }
                case WeaponEnums.FireMode.BurstFire:
                    {
                        StartCoroutine(Fire(currentWeapon.burstCount));
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
            if (fireTimer < (1 / currentWeapon.fireRate) || isReloading || !isFiring) yield break;

            for (int i = 0; i < repeats; i++)
            {
                Debug.Log("Weapon fired! Weapon shot: " + currentWeapon.weaponName);

                if (currentWeapon.currentMagAmmo <= 0)
                {
                    shotsFired = currentWeapon.maxMagAmmo;
                    yield break;
                }

                GameObject bullet = Instantiate(currentWeapon.projectile, muzzlePoint.position, Quaternion.identity);

                bullet.GetComponent<Projectile>().SetStats(currentWeapon.damage, currentWeapon.projectileVelocity);

                currentWeapon.currentMagAmmo--;

                fireTimer = 0f;

                shotsFired = currentWeapon.maxMagAmmo - currentWeapon.currentMagAmmo;

                if (currentWeapon.fireMode == WeaponEnums.FireMode.BurstFire) yield return new WaitForSeconds(0.1f);
            }
            if (currentWeapon.fireMode != WeaponEnums.FireMode.FullAuto)
            {
                isFiring = false;
                yield break;
            }
        }
    }

    IEnumerator StartReload()
    {
        if (isReloading || currentWeapon.currentMagAmmo >= currentWeapon.maxMagAmmo || currentWeapon.currentReserveAmmo <= 0)
        {
            yield break;
        }

        isReloading = true;

        //Original Logic

        yield return new WaitForSeconds(currentWeapon.reloadTime);

        if((currentWeapon.currentReserveAmmo < currentWeapon.maxMagAmmo) && ((currentWeapon.currentMagAmmo + currentWeapon.currentReserveAmmo) <= currentWeapon.maxMagAmmo))
        {
            currentWeapon.currentMagAmmo += currentWeapon.currentReserveAmmo;
            currentWeapon.currentReserveAmmo = 0;
        }
        else
        {
            currentWeapon.currentMagAmmo = currentWeapon.maxMagAmmo;
            currentWeapon.currentReserveAmmo -= shotsFired;
        }

        //New Logic
        
        switch(currentWeapon.reloadMode)
        {
            case WeaponEnums.ReloadType.Magazine:
                //Magazine Here
                break;
            case WeaponEnums.ReloadType.Insertion:
                //Insertion Here
                break;
            case WeaponEnums.ReloadType.Recharge:
                //Recharge Here
                break;
        }

        isReloading = false;

        shotsFired = 0;
    }

    public bool HasWeapon(WeaponSO weapon)
    {
        if (weaponOne == weapon || weaponTwo == weapon) return true;

        return false;
    }
}
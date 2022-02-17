using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

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

    [Space, Header("UI Stuff")]
    public TextMeshProUGUI ammoTxt;
    public Image crosshair;

    Vector3 firingDir;

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

        if(weaponTwo != null)
        {
            weaponTwo.currentMagAmmo = weaponTwo.maxMagAmmo;
            weaponTwo.currentReserveAmmo = weaponTwo.maxReserveAmmo;
        }
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

        ammoTxt.text = (currentWeapon.currentMagAmmo + "/" + currentWeapon.currentReserveAmmo);

        Camera cam = Camera.main;

        RaycastHit camRay;

        bool hit = Physics.Raycast(cam.transform.position, cam.transform.forward, out camRay);

        if(hit)
        {
            if (camRay.collider.tag == "Player") return;


            firingDir = (camRay.point - muzzlePoint.position).normalized;

            Debug.DrawLine(muzzlePoint.position, camRay.point, Color.red);
        }
        else 
        {
            Vector3 screenCenter;

            screenCenter = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth / 2, cam.pixelHeight / 2, 1000));


            firingDir = (screenCenter - muzzlePoint.position).normalized; 
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

                bullet.GetComponent<Projectile>().SetStats(firingDir, currentWeapon.damage, currentWeapon.projectileVelocity);

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

        //New Logic
        
        switch(currentWeapon.reloadMode)
        {
            case WeaponEnums.ReloadType.Magazine:
                //Magazine Here
                yield return new WaitForSeconds(currentWeapon.reloadTime);

                if ((currentWeapon.currentReserveAmmo < currentWeapon.maxMagAmmo) && ((currentWeapon.currentMagAmmo + currentWeapon.currentReserveAmmo) <= currentWeapon.maxMagAmmo))
                {
                    currentWeapon.currentMagAmmo += currentWeapon.currentReserveAmmo;
                    currentWeapon.currentReserveAmmo = 0;
                }
                else
                {
                    currentWeapon.currentMagAmmo = currentWeapon.maxMagAmmo;
                    currentWeapon.currentReserveAmmo -= shotsFired;
                }
                break;
            case WeaponEnums.ReloadType.Insertion:
                //Insertion Here
                while (currentWeapon.currentMagAmmo != currentWeapon.maxMagAmmo)
                {
                    if (currentWeapon.currentReserveAmmo > 0)
                    {
                        yield return new WaitForSeconds(currentWeapon.reloadTime);
                        currentWeapon.currentMagAmmo++;
                        currentWeapon.currentReserveAmmo--;
                    }
                    else { break; }
                }
                break;
            case WeaponEnums.ReloadType.Recharge:
                //Recharge Here
                float reloadStep = currentWeapon.reloadTime / currentWeapon.maxMagAmmo;
                float t = reloadStep * currentWeapon.currentMagAmmo;
                
                while(t < currentWeapon.reloadTime - reloadStep)
                {
                    yield return new WaitForSeconds(reloadStep);
                    t += reloadStep;
                    currentWeapon.currentMagAmmo++;
                    currentWeapon.currentReserveAmmo--;
                }

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
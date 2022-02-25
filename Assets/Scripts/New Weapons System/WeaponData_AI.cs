using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WeaponData_AI : MonoBehaviour
{
    [SerializeField] WeaponDataObject data;

    [Header("Gun Details")]
    public WeaponType weaponType;
    public ReloadType reloadType;
    public FireMode fireMode;

    [Space, SerializeField] GameObject projectile;

    [Header("Internal References")]
    public Transform muzzlePoint;
    public Transform followPoint;
    public ParticleSystem muzzleFlash;
    public AudioSource gunshotSfx;

    private float fireTimer;

    public Transform target;

    [SerializeField]
    private bool isEnabled = true;

    public bool isFiring = false;

    public bool IsEnabled
    {
        get { return isEnabled; }
        set { isEnabled = value; }
    }

    void Update()
    {
        transform.position = followPoint.position;
        transform.rotation = followPoint.rotation;

        if (fireTimer < 1 / data.fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        if(isFiring)
        {
            switch (fireMode)
            {
                case FireMode.FullAuto:
                    {
                        StartCoroutine(Fire());
                        break;
                    }
                case FireMode.BurstFire:
                    {
                        StartCoroutine(Fire(data.burstCount));
                        break;
                    }
                case FireMode.SemiAuto:
                    {
                        StartCoroutine(Fire());
                        break;
                    }
            }
        }
    }

    IEnumerator DisableFire(float time = 0.3f)
    {
        isEnabled = false;
        isFiring = false;

        yield return new WaitForSeconds(time);
        isEnabled = true;

        yield break;
    }

    IEnumerator Fire(int repeats = 1)
    {
        while(true)
        {
            if (fireTimer < 1 / data.fireRate || !isEnabled ) yield break;

            for (int i = 0; i < repeats; i++)
            {

                if (gunshotSfx != null)
                    gunshotSfx.PlayOneShot(gunshotSfx.clip);

                GameObject bullet = Instantiate(projectile, muzzlePoint.position, Quaternion.identity);

                bullet.GetComponent<Projectile>().SetStats(target.position - transform.position, data.damage, data.bulletVelocity);

                fireTimer = 0f;

                if (muzzleFlash != null)
                    muzzleFlash.Play();

            }

            if(fireMode != FireMode.FullAuto)
            {
                isFiring = false;
                yield break;
            }
        }   
    }

    public void DrawObj()
    {
        StartCoroutine(PrepareWeapon());
    }

    IEnumerator PrepareWeapon()
    {
        yield return new WaitForEndOfFrame();

        isEnabled = true;

        yield break;
    }

    public void UnloadObj()
    {
        isFiring = false;
        isEnabled = false;

        gameObject.SetActive(false);
    }
}

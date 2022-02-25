using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { Pistol, SMG, AssaultRifle, SniperRifle, Shotgun, RocketLauncher };
public enum ReloadType { Magazine, Insertion, Recharge };
public enum FireMode { FullAuto, BurstFire, SemiAuto };

[System.Serializable]
public class PositionData
{
    public Vector3 position;
    public Quaternion rotation;
}

public class WeaponData : MonoBehaviour
{
    [SerializeField] WeaponDataObject data;

    [Header("Gun Details")]
    public WeaponType weaponType;
    public ReloadType reloadType;
    public FireMode fireMode;

    [Space, SerializeField] GameObject projectile;

    public int currentReserve { get; private set; }
    [Space]
    public int maxReserve;
    public int currentMag;
    private int shotsFired;

    [Header("Animation Details")]
    public PositionData aimPos;
    public float aimSpeed = 8f;

    [Header("External References")]
    public Camera cam;
    public WeaponManagerNew weaponManager;
    public PlayerInputs input;

    [Header("Internal References")]
    public Transform muzzlePoint;
    public ParticleSystem muzzleFlash;
    public AudioSource gunshotSfx;

    private float fireTimer;

    [SerializeField]
    private bool isReloading = false;
    private bool isEnabled = true;
    public PositionData originalPos { get; private set; }

    private float playerFOV;
    private float fovAdjust;
    private bool isAiming = false;
    Vector3 firingDir;

    bool isFiring = false;

    public bool IsEnabled
    {
        get { return isEnabled; }
        set { isEnabled = value; }
    }

    void Awake()
    {
        input = new PlayerInputs();

        originalPos = new PositionData();

        input.InGame.Fire.started += _ => isFiring = true;

        input.InGame.Fire.canceled += _ => isFiring = false;

        input.InGame.Reload.performed += _ => StartCoroutine(StartReload());

        input.InGame.Aim.performed += _ => StartADS();
        input.InGame.Aim.canceled += _ => StopADS();
    }

    void OnEnable()
    {
        input.Enable();
    }
    void OnDisable()
    {
        input.Disable();
    }

    void Start()
    {
        weaponManager = GetComponentInParent<WeaponManagerNew>();

        cam = Camera.main;

        playerFOV = cam.fieldOfView;
        fovAdjust = cam.fieldOfView - (cam.fieldOfView / 5);


        InitAmmo();

        originalPos.position = transform.localPosition;
        originalPos.rotation = transform.localRotation;

        if (!weaponManager.HasWeapon(this))
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        shotsFired = data.magCapacity - currentMag;

        if (fireTimer < 1 / data.fireRate)
        {
            fireTimer += Time.deltaTime;
        }


        if (currentMag == 0)
            StartCoroutine(StartReload());

        if (isAiming)
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovAdjust, 0.5f);
        else
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, playerFOV, 0.5f);

        RaycastHit camRay;

        bool hit = Physics.Raycast(cam.transform.position, cam.transform.forward, out camRay);

        if (hit)
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
            if (fireTimer < 1 / data.fireRate || !isEnabled || weaponManager.pauseFire || isReloading) yield break;

            for (int i = 0; i < repeats; i++)
            {
                if (currentMag <= 0)
                {
                    StartCoroutine(DisableFire());

                    shotsFired = data.magCapacity;
                    yield break;
                }

                if (gunshotSfx != null)
                    gunshotSfx.PlayOneShot(gunshotSfx.clip);

                GameObject bullet = Instantiate(projectile, muzzlePoint.position, Quaternion.identity);

                bullet.GetComponent<Projectile>().SetStats(firingDir, data.damage, data.bulletVelocity);

                currentMag--;

                fireTimer = 0f;

                shotsFired = data.magCapacity - currentMag;

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

    void StartADS()
    {
        isAiming = true;
        transform.localPosition = aimPos.position;
        transform.localRotation = aimPos.rotation;
    }
    void StopADS()
    {
        isAiming = false;
        transform.localPosition = originalPos.position;
        transform.localRotation = originalPos.rotation;
    }

    IEnumerator StartReload()
    {
        if (isReloading || currentMag >= data.magCapacity || currentReserve <= 0)
        {
            yield break;
        }

        isReloading = true;


        yield return new WaitForSeconds(data.reloadTime);

        if ((currentReserve < data.magCapacity) && ((currentMag + currentReserve) <= data.magCapacity))
        {
            currentMag += currentReserve;
            currentReserve = 0;
        }
        else
        {
            currentMag = data.magCapacity;
            currentReserve -= shotsFired;
        }

        isReloading = false;
        
        shotsFired = 0;
    }

    public void DrawObj()
    {
        StartCoroutine(PrepareWeapon());
    }

    public void InitAmmo()
    {
        currentReserve = data.startReserves;
        currentMag = data.magCapacity;
        maxReserve = data.magCapacity * data.extraMags;
    }

    IEnumerator PrepareWeapon()
    {
        yield return new WaitForEndOfFrame();

        isEnabled = true;

        yield break;
    }

    public void UnloadObj()
    {
        isReloading = false;
        isFiring = false;
        transform.localRotation = originalPos.rotation;
        isEnabled = false;

        gameObject.SetActive(false);
    }
}

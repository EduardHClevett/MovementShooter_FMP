using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManagerNew : MonoBehaviour
{
    public GameObject primaryWeapon;
    public GameObject secondaryWeapon;

    [HideInInspector] public GameObject currentWeapon;
    public GameObject startWeapon;

    [HideInInspector] public bool pauseFire = false;

    public PlayerInputs input;

    void Awake()
    {
        input = new PlayerInputs();

        input.InGame.SwapWeapon.started += _ => SwitchWeapon();
    }

    void Start()
    {
        primaryWeapon = null;
        secondaryWeapon = startWeapon;

        currentWeapon = secondaryWeapon;

        if(currentWeapon != null)
            StartCoroutine(Init());
    }

    void OnEnable()
    {
        input.Enable();
    }
    void OnDisable()
    {
        input.Disable();
    }

    IEnumerator Init()
    {
        currentWeapon.SetActive(true);

        yield return new WaitForSeconds(0.1f);
        currentWeapon.GetComponent<WeaponData>().DrawObj();

        yield return new WaitForEndOfFrame();
        currentWeapon.GetComponent<WeaponData>().UpdateUI();

        yield break;
    }

    public bool HasWeapon(WeaponData weapon)
    {
        if ((primaryWeapon != null && primaryWeapon.GetComponent<WeaponData>() == weapon) || (secondaryWeapon != null && secondaryWeapon.GetComponent<WeaponData>() == weapon)) return true;

        return false;
    }

    void SwitchWeapon()
    {
        if (primaryWeapon != null && currentWeapon != primaryWeapon)
        {
            currentWeapon.GetComponent<WeaponData>().UnloadObj();

            currentWeapon = primaryWeapon;

            currentWeapon.SetActive(true);
            currentWeapon.GetComponent<WeaponData>().DrawObj();
        }
        else if (secondaryWeapon != null && currentWeapon != secondaryWeapon)
        {
            currentWeapon.GetComponent<WeaponData>().UnloadObj();

            currentWeapon = secondaryWeapon;

            currentWeapon.SetActive(true);
            currentWeapon.GetComponent<WeaponData>().DrawObj();
        }
    }
}

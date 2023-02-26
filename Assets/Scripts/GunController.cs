using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Gun equippedGun;
    public Gun defaultGun;
    public Transform gunHold;

    void Start()
    {
        if (defaultGun != null)
        {
            EquipGun(defaultGun);
        }
     }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void EquipGun(Gun gunToEquip)
    {
        if (equippedGun != null) 
        {
            Destroy(equippedGun.gameObject);
        }
        equippedGun = Instantiate(gunToEquip, gunHold.position, gunHold.rotation);
        equippedGun.transform.parent = gunHold;
    }
}

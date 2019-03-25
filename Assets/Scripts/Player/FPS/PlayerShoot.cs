﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;

public class PlayerShoot : NetworkBehaviour {
    public Camera Cam { get; set; }

    public PlayerEquipment Equipment { get; set; }

    public GameObject Cross;

    [SerializeField] private LayerMask _mask;
    private bool _shootingDone = false;


    public Animator weaponAnimator;

    // Start is called before the first frame update
    void Start() {
        if (Cam == null) enabled = false;
        else Cross = GameObject.Find("cross");
    }


    void Update() {
        //fire mode
        if (Input.GetKeyDown(KeyCode.B)) Equipment.Weapon.changeFireMode();

        //reloading
        if (Input.GetKeyDown(KeyCode.R) && Equipment.Weapon.CurrentMagAmmo != Equipment.Weapon.MaxMagAmmo) Equipment.Weapon.reload();

        //fireing
        if (Input.GetButton("Fire1") && Equipment.Weapon.State == PlayerWeapon.WeaponState.idle &&
            !PauseGame.menuActive && Equipment.Weapon.CurrentMagAmmo >= 1)  {
           Shoot();
        }
        if (Input.GetButtonUp("Fire1"))
            _shootingDone = false;

        //aiming
        if (Input.GetButton("Fire2") && Equipment.Weapon.State == PlayerWeapon.WeaponState.idle &&
    !PauseGame.menuActive)
        {
            //scope();
        }

    }

    IEnumerator TripleShot() {
        PerformWeaponFire();
        yield return new WaitForSeconds(Equipment.Weapon.FireRate * 0.8f);
        PerformWeaponFire();
        yield return new WaitForSeconds(Equipment.Weapon.FireRate * 0.8f);
        PerformWeaponFire();
        yield return new WaitForSeconds(Equipment.Weapon.FireRate * 0.8f);
    }

    void Shoot() {
       
        if (Equipment.Weapon.Mode == PlayerWeapon.FireMode.single && !_shootingDone) {
            PerformWeaponFire();
            _shootingDone = true;
        }
        else if (Equipment.Weapon.Mode == PlayerWeapon.FireMode.triple && !_shootingDone) {
            Equipment.Weapon.Recoil = Equipment.Weapon.Recoil / 2;
            StartCoroutine(TripleShot());
            Equipment.Weapon.Recoil = Equipment.Weapon.Recoil * 2;
            _shootingDone = true;
        }
        else if (Equipment.Weapon.Mode == PlayerWeapon.FireMode.continous) {
            PerformWeaponFire();
        }
        
    }
    
    

    void PerformWeaponFire() {
        if (Equipment.Weapon.CurrentMagAmmo >= 1) {
            Equipment.PlayerShooting();
            Equipment.Weapon.shoot();
            gameObject.GetComponent<PlayerMotor>().IncreaseRecoil(Equipment.Weapon.Recoil);
            CmdPlayerShooting();
            RaycastHit hit;
            if (Physics.Raycast(Cam.transform.position, Cam.transform.forward, out hit, Equipment.Weapon.Range,
                _mask)) {
                Debug.Log("We hit " + hit.collider.name);
                if (hit.collider.tag == "Player")
                    CmdPlayerShoot(hit.collider.GetComponentInParent<PlayerManager>().transform.name,
                        Equipment.Weapon.Damage);
                else if (hit.collider.tag == "EnemyHead")
                    CmdEnemyShoot(hit.collider.GetComponentInParent<NavMeshAgent>().transform.name,
                        3 * Equipment.Weapon.Damage);
                else if (hit.collider.tag == "EnemyBody")
                    CmdEnemyShoot(hit.collider.GetComponentInParent<NavMeshAgent>().transform.name,
                        2 * Equipment.Weapon.Damage);
                else if (hit.collider.tag == "EnemyLegs")
                    CmdEnemyShoot(hit.collider.GetComponentInParent<NavMeshAgent>().transform.name,
                        Equipment.Weapon.Damage);

                Equipment.DoHitEffect(hit.point, hit.normal);
                CmdOnHit(hit.point, hit.normal);
            }

            if (Equipment.Weapon.CurrentMagAmmo == 0 && Equipment.Weapon.CurrentAmmo >= 1) Equipment.Weapon.reload();
        }
    }


    [Command]
    void CmdPlayerShooting() {
        Equipment.RpcPlayerShooting();
    }

    [Command]
    void CmdOnHit(Vector3 hitPoint, Vector3 normal) {
        Equipment.RpcDoHitEffect(hitPoint, normal);
    }

    [Command]
    void CmdEnemyShoot(string shootEnemyId, float damage) {
        Debug.Log(shootEnemyId + " has been shoot");
        GameManager.GetEnemy(shootEnemyId).TakeDamage(damage);
    }


    public void InvokeCmdPlayerShoot(string shootPlayerId, float damage) {
        CmdPlayerShoot(shootPlayerId, damage);
    }

    [Command]
    void CmdPlayerShoot(string shootPlayerId, float damage) {
        Debug.Log(shootPlayerId + " has been shoot");
        GameManager.GetPlayer(shootPlayerId).RpcTakeDamage(damage);
    }
}
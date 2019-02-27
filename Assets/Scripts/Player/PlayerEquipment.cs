﻿
using UnityEngine;
using UnityEngine.Networking;

public class PlayerEquipment : NetworkBehaviour {
    public AudioSource WeaponSound { get; set; }
    public PlayerWeapon Weapon { get; set; }


    public void PlayerShooting() {
        Weapon.Flash.Play();
        WeaponSound.Play();
        GameObject smokeEffect =
            Instantiate(Weapon.Smoke, Weapon.transform.GetChild(0).position, Quaternion.Euler(-90, 0, 0));
        Destroy(smokeEffect, 2f);
    }


    [ClientRpc]
    public void RpcPlayerShooting() {
        if (!isLocalPlayer) {
            Weapon.Flash.Play();
            WeaponSound.Play();
            GameObject smokeEffect = Instantiate(Weapon.Smoke, Weapon.transform.GetChild(0).position,
                Quaternion.Euler(-90, 0, 0));
            Destroy(smokeEffect, 2f);
        }
    }

    public void DoHitEffect(Vector3 hitPoint, Vector3 normal) {
        GameObject hitEffect = Instantiate(Weapon.HitEffect, hitPoint, Quaternion.LookRotation(normal));
        Destroy(hitEffect, 2f);
    }


    [ClientRpc]
    public void RpcDoHitEffect(Vector3 hitPoint, Vector3 normal) {
        if (!isLocalPlayer) {
            GameObject hitEffect = Instantiate(Weapon.HitEffect, hitPoint, Quaternion.LookRotation(normal));
            Destroy(hitEffect, 2f);
        }
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Control;
namespace RPG.Combat
{
    public class WeaponPickup : MonoBehaviour, IRaycastable
    {
        [SerializeField] WeaponConfig weaponToEquip = null;
        [SerializeField] float respawnTime = 5;
        private void OnTriggerEnter(Collider other) {
            if(other.gameObject.tag == "Player")
            {
                PickUp(other.GetComponent<Fighter>());
            }

        }

        private void PickUp(Fighter fighter)
        {
            fighter.EquipWeapon(weaponToEquip);
            StartCoroutine(HideForSeconds(respawnTime));
        }

        private IEnumerator HideForSeconds(float seconds)
        {
            ShowPickup(false);
            yield return new WaitForSeconds(seconds);
            ShowPickup(true);

        }
   
        private void ShowPickup(bool shouldShow)
        {
            GetComponent<Collider>().enabled = shouldShow;

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(shouldShow);
            }

        }

        public bool HandleRaycast(PlayerController callingController)
        {
            if(Input.GetMouseButtonDown(0))
            {
                PickUp(callingController.GetComponent<Fighter>());
            }
            return true;
        }

        public CursorType GetCursorType()
        {
            return CursorType.Pickup;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using GameDevTV.Inventories;
using UnityEngine;
namespace RPG.Inventories
{
    public class InventorySpawner : MonoBehaviour
    {
        [System.Serializable]
        public struct InitialInventorySlot
        {
            public InventoryItem item;
            public int number;
        }
        [SerializeField] InitialInventorySlot[] items = null;
        // Start is called before the first frame update
        GameObject player;
        void Start()
        {
           // player = GameObject.FindGameObjectWithTag("player");
           
            Inventory inventory = Inventory.GetPlayerInventory();
            foreach(InitialInventorySlot itemSlot in items)
            {
                inventory.AddToFirstEmptySlot(itemSlot.item,itemSlot.number);
            }
        }

    }

}

using System.Collections;
using System.Collections.Generic;
using GameDevTV.Inventories;
using RPG.Attributes;
using RPG.Stats;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Inventories
{
    public class StatsActionStore : ActionStore
    { 
        GameObject player;

        void Awake()
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        public bool ItemUse(int index, GameObject user)
        {
            if (GetDockedItems().ContainsKey(index))
            {        
               /* ItemEffect effectInstance = Instantiate<ItemEffect>(GetDockedItems()[index].item.GetItemEffect());
                if(effectInstance.GetIsPermanentEffect())
                effectInstance.HealAction(player);*/

                GetDockedItems()[index].item.Use(user);     
                if (GetDockedItems()[index].item.isConsumable())
                {
                    RemoveItems(index, 1);
                }
                return true;
            }
            return false;
        }
        
        void Update()
        {
            if(Input.GetButtonDown("ActionBar1"))
            {
                ItemUse(0,player);
            }  
            if(Input.GetButtonDown("ActionBar2"))
            {
                ItemUse(1,player);
            }   
            if(Input.GetButtonDown("ActionBar3"))
            {
                ItemUse(2,player);
            }   
            if(Input.GetButtonDown("ActionBar4"))
            {
                ItemUse(3,player);
            }   
            if(Input.GetButtonDown("ActionBar5"))
            {
                ItemUse(4,player);
            }   
            if(Input.GetButtonDown("ActionBar6"))
            {
                ItemUse(5,player);
            }    
        

        }

    }


}

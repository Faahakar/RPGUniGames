using System.Collections;
using System.Collections.Generic;
using GameDevTV.Inventories;
using RPG.Stats;
using UnityEngine;

namespace RPG.Inventories
    {
    public class StatsEquipment : Equipment, IModifierProvider
    {
         IEnumerable<float> IModifierProvider.GetAdditiveModifiers(Stat stat)
        {
            foreach(var slot in GetAllPopulatedSlots())
            {
                // Faz um cast como IModifierProvider, já que nem todo equipabbleitem tem modificadores, logo depois a gente checa
                // se o item tem modificador, se não, volta pro começo do foreach.
                var item = GetItemInSlot(slot) as IModifierProvider;
                if(item == null) continue;
                foreach(float modifier in item.GetAdditiveModifiers(stat))
                {
                     yield return modifier;
                }
            }
        }

         IEnumerable<float> IModifierProvider.GetPercentageModifiers(Stat stat)
        {
           foreach(var slot in GetAllPopulatedSlots())
            {
                
                var item = GetItemInSlot(slot) as IModifierProvider;
                if(item == null) continue;
                foreach(float modifier in item.GetPercentageModifiers(stat))
                {
                     yield return modifier;
                }
            }
        }
    }

}


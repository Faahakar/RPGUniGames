using System.Collections;
using System.Collections.Generic;
using RPG.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Stats
{
    public class TemporaryEffectObserver : MonoBehaviour, IModifierProvider
    {

        IEnumerable<float> IModifierProvider.GetAdditiveModifiers(Stat stat)
        {
            foreach(IModifierProvider effect in FindObjectsOfType<ItemEffect>())
            {
                if(effect== null) continue;
                
                foreach(float modifier in effect.GetAdditiveModifiers(stat))
                {
                    yield return modifier;
                }
            }
        }


         IEnumerable<float> IModifierProvider.GetPercentageModifiers(Stat stat)
        {
            foreach(IModifierProvider effect in FindObjectsOfType<ItemEffect>())
            {
                
                if(effect== null) continue;
                foreach(float modifier in effect.GetPercentageModifiers(stat))
                {
                    yield return modifier;
                }
            }
        }


    }

}

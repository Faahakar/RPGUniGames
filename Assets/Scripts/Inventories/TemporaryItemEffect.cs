using System;
using System.Collections;
using System.Collections.Generic;
using GameDevTV.Inventories;
using UnityEngine;
using UnityEngine.Events;
using RPG.Attributes;

namespace RPG.Stats
{

    public class TemporaryItemEffect : ItemEffect, IModifierProvider
    {
        [System.Serializable]
        public class ItemUsedEvent : UnityEvent<float,GameObject>
        {

        }
        [SerializeField] ItemUsedEvent health;
        [SerializeField]
        Modifier[] additiveModifiers;
        [SerializeField]
        Modifier[] percentageModifiers;
        [System.Serializable]
        struct Modifier
       {
           public Stat stat;
           public float value;
           public bool isPermanent;
       }
       [SerializeField] float effectTimer = 10f;
       [SerializeField] bool isPeriodic = false;
       [SerializeField] float periodTime = 5f;
       [SerializeField] bool isTemporary = false;
       bool effectUsed = false;
       GameObject user;
       void Awake()
       {
            user = GameObject.FindGameObjectWithTag("Player");
       }
        void Update()
        {
            if(!effectUsed)
            GetValue();
            if(isTemporary)
            TemporaryEffect();
        }


        public float GetValue()
        {
            if(additiveModifiers == null) return 0;
            float value = 0;
            for (int i = 0; i < additiveModifiers.Length; i++)
            {
                if(additiveModifiers[i].stat == Stat.Heal)
                {
                    value = additiveModifiers[i].value;
                    effectUsed = true;
                }              
            }
            if(percentageModifiers == null) return 0;
            float percentageValue = 0;
            for (int i = 0; i < percentageModifiers.Length; i++)
            {
                if(percentageModifiers[i].stat == Stat.Heal)
                {
                    percentageValue = percentageModifiers[i].value;
                    effectUsed = true;
                }              
            }
            float percentage = (percentageValue*user.GetComponent<BaseStats>().GetStat(Stat.Health))/100;
            float total = value+percentage;
            health.Invoke(total,user);
            return total;
        }
        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            foreach(Modifier modifier in additiveModifiers)
            {       
                    if(modifier.stat == stat)
                    {                 
                        if(TemporaryEffect() || modifier.isPermanent)
                        {
                            yield break;
                        }   
                
                        yield return modifier.value;
                    }
                    
            }
        }

        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
             foreach(Modifier modifier in percentageModifiers)
            {
                    if(modifier.stat == stat)
                    { 
                        if(TemporaryEffect() || modifier.isPermanent)
                        {
                            yield break;
                        }   
                        yield return modifier.value;
                    }
                    
            }
        }
        public bool TemporaryEffect()
        {          
            effectTimer -= Time.deltaTime;
            if(effectTimer < 0)
            {           
                effectTimer = 0;
                return true;
                
            }
            return false;
            
        }

    }
    

}
using UnityEngine;
using GameDevTV.Saving;
using RPG.Stats;
using RPG.Core;
using System;
using GameDevTV.Utils;
using UnityEngine.Events;

namespace RPG.Attributes
{
    public class Stamina : MonoBehaviour, ISaveable
    {
        LazyValue<float> staminaPoints;
        [SerializeField] GameObject player = null;

        private void Awake()
        {
            staminaPoints  = new LazyValue<float>(GetInitialStamina);
        }
        private float GetInitialStamina()
        {
            return 100f;
        }
        private void Start()
        {
            staminaPoints.ForceInit();
        }
        public void UseStamina(float value)
        {
            SetStaminaPoints(Mathf.Max(staminaPoints.value - value, 0));
            if(staminaPoints.value == 0)
            {

            }

        }
        public void GainStamina(float value)
        {
          if(gameObject != null)
          this.SetStaminaPoints(Mathf.Min(this.staminaPoints.value + value,this.GetMaxStamina()));
        }

        private float GetMaxStamina()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Stamina);
        }
         public float GetStaminaFraction()
         {
            return staminaPoints.value/GetComponent<BaseStats>().GetStat(Stat.Stamina);
         }
         public float GetStaminaPoints()
         {
            return staminaPoints.value;
         }

        public void SetStaminaPoints(float value)
        {
            staminaPoints.value = value;
        }
        public object CaptureState()
        {
            return staminaPoints.value;
        }

        public void RestoreState(object state)
        {
            staminaPoints.value = (float)state;
        }
    }
}
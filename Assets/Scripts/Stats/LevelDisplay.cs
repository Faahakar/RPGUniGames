using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace RPG.Stats
{

    public class LevelDisplay : MonoBehaviour 
    {
        BaseStats  baseStats;
        private void Awake()
        {
            baseStats = GameObject.FindWithTag("Player").GetComponent<BaseStats>();
        }
        private void Update()
        {
            SetLevelText();
           
        }
        private void SetLevelText()
        {
              GetComponent<Text>().text = System.String.Format("{0}",baseStats.GetLevel());
        }
        
    }

}
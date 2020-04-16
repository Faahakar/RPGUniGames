using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using RPG.Attributes;

namespace RPG.Combat
{
   
    public class EnemyHealthDisplay : MonoBehaviour 
    {
        
        Health health;
        Fighter fighter;
        private void Awake()
        {     
            fighter = GameObject.FindWithTag("Player").GetComponent<Fighter>();
        }
        private void Update()
        {
             health = fighter.GetTarget(); 
             SetHealthText();            
        }
        private void SetHealthText()
        {
             if(health == null) 
             {
                 GetComponent<Text>().text = "N/A";
                 return;
             }
             
             GetComponent<Text>().text = System.String.Format("{0:0}/{1:0}",health.GetHealthPoints(),health.GetMaxHealth());
        }
        
    }

}
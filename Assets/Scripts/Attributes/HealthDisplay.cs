using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace RPG.Attributes
{

    public class HealthDisplay : MonoBehaviour 
    {
        Health health;
        private void Awake()
        {
            health = GameObject.FindWithTag("Player").GetComponent<Health>();
        }
        private void Update()
        {
            SetHealthText();
           
        }
        private void SetHealthText()
        {
            GetComponent<Text>().text = System.String.Format("{0:0}/{1:0}",health.GetHealthPoints(),health.GetMaxHealth());
        }
        
    }

}
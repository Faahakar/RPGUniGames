using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace RPG.Stats
{

    public class ExperienceDisplay : MonoBehaviour 
    {
        Experience experience;
        private void Awake()
        {
            experience = GameObject.FindWithTag("Player").GetComponent<Experience>();
        }
        private void Update()
        {
            SetExpText();
           
        }
        private void SetExpText()
        {
              GetComponent<Text>().text = System.String.Format("{0}",experience.GetExperience());
        }
        
    }

}
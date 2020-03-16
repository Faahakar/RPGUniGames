using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
namespace RPG.Cinematics
{
    public class CinematicTrigger : MonoBehaviour
    {
        private bool triggered = false;
        private void OnTriggerEnter(Collider other)
        {
            if(!triggered  && other.gameObject.tag == "Player")
            {
                GetComponent<PlayableDirector>().Play();         
                triggered =  true;
            }
            
               
        }
    }

}


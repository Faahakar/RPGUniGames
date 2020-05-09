using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.StatueQuest
{

    public class StatueQuest : MonoBehaviour{

        public GameObject HUD_StatueQuest;
        public GameObject HUD_PressF;
        bool isPegandoQuest;

        void Start(){
            HUD_StatueQuest.SetActive (false);
            HUD_PressF.SetActive (false);            
        }

        void Update(){
            if(Input.GetKeyDown(KeyCode.F) && isPegandoQuest) {
                HUD_StatueQuest.SetActive (true);
                HUD_PressF.SetActive (false);       
            }
        }

        void OnTriggerEnter(Collider hit){
            if(hit.gameObject.tag == "Player"){
                //HUD_StatueQuest.SetActive (true);
                isPegandoQuest = true;
                HUD_PressF.SetActive (true);
            }
        }

        void OnTriggerExit(Collider hit){
            if(hit.gameObject.tag == "Player"){
                HUD_StatueQuest.SetActive (false);
                HUD_PressF.SetActive (false);
                isPegandoQuest = false;
            }
        }
    }
}

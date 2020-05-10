using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Attributes
{
    public class StaminaBar : MonoBehaviour
    {
        // Start is called before the first frame updateHealth health;
        [SerializeField] Stamina stamina = null;
        [SerializeField] RectTransform foreground = null;
        [SerializeField] Canvas rootCanvas = null;

        [SerializeField] bool disableCanvas = true;

        // Update is called once per frame
        void Update()
        {
            if(disableCanvas == true)
            {
                if(Mathf.Approximately(stamina.GetStaminaFraction(),0) || Mathf.Approximately(stamina.GetStaminaFraction(),1 ))
                {
                    rootCanvas.enabled = false;
                    return;
                }
                rootCanvas.enabled = true;
            }
            foreground.localScale = new Vector3(Mathf.Max(stamina.GetStaminaFraction(),0), 1 , 1);
            
        }
    }

}
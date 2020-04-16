using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Attributes
{
    public class HealthBar : MonoBehaviour
    {
        // Start is called before the first frame updateHealth health;
        [SerializeField] Health health = null;
        [SerializeField] RectTransform foreground = null;
        [SerializeField] Canvas rootCanvas = null;

        // Update is called once per frame
        void Update()
        {
            if(Mathf.Approximately(health.GetHealthFraction(),0) || Mathf.Approximately(health.GetHealthFraction(),1 ))
            {
                rootCanvas.enabled = false;
                return;
            }
            rootCanvas.enabled = true;
            foreground.localScale = new Vector3(health.GetHealthFraction(), 1 , 1);
        }
    }

}
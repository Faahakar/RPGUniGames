using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Attributes
{
    
    public class HealthBar : MonoBehaviour
    {

        public enum Direction
        {
            X,
            Y,      
            Z
        }
       [SerializeField]  Direction direction = Direction.X;
        // Start is called before the first frame updateHealth health;
        [SerializeField] Health health = null;
        [SerializeField] RectTransform foreground = null;
        [SerializeField] Canvas rootCanvas = null;

        [SerializeField] bool disableCanvas = true;
        void Start()
        {
            
        }
        
        // Update is called once per frame
        void Update()
        {
            if(disableCanvas == true)
            {
                if(Mathf.Approximately(health.GetHealthFraction(),0) || Mathf.Approximately(health.GetHealthFraction(),1 ))
                {
                    rootCanvas.enabled = false;
                    return;
                }
                rootCanvas.enabled = true;
            }
            if(direction == Direction.X)
            foreground.localScale = new Vector3(health.GetHealthFraction(), 1 , 1);
             if(direction == Direction.Y)
            foreground.localScale = new Vector3(1, health.GetHealthFraction() , 1);
             if(direction == Direction.Z)
            foreground.localScale = new Vector3(1, 1 , health.GetHealthFraction());
        }
    }

}
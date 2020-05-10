using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Attributes
{

    public class HeartFill : MonoBehaviour
    {

        [SerializeField] Health health = null;
        [SerializeField] Image image = null;
        // Start is called before the first frame update

        // Update is called once per frame
        void Update()
        {
          image.fillAmount = health.GetHealthFraction();
        }
    }


}

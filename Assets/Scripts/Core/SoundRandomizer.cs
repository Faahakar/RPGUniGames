using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RPG.Sounds
{
    public class SoundRandomizer : MonoBehaviour
    {
        private AudioSource audioSource;
        public AudioClip[] shoot;
        private AudioClip shootClip;
        void Start()
        {
            audioSource = gameObject.GetComponent<AudioSource>();
        }
        
        public void RandomSound()
        {          
            int index = Random.Range(0, shoot.Length);
            shootClip = shoot[index];
            audioSource.clip = shootClip;
            audioSource.Play();          
        }
    }

}


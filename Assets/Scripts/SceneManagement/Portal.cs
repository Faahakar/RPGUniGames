using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using RPG.Control;

namespace RPG.SceneManagement
{ 
      public class Portal : MonoBehaviour {
          enum DestinationIdentifier
          {
            A,B,C,D,E
          }
          [SerializeField] int sceneToLoad = -1;
          [SerializeField] float fadeoutTime = 3f;
          [SerializeField] float fadeInTime = 3f;
          [SerializeField] float fadeWaitTime = 0.5f;
          [SerializeField] Transform spawnPoint;
          [SerializeField] DestinationIdentifier destination;
          private void OnTriggerEnter(Collider other)
          {
              if( other.gameObject.tag == "Player")
            {    
                StartCoroutine(Transition());
                
            }
          }
              
          private IEnumerator Transition()
          {       
               
              if(sceneToLoad < 0)
              {
                  Debug.LogError("Scene to load is not set");
                  yield break;
              }
            
            DontDestroyOnLoad(gameObject);
            Fader fader = FindObjectOfType<Fader>();
            //Save Current Level
            SavingWrapper wrapper = FindObjectOfType<SavingWrapper>();
            PlayerController playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            playerController.enabled = false;
            // Remove Control

            yield return fader.FadeOut(fadeoutTime);         
            wrapper.Save();
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);   
            PlayerController newplayerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            newplayerController.enabled = false;
            // Remove control
                            
            yield return asyncLoad;
            // Load Level
            wrapper.Load();
              
            Portal otherPortal = GetOtherPortal();
            if (otherPortal != null)
            {
                UpdatePlayer(otherPortal);
            }
            wrapper.Save();

            yield return new WaitForSeconds(fadeWaitTime);
            yield return fader.FadeIn(fadeInTime);
            // restore control
            playerController.enabled = true;
            Destroy(gameObject);
             
             
          }

          private Portal GetOtherPortal()
          {
             foreach(Portal portal in FindObjectsOfType<Portal>())
             {
                 if(portal == this) continue;
                 if(portal.destination != destination) continue;
                 return portal;
             }
           return null;
          }
          private void UpdatePlayer(Portal otherPortal)
          {
              GameObject player = GameObject.FindWithTag("Player");
              GetComponent<NavMeshAgent>().enabled = false;
              player.GetComponent<NavMeshAgent>().Warp(otherPortal.spawnPoint.position);
              player.transform.rotation = otherPortal.spawnPoint.rotation;
              GetComponent<NavMeshAgent>().enabled = true;
          }
        }
    }

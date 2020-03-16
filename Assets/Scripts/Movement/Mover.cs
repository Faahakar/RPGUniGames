using UnityEngine;
using UnityEngine.AI;
using RPG.Resources;
using RPG.Combat;
using RPG.Saving;
using RPG.Core;
using System.Collections;
using System.Collections.Generic;
namespace RPG.Movement
{
    public class Mover : MonoBehaviour,IAction,ISaveable
    {
        [SerializeField] Transform target;
        [SerializeField] float maxSpeed = 6f;
        NavMeshAgent navMeshAgent;
        Health health;
        private void Start()
        {
        health = GetComponent<Health>();
          navMeshAgent = GetComponent<NavMeshAgent>(); 
        }
        void Update()
        {
            navMeshAgent.enabled = !health.IsDead();
            UpdateAnimator();
        }
        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            GetComponent<Fighter>().Cancel();
            MoveTo(destination, speedFraction);
        }


        public void MoveTo(Vector3 destination, float speedFraction)
        {
            navMeshAgent.destination = destination;
            navMeshAgent.speed = maxSpeed*Mathf.Clamp01(speedFraction);
            navMeshAgent.isStopped = false;
        }

        private void UpdateAnimator()
        {
            Vector3 velocity = navMeshAgent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float speed = localVelocity.z;
            GetComponent<Animator>().SetFloat("ForwardSpeed", speed);
        }
        public void Cancel()
        {           
            navMeshAgent.isStopped = true;              
        }
        public object CaptureState()
        {
            Dictionary<string,object> data = new Dictionary<string,object>();
            data["position"] =  new SerializableVector3(transform.position);
            data["rotation"] = new SerializableVector3(transform.eulerAngles);
            return  data;
        }
        public void RestoreState(object state)
        {
           Dictionary<string,object> data = (Dictionary<string,object>)state;
           GetComponent<NavMeshAgent>().enabled = false;
           transform.position =  ((SerializableVector3)data["position"]).ToVector();
           transform.eulerAngles = ((SerializableVector3)data["rotation"]).ToVector();
           GetComponent<NavMeshAgent>().enabled = true;
        }
       

    }

}


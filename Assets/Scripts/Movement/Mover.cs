﻿using UnityEngine;
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

        float speed = 0;
        Vector3 velocity, localVelocity;
        Health health;
        private void Awake()
        {
          health = GetComponent<Health>();
          navMeshAgent = GetComponent<NavMeshAgent>(); 
        }
        private void Start()
        {
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
            velocity = navMeshAgent.velocity;
            localVelocity = transform.InverseTransformDirection(velocity);
            speed = localVelocity.z;
            GetComponent<Animator>().SetFloat("ForwardSpeed", speed);
        }
        public void StopCharacter()
        {
           velocity = new Vector3(0,0,0);
           localVelocity = new Vector3(0,0,0);
           speed = 0f;
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
           navMeshAgent.enabled = false;
           transform.position =  ((SerializableVector3)data["position"]).ToVector();
           transform.eulerAngles = ((SerializableVector3)data["rotation"]).ToVector();
           navMeshAgent.enabled = true;
        }
       

    }

}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


namespace GuidewireSim
{
    /**
     * This class is responsible for tracking collisions of the object it is attached to. Attach this component only to sphere objects
     * of the guidewire.
     */
    public class CollisionDetectionPrimitiveCylinder : MonoBehaviour
    {
        SimulationLoop simulationLoop; //!< The SimulationLoop component in the Simulation GameObject
        CollisionHandler collisionHandler; //!< The CollisionHandler component in the Simulation GameObject

        public int cylinderID;

        private void Awake()
        {
            simulationLoop = FindObjectOfType<SimulationLoop>();
            Assert.IsNotNull(simulationLoop);

            collisionHandler = FindObjectOfType<CollisionHandler>();
            Assert.IsNotNull(collisionHandler);
        }

        private void Start()
        {
            AssignCylinderID();
        }

        /**
         * Assigns the unique ID of the object sphere it is attached to to #sphereID.
         */
        public void AssignCylinderID()
        {
            GameObject thisCylinder = this.transform.gameObject;
            Debug.Log(simulationLoop.CylinderCount);

            for (int cylinderIndex = 0; cylinderIndex < simulationLoop.CylinderCount; cylinderIndex++)
            {   
                Debug.Log(simulationLoop.cylinders[cylinderIndex]);
                Debug.Log(thisCylinder);
                if (thisCylinder == simulationLoop.cylinders[cylinderIndex])
                {
                    cylinderID = cylinderIndex;
                    return;
                }
            }

            Debug.LogWarning("No cylinderID could be assigned.");
        }

        /**
         * Registers a collision that Unity's collision detection detected.
         */
        private void OnCollisionEnter(Collision other)
        {
            ContactPoint collisionContact = other.GetContact(0);

            Vector3 contactPoint = collisionContact.point;
            Vector3 collisionNormal = collisionContact.normal;

            Debug.Log("Collision Enter");
            //collisionHandler.RegisterCollision(this.transform, cylinderID, contactPoint, collisionNormal);
        }

        /**
         * Registers a collision that Unity's collision detection detected.
         */
        private void OnCollisionStay(Collision other)
        {
            ContactPoint collisionContact = other.GetContact(0);

            Vector3 contactPoint = collisionContact.point;
            Vector3 collisionNormal = collisionContact.normal;

            Debug.Log("Collision Stay");
            //collisionHandler.RegisterCollision(this.transform, cylinderID, contactPoint, collisionNormal);
        }
    }
}
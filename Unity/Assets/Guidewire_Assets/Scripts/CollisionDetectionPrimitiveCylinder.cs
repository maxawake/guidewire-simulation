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
        CylinderCollisionHandler cylinderCollisionHandler; //!< The CollisionHandler component in the Simulation GameObject

        CollisionHandler sphereCollisionHandler;

        Vector3[] spherePositions;

        public int cylinderID;

        float factor = 0.01f;

        private void Awake()
        {
            
        }

        private void Start()
        {
            simulationLoop = FindAnyObjectByType<SimulationLoop>();
            Assert.IsNotNull(simulationLoop);

            cylinderCollisionHandler = FindAnyObjectByType<CylinderCollisionHandler>();
            Assert.IsNotNull(cylinderCollisionHandler);

            sphereCollisionHandler = FindAnyObjectByType<CollisionHandler>();
            Assert.IsNotNull(sphereCollisionHandler);

            AssignCylinderID();
            spherePositions = simulationLoop.spherePositions;
        }

        /**
         * Assigns the unique ID of the object sphere it is attached to to #sphereID.
         */
        public void AssignCylinderID()
        {
            GameObject thisCylinder = this.transform.parent.gameObject;

            for (int cylinderIndex = 0; cylinderIndex < simulationLoop.CylinderCount; cylinderIndex++)
            {   
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
            Debug.Log(collisionNormal);
            Debug.Log(contactPoint);
            
            //cylinderCollisionHandler.RegisterCollision(this.transform, cylinderID, contactPoint, collisionNormal);
            
            Vector3 spherePosition1 = spherePositions[cylinderID];
            Vector3 spherePosition2 = spherePositions[cylinderID+1];
            Vector3 rodLine = spherePosition2 - spherePosition1;
            Vector3 toContactPoint = contactPoint - spherePosition1;
            float distance = Vector3.Dot(toContactPoint, rodLine) / rodLine.sqrMagnitude;
            if (distance < 0.5) {
                sphereCollisionHandler.RegisterCollision(this.transform, cylinderID, spherePosition1, factor*(1-distance)*collisionNormal);
                sphereCollisionHandler.RegisterCollision(this.transform, cylinderID+1, spherePosition2, factor*distance*collisionNormal);
            } else {
                sphereCollisionHandler.RegisterCollision(this.transform, cylinderID, spherePosition1,factor*distance*collisionNormal);
                sphereCollisionHandler.RegisterCollision(this.transform, cylinderID+1, spherePosition2, factor*(1-distance)*collisionNormal);
            }
            Debug.Log("distance: " + distance);
            
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
            //cylinderCollisionHandler.RegisterCollision(this.transform, cylinderID, contactPoint, collisionNormal);

            Vector3 spherePosition1 = spherePositions[cylinderID];
            Vector3 spherePosition2 = spherePositions[cylinderID+1];
            Vector3 rodLine = spherePosition2 - spherePosition1;
            Vector3 toContactPoint = contactPoint - spherePosition1;
            float distance = Vector3.Dot(toContactPoint, rodLine) / rodLine.sqrMagnitude;
            if (distance < 0.5) {
                sphereCollisionHandler.RegisterCollision(this.transform, cylinderID, spherePosition1, factor*(1-distance)*collisionNormal);
                sphereCollisionHandler.RegisterCollision(this.transform, cylinderID+1, spherePosition2, factor*distance*collisionNormal);
            } else {
                sphereCollisionHandler.RegisterCollision(this.transform, cylinderID, spherePosition1, factor*distance*collisionNormal);
                sphereCollisionHandler.RegisterCollision(this.transform, cylinderID+1, spherePosition2, factor*(1-distance)*collisionNormal);
            }
            Debug.Log("distance: " + distance);
            
        }
    }
}
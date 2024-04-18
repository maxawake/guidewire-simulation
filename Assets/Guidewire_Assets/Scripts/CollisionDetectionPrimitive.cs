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
    public class CollisionDetectionPrimitive : MonoBehaviour
    {
        SimulationLoop simulationLoop; //!< The SimulationLoop component in the Simulation GameObject
        CollisionHandler collisionHandler; //!< The CollisionHandler component in the Simulation GameObject

        public int sphereID; /**< The unique ID of the sphere that this component is attached to. Matches the position in @p spheres in #SimulationLoop.
                              *   @note Should also match the position in @p spherePositions, @p sphereVelocities, @p sphereExternalForces,
                              *   @p spherePositionPredictions in #SimulationLoop.
                              */

        private void Awake()
        {
            simulationLoop = FindObjectOfType<SimulationLoop>();
            Assert.IsNotNull(simulationLoop);

            collisionHandler = FindObjectOfType<CollisionHandler>();
            Assert.IsNotNull(collisionHandler);
        }

        private void Start()
        {
            AssignSphereID();
        }

        /**
         * Assigns the unique ID of the object sphere it is attached to to #sphereID.
         */
        private void AssignSphereID()
        {
            GameObject thisSphere = this.transform.gameObject;

            for (int sphereIndex = 0; sphereIndex < simulationLoop.SpheresCount; sphereIndex++)
            {
                if (thisSphere == simulationLoop.spheres[sphereIndex])
                {
                    sphereID = sphereIndex;
                    return;
                }
            }

            Debug.LogWarning("No sphereID could be assigned.");
        }

        /**
         * Registers a collision that Unity's collision detection detected.
         */
        private void OnCollisionEnter(Collision other)
        {
            ContactPoint collisionContact = other.GetContact(0);

            Vector3 contactPoint = collisionContact.point;
            Vector3 collisionNormal = collisionContact.normal;

            collisionHandler.RegisterCollision(this.transform, sphereID, contactPoint, collisionNormal);
        }

        /**
         * Registers a collision that Unity's collision detection detected.
         */
        private void OnCollisionStay(Collision other)
        {
            ContactPoint collisionContact = other.GetContact(0);

            Vector3 contactPoint = collisionContact.point;
            Vector3 collisionNormal = collisionContact.normal;

            collisionHandler.RegisterCollision(this.transform, sphereID, contactPoint, collisionNormal);
        }
    }
}
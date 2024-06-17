using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GuidewireSim
{
    /**
     * This class manages all collisions that should be resolved, i.e. the collisions of the last frame.
     */
    public class CylinderCollisionHandler : MonoBehaviour
    {   
        ParameterHandler parameterHandler;
        public List<CylinderCollisionPair> registeredCollisions; //!< All collisions that occured between the last and the current frame in OnTriggerEnter.

        public MeshCollider[] cylinderColliders; /**< Each element stores a reference to the SpherCollider of the respective element in @p cylinders
                                                      *   in SimulationLoop.
                                                      *   @exampletext The second element in this list is the cylinderCollider corresponding to the
                                                      *   cylinder GameObject that is referenced in the second element of @p cylinders in SimulationLoop.
                                                      */

        float cylinderRadius; //!< The radius of the cylinder elements of the guidewire.

        private void Awake()
        {
            parameterHandler = GetComponent<ParameterHandler>();
            Assert.IsNotNull(parameterHandler);
        }

        private void Start()
        {
            registeredCollisions = new List<CylinderCollisionPair>();    
            //cylinderRadius = parameterHandler.cylinderRadius;
        }

        /**
         * Registers a collision by adding it to #registeredCollisions.
         * @param cylinder The cylinder of the guidewire that collided.
         * @param cylinderID The unique ID of @p cylinder.
         * @param contactPoint The contact point of the collision.
         * @param collisionNormal The normal of the collision.
         */
        public void RegisterCollision(Transform cylinder, int cylinderID, Vector3 contactPoint, Vector3 collisionNormal)
        {
            CylinderCollisionPair registeredCollision = new CylinderCollisionPair(cylinder, cylinderID, contactPoint, collisionNormal);
            registeredCollisions.Add(registeredCollision);
        }

        /**
         * Clears the list of all registered collisions.
         */
        public void ResetRegisteredCollisions()
        {
            registeredCollisions.Clear();
        }

        /**
         * Sets the position of the collider of each cylinder to the cylinder's position prediction.
         * @note The position of the collider is implicitly set by setting the colliders center argument.
         * @param cylindersCount The count of all cylinders of the guidewire. Equals the length of @p cylinderPositionPredictions.
         * @param cylinderPositionPredictions The prediction of the position at the current frame of each cylinder (in this case of the last frame).
         * @param cylinderPositions The position at the current frame of each cylinder.
         */
        // public void SetCollidersToPredictions(int cylindersCount, Vector3[] cylinderPositionPredictions, Vector3[] cylinderPositions)
        // {
        //     for (int cylinderIndex = 0; cylinderIndex < cylindersCount; cylinderIndex++)
        //     {
        //         Vector3 centerPosition = (cylinderPositionPredictions[cylinderIndex] - cylinderPositions[cylinderIndex]) / (2 * cylinderRadius);
        //         //cylinderColliders[cylinderIndex].attachedRigidbody.transform = centerPosition;
        //     }
        // }
    }
}
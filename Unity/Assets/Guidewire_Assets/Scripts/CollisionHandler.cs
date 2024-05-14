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
    public class CollisionHandler : MonoBehaviour
    {   
        ParameterHandler parameterHandler;
        public List<CollisionPair> registeredCollisions; //!< All collisions that occured between the last and the current frame in OnTriggerEnter.

        public SphereCollider[] sphereColliders; /**< Each element stores a reference to the SpherCollider of the respective element in @p spheres
                                                      *   in SimulationLoop.
                                                      *   @exampletext The second element in this list is the SphereCollider corresponding to the
                                                      *   sphere GameObject that is referenced in the second element of @p spheres in SimulationLoop.
                                                      */

        float sphereRadius; //!< The radius of the sphere elements of the guidewire.

        private void Awake()
        {
            parameterHandler = GetComponent<ParameterHandler>();
            Assert.IsNotNull(parameterHandler);
        }

        private void Start()
        {
            registeredCollisions = new List<CollisionPair>();    
            sphereRadius = parameterHandler.sphereRadius;
        }

        /**
         * Registers a collision by adding it to #registeredCollisions.
         * @param sphere The sphere of the guidewire that collided.
         * @param sphereID The unique ID of @p sphere.
         * @param contactPoint The contact point of the collision.
         * @param collisionNormal The normal of the collision.
         */
        public void RegisterCollision(Transform sphere, int sphereID, Vector3 contactPoint, Vector3 collisionNormal)
        {
            CollisionPair registeredCollision = new CollisionPair(sphere, sphereID, contactPoint, collisionNormal);
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
         * Sets the position of the collider of each sphere to the sphere's position prediction.
         * @note The position of the collider is implicitly set by setting the colliders center argument.
         * @param spheresCount The count of all spheres of the guidewire. Equals the length of @p spherePositionPredictions.
         * @param spherePositionPredictions The prediction of the position at the current frame of each sphere (in this case of the last frame).
         * @param spherePositions The position at the current frame of each sphere.
         */
        public void SetCollidersToPredictions(int spheresCount, Vector3[] spherePositionPredictions, Vector3[] spherePositions)
        {
            for (int sphereIndex = 0; sphereIndex < spheresCount; sphereIndex++)
            {
                Vector3 centerPosition = (spherePositionPredictions[sphereIndex] - spherePositions[sphereIndex]) / (2 * sphereRadius);
                sphereColliders[sphereIndex].center = centerPosition;
            }
        }
    }
}
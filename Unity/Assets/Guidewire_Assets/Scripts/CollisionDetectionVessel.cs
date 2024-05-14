using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GuidewireSim
{
    /**
     * Similarly to CollisionDetectionPrimitive, this class is responsible for tracking collisions of the object it is attached to.
     * Attach this component only to blood vessel objects.
     */
    public class CollisionDetectionVessel : MonoBehaviour
    {
        SimulationLoop simulationLoop; //!< The SimulationLoop component in the Simulation GameObject
        CollisionHandler collisionHandler; //!< The CollisionHandler component in the Simulation GameObject

        private void Awake()
        {
            simulationLoop = FindAnyObjectByType<SimulationLoop>();
            Assert.IsNotNull(simulationLoop);

            collisionHandler = FindAnyObjectByType<CollisionHandler>();
            Assert.IsNotNull(collisionHandler);
        }

        // private void OnCollisionEnter(Collision other)
        // {
        //     // ContactPoint contactPoint = other.GetContact(0);
        //     // DebugExtension.DrawPoint(contactPoint.point, Color.black);
        //     int sphereID = 1;

        //     collisionHandler.RegisterCollision(other.transform, other, sphereID);
        // }

        // private void OnCollisionStay(Collision other)
        // {
        //     // ContactPoint contactPoint = other.GetContact(0);
        //     // DebugExtension.DrawPoint(contactPoint.point, Color.black);

        //     int sphereID = 1;

        //     collisionHandler.RegisterCollision(other.transform, other, sphereID);
        // }

        private int YieldSphereID(Transform sphereTransform)
        {
            GameObject sphereGO = sphereTransform.gameObject;

            for (int sphereIndex = 0; sphereIndex < simulationLoop.SpheresCount; sphereIndex++)
            {
                if (sphereGO == simulationLoop.spheres[sphereIndex])
                {
                    return sphereIndex;
                }
            }

            Debug.LogWarning("No sphereID could be assigned.");
            return 0;
        }
    }
}
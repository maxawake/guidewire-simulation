using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GuidewireSim
{
    /**
     * Carries all information of a collision that occured.
     */
    public struct CollisionPair
    {
        public Transform sphere; //!< The sphere object of the guidewire that was part of the collision.
        public Vector3 contactPoint; //!< The contact point of the collision.
        public Vector3 collisionNormal; //!< The normal of the collision.
        public int sphereID; //!< The ID of the sphere object of the guidewire that was part of the collision.

        public CollisionPair(Transform sphere, int sphereID, Vector3 contactPoint, Vector3 collisionNormal)
        {
            this.sphere = sphere;
            this.sphereID = sphereID;
            this.contactPoint = contactPoint;
            this.collisionNormal = collisionNormal;
        }
    }
}
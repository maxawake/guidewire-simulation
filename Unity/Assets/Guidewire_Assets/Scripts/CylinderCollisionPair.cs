using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GuidewireSim
{
    /**
     * Carries all information of a collision that occured.
     */
    public struct CylinderCollisionPair
    {
        public Transform cylinder; //!< The cylinder object of the guidewire that was part of the collision.
        public Vector3 contactPoint; //!< The contact point of the collision.
        public Vector3 collisionNormal; //!< The normal of the collision.
        public int cylinderID; //!< The ID of the cylinder object of the guidewire that was part of the collision.

        public CylinderCollisionPair(Transform cylinder, int cylinderID, Vector3 contactPoint, Vector3 collisionNormal)
        {
            this.cylinder = cylinder;
            this.cylinderID = cylinderID;
            this.contactPoint = contactPoint;
            this.collisionNormal = collisionNormal;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GuidewireSim
{
    /**
     * This class implements the collision solving that is part of the constraint solving step.
     */
    public class CollisionSolvingStep : MonoBehaviour
    {
        MathHelper mathHelper; //!< The component MathHelper that provides math related helper functions.
        CollisionHandler collisionHandler; //!< The component CollisionHandler that tracks all collisions.

        ParameterHandler parameterHandler;

        Vector3 deltaPosition = new Vector3(); //!< The correction of @p spherePositionPrediction in method SolveCollisionConstraint().
        Vector3 initialPositionPrediction = new Vector3();

        float sphereRadius; //!< The radius of a sphere of the guidewire.
        float collisionMargin; /**< A margin by which a colliding element of the guidewire is set away from the object
                                                        *   colliding with in the direction of the normal.
                                                        *   @note Without this margin, the colliding element of the guidewire (e.g. a sphere) is
                                                        *   corrected such that its surface exactly touches the object colliding with, which
                                                        *   results in the guidewire still penetrating the object.
                                                        */
        float collisionStiffness; //!< The collision constraint stiffness parameter.

        private void Awake()
        {
            mathHelper = GetComponent<MathHelper>();
            Assert.IsNotNull(mathHelper);

            collisionHandler = GetComponent<CollisionHandler>();
            Assert.IsNotNull(collisionHandler);

            parameterHandler = GetComponent<ParameterHandler>();
            Assert.IsNotNull(parameterHandler);
        }

        private void Start()
        {
            sphereRadius = parameterHandler.sphereRadius;
            collisionMargin = parameterHandler.collisionMargin;
            collisionStiffness = parameterHandler.collisionStiffness;
        }

        /**
         * Is responsible for executing one iteration of the constraint solving step for the collision constraint
         * of each collision of this frame.
         * @param spherePositionPredictions The prediction of the position at the current frame of each sphere (in this case of the last frame).
         * @param solverStep The current iteration of the constraint solving step.
         * @param constraintSolverSteps The total number of solver steps of the constraint solving step.
         */
        public void SolveCollisionConstraints(Vector3[] spherePositionPredictions, int solverStep, int constraintSolverSteps)
        {

            for (int collisionIndex = 0; collisionIndex < collisionHandler.registeredCollisions.Count; collisionIndex++)
            {
                CollisionPair collisionPair = collisionHandler.registeredCollisions[collisionIndex];
                int sphereID = collisionPair.sphereID;
                Vector3 spherePositionPrediction = spherePositionPredictions[sphereID];

                SolveCollisionConstraint(spherePositionPrediction, collisionPair.contactPoint, collisionPair.collisionNormal,
                                         solverStep, out deltaPosition);
                CorrectCollisionPredictions(sphereID, spherePositionPredictions, solverStep, constraintSolverSteps);
            }
        }

        /**
         * Solves the collision constraint for one collision that occured this frame.
         * @param spherePositionPredictions The prediction of the position at the current frame of each sphere (in this case of the last frame).
         * @param contactPoint The contact point of the collision.
         * @param collisionNormal The normal of the collision.
         * @param solverStep The current iteration of the constraint solving step.
         * @attention Current calculation of the normal only works for spheres.
         */
        private void SolveCollisionConstraint(Vector3 spherePositionPrediction, Vector3 contactPoint, Vector3 collisionNormal,
                                             int solverStep, out Vector3 deltaPosition)
        {
            if (solverStep == 0)
            {
                DrawCollisionInformation(spherePositionPrediction, contactPoint, collisionNormal);
            }

            deltaPosition = CalculateDeltaPosition(spherePositionPrediction, contactPoint, collisionNormal);
        }

        /**
         * Draws the contact point, collision normal, and displacement corrections into the scene of the collision that occured.
         * @param spherePositionPrediction The position prediction of the sphere that collided.
         * @param contactPoint The contact point of the collision.
         * @param collisionNormal The normal of the collision.
         */
        private void DrawCollisionInformation(Vector3 spherePositionPrediction, Vector3 contactPoint, Vector3 collisionNormal)
        {
            Debug.DrawLine(contactPoint, contactPoint + 10f * collisionNormal, Color.blue, 2f);
            DebugExtension.DrawPoint(spherePositionPrediction, Color.white);
            DebugExtension.DrawPoint(contactPoint, Color.yellow);
        }

        /**
         * Calculates the displacement of the collision constraint.
         * @param spherePositionPredictions The prediction of the position at the current frame of each sphere (in this case of the last frame).
         * @param closestSurfacePoint The contact point of the collision.
         * @param normalVector The collision normal.
         * @return The delta position, i.e. the calculated displacement.
         */
        private Vector3 CalculateDeltaPosition(Vector3 spherePositionPrediction, Vector3 closestSurfacePoint, Vector3 normalVector)
        {
            return - (spherePositionPrediction - sphereRadius * normalVector - closestSurfacePoint - collisionMargin * normalVector);
        }

        /**
         * Corrects the position prediction of the sphere of @p sphereIndex with the calculated displacement.
         * @param sphereIndex The sphere ID of the colliding sphere.
         * @param spherePositionPredictions The prediction of the position at the current frame of each sphere (in this case of the last frame).
         * @param solverStep The current iteration of the constraint solving step.
         * @param constraintSolverSteps The total number of solver steps of the constraint solving step.
         */
        private void CorrectCollisionPredictions(int sphereIndex, Vector3[] spherePositionPredictions, int solverStep, int constraintSolverSteps)
        {
            Assert.IsTrue(sphereIndex >= 0);

            if (solverStep == 0)
            {
                initialPositionPrediction = spherePositionPredictions[sphereIndex];
            }

            spherePositionPredictions[sphereIndex] += collisionStiffness * deltaPosition;

            if (solverStep == constraintSolverSteps - 1)
            {
                DebugExtension.DrawPoint(spherePositionPredictions[sphereIndex], Color.red);
            }
        }
    }
}
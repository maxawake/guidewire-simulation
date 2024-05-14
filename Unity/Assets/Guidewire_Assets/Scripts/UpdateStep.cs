using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using BSM = BulletSharp.Math;
using System.IO; 

namespace GuidewireSim
{
/**
 * This class implements the update step of the algorithm.
 */
public class UpdateStep : MonoBehaviour
{
    MathHelper mathHelper; //!< The component MathHelper that provides math related helper functions.

    private void Awake()
    {
        mathHelper = GetComponent<MathHelper>();
        Assert.IsNotNull(mathHelper);
    }

    /**
     * Updates the sphere velocities given the current prediction and the current position.
     * @param sphereVelocities The velocity of the current frame of each sphere.
     * @param spheresCount The count of all spheres of the guidewire. Equals the length of @p spherePositionPredictions.
     * @param spherePositionPredictions The prediction of the position at the current frame of each sphere (in this case of the last frame).
     * @param spherePositions The position at the current frame of each sphere.
     * @return The velocity of the current frame of each sphere, i.e. @p sphereVelocities.
     */
    public Vector3[] UpdateSphereVelocities(Vector3[] sphereVelocities, int spheresCount, Vector3[] spherePositionPredictions, 
    Vector3[] spherePositions)
    {   
        for (int sphereIndex = 0; sphereIndex < spheresCount; sphereIndex++)
        {
            sphereVelocities[sphereIndex] = (spherePositionPredictions[sphereIndex] - spherePositions[sphereIndex]) / Time.deltaTime;
        }

        return sphereVelocities;
    }

    /**
     * Updates the sphere positions given the current position predictions.
     * @param spherePositions The position at the current frame of each sphere.
     * @param spheresCount The count of all spheres of the guidewire. Equals the length of @p spherePositionPredictions.
     * @param spherePositionPredictions The prediction of the position at the current frame of each sphere (in this case of the last frame).
     * @return The position at the current frame of each sphere, i.e. @p spherePositions.
     */
    public Vector3[] UpdateSpherePositions(Vector3[] spherePositions, int spheresCount, Vector3[] spherePositionPredictions)
    {
        for (int sphereIndex = 0; sphereIndex < spheresCount; sphereIndex++)
        {
            
            spherePositions[sphereIndex] = spherePositionPredictions[sphereIndex];
        }
        return spherePositions;
    }

    /**
     * Updates the cylinder angular velocities for the update step of the simulation.
     * @param cylinderAngularVelocities The angular velocity of the current frame of each orientation element/ cylinder.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param cylinderOrientations The orientation of each cylinder at its center of mass.
     * @param cylinderOrientationPredictions The prediction of the orientation of each cylinder at its center of mass.
     * @return The angular velocity of the current frame of each orientation element/ cylinder, i.e. @p cylinderAngularVelocities.
     */
    public Vector3[] UpdateCylinderAngularVelocities(Vector3[] cylinderAngularVelocities, int cylinderCount, BSM.Quaternion[] cylinderOrientations,
                                                     BSM.Quaternion[] cylinderOrientationPredictions)
    {
        float factor = 2f / Time.deltaTime;

        for (int cylinderIndex = 0; cylinderIndex < cylinderCount; cylinderIndex++)
        {
            BSM.Quaternion qu = BSM.Quaternion.Conjugate(cylinderOrientations[cylinderIndex]) * cylinderOrientationPredictions[cylinderIndex];
            BSM.Quaternion quaternionResult = factor * qu;
            cylinderAngularVelocities[cylinderIndex] = mathHelper.ImaginaryPart(quaternionResult);
        }

        return cylinderAngularVelocities;
    }

    /**
     * Updates the cylinder orientations given the current orientation predictions for the update step of the simulation.
     * @param cylinderOrientations The orientation of each cylinder at its center of mass.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param cylinderOrientationPredictions The prediction of the orientation of each cylinder at its center of mass.
     * @return The orientation of each cylinder at its center of mass, i.e. @p cylinderOrientations.
     */
    public BSM.Quaternion[] UpdateCylinderOrientations(BSM.Quaternion[] cylinderOrientations, int cylinderCount,
                                                     BSM.Quaternion[] cylinderOrientationPredictions)
    {   
        // TODO: Why is the first cylinder not updated? Was 1 before
        for (int cylinderIndex = 0; cylinderIndex < cylinderCount; cylinderIndex++)
        {
            cylinderOrientations[cylinderIndex] = cylinderOrientationPredictions[cylinderIndex];
        }

        return cylinderOrientations;
    }
}
}
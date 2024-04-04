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
    private Vector3[] lastSphereVelocities;

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

public Vector3[] UpdateSphereVelocities(Vector3[] sphereVelocities, int spheresCount, Vector3[] spherePositionPredictions, Vector3[] spherePositions)
{
    string debugFilePath = "/home/akreibich/TestRobinCode37/DebugVelocities.txt";
    using (StreamWriter writer = new StreamWriter(debugFilePath, true)) 
    {
        for (int sphereIndex = 0; sphereIndex < spheresCount; sphereIndex++)
        {
            if (sphereIndex == 0) // Check if it's the first sphere
            {
                sphereVelocities[sphereIndex] = Vector3.zero; // Set velocity to (0, 0, 0)
            }
            else
            {
                // Update the velocity for other spheres
                Vector3 newVelocity = (spherePositionPredictions[sphereIndex] - spherePositions[sphereIndex]) / Time.deltaTime;
                sphereVelocities[sphereIndex] = newVelocity;
            }

            writer.WriteLine($"After Update: Sphere Index: {sphereIndex}, {1* sphereVelocities[sphereIndex]}");
        }
            CreationScript creationScript = GameObject.Find("GameObject (1)").GetComponent<CreationScript>();
            if (creationScript != null)
            {
        	creationScript.UpdateSphereVelocities(sphereVelocities);
    	    }
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
        // Initialize StreamWriter to write to the specified file.
        // This will append to the file if it already exists.
        using (StreamWriter writer = new StreamWriter("/home/akreibich/TestRobinCode37/UpdateStepDebug.txt", true))
        {
            for (int sphereIndex = 0; sphereIndex < spheresCount; sphereIndex++)
            {
                // Write the position before updating to the file.
                //writer.WriteLine($"Before Update: Sphere Index: {sphereIndex}, {spherePositions[sphereIndex].ToString()}");

                // Calculate the difference between the new and old positions.
                //Vector3 difference = 1000 * (spherePositionPredictions[sphereIndex] - spherePositions[sphereIndex]);

                // Update the position.
                spherePositions[sphereIndex] = spherePositionPredictions[sphereIndex];

                // Write the updated position and difference to the file.
               // writer.WriteLine($"After Update: Sphere Index: {sphereIndex}, {spherePositions[sphereIndex].ToString()}");
               // writer.WriteLine($"Difference: Sphere Index: {sphereIndex}, {difference.ToString()}");
            }
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
        for (int cylinderIndex = 1; cylinderIndex < cylinderCount; cylinderIndex++)
        {
            cylinderOrientations[cylinderIndex] = cylinderOrientationPredictions[cylinderIndex];
        }

        return cylinderOrientations;
    }
}
}

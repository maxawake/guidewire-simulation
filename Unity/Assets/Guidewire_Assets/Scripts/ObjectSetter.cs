using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using BSM = BulletSharp.Math;

namespace GuidewireSim
{
/**
 * This class is responsible for setting the transformation positions of the GameObjects in the scene to their respective simulation
 * data like @p spherePositions.
 */
public class ObjectSetter : MonoBehaviour
{
    MathHelper mathHelper; //!< The component MathHelper that provides math related helper functions.

    private void Awake()
    {
        mathHelper = GetComponent<MathHelper>();
        Assert.IsNotNull(mathHelper);
    }

    /**
     * Sets the positions of the GameObjects @p spheres to their respective @p spherePositions.
     * @param spheres All spheres that are part of the guidewire.
     * @param spheresCount The count of all spheres of the guidewire. Equals the length of @p spherePositionPredictions.
     * @param spherePositions The position at the current frame of each sphere.
     */
    public void SetSpherePositions(GameObject[] spheres, int spheresCount, Vector3[] spherePositions)
    {
        for (int sphereIndex = 0; sphereIndex < spheresCount; sphereIndex++)
        {
            spheres[sphereIndex].transform.position = spherePositions[sphereIndex];
        }
    }

    /**
     * Sets the positions of the GameObjects @p cylinders to their respective @p cylinderPositions.
     * @param cylinders All cylinders that are part of the guidewire.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param cylinderPositions The position/ center of mass of each cylinder.
     */
    public void SetCylinderPositions(GameObject[] cylinders, int cylinderCount, Vector3[] cylinderPositions)
    {
        for (int cylinderIndex = 0; cylinderIndex < cylinderCount; cylinderIndex++)
        {
            cylinders[cylinderIndex].transform.position = cylinderPositions[cylinderIndex];
        }
    }

    /**
     * Rotates each cylinder GameObject such that its centerline is parallel with the line segment that is spanned by the two adjacent
     * sphere's center of masses.
     * @param cylinders All cylinders that are part of the guidewire.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param cylinderOrientations The orientation of each cylinder at its center of mass.
     * @param directors The orthonormal basis of each orientation element / cylinder, also called directors.
     * @note @p appliedTransformation is the rotation that aligns the y-axis of the cylinder with the z-axis of the orientations
     * (the third director). This is needed, because the y-axis of the cylinder is parallel with its centerline, while the z-axis
     * of the orientations (the third director) is also defined as being parallel with the cylinder's centerline. Thus @p appliedTransformation
     * is necessary.
     */
    public void SetCylinderOrientations(GameObject[] cylinders, int cylinderCount, BSM.Quaternion[] cylinderOrientations, Vector3[][] directors)
    {
        for (int cylinderIndex = 0; cylinderIndex < cylinderCount; cylinderIndex++)
        {
            Quaternion cylinderOrientation = Quaternion.LookRotation(directors[2][cylinderIndex]);

            cylinders[cylinderIndex].transform.rotation = cylinderOrientation;
        }
    }
}
}
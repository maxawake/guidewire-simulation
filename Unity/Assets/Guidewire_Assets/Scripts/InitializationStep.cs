using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using BSM = BulletSharp.Math;

namespace GuidewireSim
{
/**
 * This class is responsible for initializing all data with their initial values of the simulation at the start of the simulation.
 */
[RequireComponent(typeof(MathHelper))]
public class InitializationStep : MonoBehaviour
{   
    // TODO: Check if can be outsourced
    private SimulationLoop simulationLoop; // Declare simulationLoop
    private float rodElementLength; // Declare rodElementLength

    CollisionHandler collisionHandler; //!< The component CollisionHandler that solves all collisions.
    MathHelper mathHelper; //!< The component MathHelper that provides math related helper functions.

    // TODO: Why private and why changed value?
    [Range(1000f, 10000f)] private float materialDensity = 7860; /**< The density of the rod material. The value 7960 is taken from 
                                                          *   Table 2 of the CoRdE paper.
                                                          */

    [Range(0.0001f, 1f)] private float materialRadius = 0.001f; /**< The radius of the cross-section of the rod. Tha value 0.001 or 1mm
                                                        *  is taken from Table 2 of the CoRdE paper.
                                                        */

    private void Awake()
    {   
        // TODO: Check if can be outsourced
        simulationLoop = GetComponent<SimulationLoop>(); // Initialize simulationLoop
        rodElementLength = simulationLoop.GetRodElementLength(); // Initialize rodElementLength

        collisionHandler = GetComponent<CollisionHandler>();
        Assert.IsNotNull(collisionHandler);

        mathHelper = GetComponent<MathHelper>();
        Assert.IsNotNull(mathHelper);
    }

    /**
     * Initializes @p spherePositions with the positions of @p spheres at the start of the simulation.
     * @param spheres All spheres that are part of the guidewire.
     * @param spheresCount The count of all spheres of the guidewire. Equals the length of @p spherePositionPredictions.
     * @param[out] spherePositions The position at the current frame of each sphere.
     * @req @p spheresCount should be at least one.
     */
    public void InitSpherePositions(GameObject[] spheres, int spheresCount, out Vector3[] spherePositions)
    {
        Assert.IsTrue(spheresCount >= 1);

        spherePositions = new Vector3[spheresCount];

        for (int sphereIndex = 0; sphereIndex < spheresCount; sphereIndex++)
        {
            spherePositions[sphereIndex] = spheres[sphereIndex].transform.position;
        }
    }

    /**
     * Initializes @p sphereVelocities with the default value of zero at the start of the simulation.
     * @param spheresCount The count of all spheres of the guidewire. Equals the length of @p spherePositionPredictions.
     * @param[out] sphereVelocities The velocity of the current frame of each sphere.
     * @note Velocities are set to zero at the start of the simulation.
     */
    public void InitSphereVelocities(int spheresCount, out Vector3[] sphereVelocities)
    {
        sphereVelocities = new Vector3[spheresCount];
    }

    /**
     * Initializes @p sphereInverseMasses with the default value of one at the start of the simulation. TODO: I (Alex Kreibich) adapt this so the total mass stays the same. The idea for how this works is described in my Bachelorthesis in the Methods Chapter
     * @param spheresCount The count of all spheres of the guidewire. Equals the length of @p spherePositionPredictions.
     * @param[out] sphereInverseMasses The constant inverse masses  of each sphere.
     */
    public void InitSphereInverseMasses(int spheresCount, out float[] sphereInverseMasses)
    {
        sphereInverseMasses = new float[spheresCount];
        // TODO: Why is this calculation done?
        //float inverseMassValue = ((100/rodElementLength)+1)/10f; 

        for (int sphereIndex = 0; sphereIndex < spheresCount; sphereIndex++)
        {
            //TODO: Why ? sphereInverseMasses[sphereIndex] = inverseMassValue;
            sphereInverseMasses[sphereIndex] = 1f;
        }
    }

    /**
     * Initializes @p cylinderPositions as middle points of the positions of @p spheres at the start of the simulation.
     * @param spheresCount The count of all spheres of the guidewire. Equals the length of @p spherePositionPredictions.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param[out] cylinderPositions The position/ center of mass of each cylinder.
     */
    public void InitCylinderPositions(int cylinderCount, Vector3[] spherePositions, out Vector3[] cylinderPositions)
    {
        cylinderPositions = new Vector3[cylinderCount];

        mathHelper.CalculateCylinderPositions(cylinderCount, spherePositions, cylinderPositions);
    }

    /**
     * Initializes @p cylinderOrientations with the default value of (0f, 0f, 0f, 1f) which equals the quaternion identity
     * at the start of the simulation.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientations.
     * @param[out] cylinderOrientations The orientation of each cylinder at its center of mass.
     */
    public void InitCylinderOrientations(int cylinderCount, out BSM.Quaternion[] cylinderOrientations)
    {
        cylinderOrientations = new BSM.Quaternion[cylinderCount];

        for (int cylinderIndex = 0; cylinderIndex < cylinderCount; cylinderIndex++)
        {
            cylinderOrientations[cylinderIndex] = new BSM.Quaternion(0f, 0f, 0f, 1f);
        }

    }

    /**
     * Calculates the discrete darboux vector for each orientation pair (two adjacent orientations) at its rest configuration,
     * i.e. at frame 0.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param cylinderOrientations The orientation of each cylinder at its center of mass.
     * @param[out] discreteRestDarbouxVectors The array of all discrete Darboux Vectors at the rest configuration, i.e. at frame 0. Has (n-1) elements,
     * if n is the number of orientations of the guidewire, because the darboux vector is taken of two adjacent orientations.
     * @param rodElementLength The distance between two spheres, also the distance between two orientations.
     * @req @p cylinderCount should be at least one.
     * @req @p rodElementLength should be positive.
     * @note All cylinder orientations must be computed for frame 0 first.
     */
    public void InitDiscreteRestDarbouxVectors(int cylinderCount, BSM.Quaternion[] cylinderOrientations, out Vector3[] discreteRestDarbouxVectors,
                                               float rodElementLength)
    {
        Assert.IsTrue(cylinderCount >= 1);
        Assert.IsTrue(rodElementLength > 0f);

        discreteRestDarbouxVectors = new Vector3[cylinderCount - 1];

        for (int darbouxIndex = 0; darbouxIndex < cylinderCount - 1; darbouxIndex++)
        {
            discreteRestDarbouxVectors[darbouxIndex] = mathHelper.DiscreteDarbouxVector(cylinderOrientations[darbouxIndex],
                                                                                        cylinderOrientations[darbouxIndex + 1],
                                                                                        rodElementLength);
        }
    }

    /**
     * Initializes @p cylinderAngularVelocities with the default value of zero at the start of the simulation.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param[out] cylinderAngularVelocities The angular velocity of the current frame of each orientation element/ cylinder.
     */
    public void InitCylinderAngularVelocities(int cylinderCount, out Vector3[] cylinderAngularVelocities)
    {
        cylinderAngularVelocities = new Vector3[cylinderCount];
    }

    /**
     * Initializes @p cylinderScalarWeights with the default value of one at the start of the simulation.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param[out] cylinderScalarWeights The constant scalar weights of each orientation/ quaternion similar to @p sphereInverseMasses.
     */
    public void InitCylinderScalarWeights(int cylinderCount, out float[] cylinderScalarWeights)
    {
        cylinderScalarWeights = new float[cylinderCount];

        for (int cylinderIndex = 0; cylinderIndex < cylinderCount; cylinderIndex++)
        {   
            // TODO: Why not here?
            cylinderScalarWeights[cylinderIndex] = 1f; //(50/(500/rodElementLength)-1);
        }
    }

    /**
     * Initializes @p sphereExternalForces with the default value of zero at the start of the simulation.
     * @param spheresCount The count of all spheres of the guidewire. Equals the length of @p spherePositionPredictions.
     * @param[out] sphereExternalForces The sum of all current external forces that are applied per particle/ sphere.
     */
    public void InitSphereExternalForces(int spheresCount, out Vector3[] sphereExternalForces)
    {
        sphereExternalForces = new Vector3[spheresCount];
    }

    /**
     * Initializes @p spherePositionPredictions with the default value of zero at the start of the simulation.
     * @param spheresCount The count of all spheres of the guidewire. Equals the length of @p spherePositionPredictions.
     * @param[out] spherePositionPredictions The prediction of the position at the current frame of each sphere.
     */
    public void InitSpherePositionPredictions(GameObject[] spheres, int spheresCount, out Vector3[] spherePositionPredictions)
    {
        spherePositionPredictions = new Vector3[spheresCount];

        for (int sphereIndex = 0; sphereIndex < spheresCount; sphereIndex++)
        {
            spherePositionPredictions[sphereIndex] = spheres[sphereIndex].transform.position;
        }
    }

    /**
     * Initializes @p cylinderOrientationPredictions with the default value of (0f, 0f, 0f, 1f) which equals the quaternion identity
     * at the start of the simulation.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param[out] cylinderOrientationPredictions The prediction of the orientation of each cylinder at its center of mass.
     */
    public void InitCylinderOrientationPredictions(int cylinderCount, out BSM.Quaternion[] cylinderOrientationPredictions)
    {
        cylinderOrientationPredictions = new BSM.Quaternion[cylinderCount];

        for (int cylinderIndex = 0; cylinderIndex < cylinderCount; cylinderIndex++)
        {
            cylinderOrientationPredictions[cylinderIndex] = new BSM.Quaternion(0f, 0f, 0f, 1f);
        }
    }

    /**
     * Initializes @p inertiaTensor so that all elements except the diagonal ones are zero. The first and second diagonal entry equal
     * \f$ \rho * \pi * \frac{r^{2}}{4} \f$, and the third diagonal entry equals \f$ \rho * \pi * \frac{r^{2}}{2} \f$.
     * @param[out] inertiaTensor The inertia tensor. Entries are approximates as in the CoRdE paper.
     */
    public void InitInertiaTensor(out float[,] inertiaTensor)
    {
        inertiaTensor = new float[3,3];
        inertiaTensor[0,0] = inertiaTensor[1,1] = materialDensity * Mathf.PI * materialRadius * materialRadius / 4f;
        inertiaTensor[2,2] = materialDensity * Mathf.PI * materialRadius * materialRadius / 2f;
    }

    /**
     * Initializes @p inverseInertiaTensor as the inverse of @p inertiaTensor.
     * @param inertiaTensor The inertia tensor. Entries are approximates as in the CoRdE paper.
     * @param[out] inverseInertiaTensor The inverse of @p inertiaTensor.
     */    
    public void InitInverseInertiaTensor(float[,] inertiaTensor, out float[,] inverseInertiaTensor)
    {
        Assert.IsTrue(inertiaTensor[0,0] != 0);
        Assert.IsTrue(inertiaTensor[1,1] != 0);
        Assert.IsTrue(inertiaTensor[2,2] != 0);

        inverseInertiaTensor = new float[3,3];
        inverseInertiaTensor[0,0] = 1f / inertiaTensor[0,0];
        inverseInertiaTensor[1,1] = 1f / inertiaTensor[1,1];
        inverseInertiaTensor[2,2] = 1f / inertiaTensor[2,2];
    }

    /**
        * Initializes @p cylinderExternalTorques with the default value of zero at the start of the simulation.
        * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
        * @param[out] cylinderExternalTorques The sum of all current external torques that are applied per orientation element/ cylinder.
        */
    public void InitCylinderExternalTorques(int cylinderCount, out Vector3[] cylinderExternalTorques)
    {
        cylinderExternalTorques = new Vector3[cylinderCount];
    }

    /**
     * Initializes the world space basis vectors (1, 0, 0), (0, 1, 0), (0, 0, 1) as embedded quaternions with scalar part zero.
     * @param[out] worldSpaceBasis The three basis vectors of the world coordinate system.
     */
    public void InitWorldSpaceBasis(out BSM.Quaternion[] worldSpaceBasis)
    {
        worldSpaceBasis = new BSM.Quaternion[3];

        worldSpaceBasis[0] = new BSM.Quaternion(1f, 0f, 0f, 0f);
        worldSpaceBasis[1] = new BSM.Quaternion(0f, 1f, 0f, 0f);
        worldSpaceBasis[2] = new BSM.Quaternion(0f, 0f, 1f, 0f);
    }

    /**
     * Initializes the @p directors array of arrays. The zero-th array defines all first directors of each director basis and so on.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param worldSpaceBasis The three basis vectors of the world coordinate system.
     * @param[out] directors The orthonormal basis of each orientation element / cylinder, also called directors.
     * @note Example: The (i, j)th element holds the (i-1)th director of orientation element j.
     */
    public void InitDirectors(int cylinderCount, BSM.Quaternion[] worldSpaceBasis, out Vector3[][] directors)
    {
        directors = new Vector3[3][];
        directors[0] = new Vector3[cylinderCount];
        directors[1] = new Vector3[cylinderCount];
        directors[2] = new Vector3[cylinderCount];

        for (int cylinderIndex = 0; cylinderIndex < cylinderCount; cylinderIndex++)
        {
            directors[0][cylinderIndex] = mathHelper.ImaginaryPart(worldSpaceBasis[0]);
            directors[1][cylinderIndex] = mathHelper.ImaginaryPart(worldSpaceBasis[1]);
            directors[2][cylinderIndex] = mathHelper.ImaginaryPart(worldSpaceBasis[2]);
        }
    }

    public void InitSphereColliders(int spheresCount, GameObject[] spheres)
    {
        collisionHandler.sphereColliders = new SphereCollider[spheresCount];

        for (int sphereIndex = 0; sphereIndex < spheresCount; sphereIndex++)
        {
            SphereCollider sphereCollider = spheres[sphereIndex].GetComponent<SphereCollider>();
            Assert.IsNotNull(sphereCollider);

            collisionHandler.sphereColliders[sphereIndex] = sphereCollider;
        }
    }
}
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using BSM = BulletSharp.Math;
using System.IO;
//using System.Numerics;

namespace GuidewireSim
{
/**
 * This class implements the prediction step of the algorithm.
 */
public class PredictionStep : MonoBehaviour
{
    MathHelper mathHelper; //!< The component MathHelper that provides math related helper functions.

    ParameterHandler parameterHandler;
    private float displacement; 


    private void Awake()
    {
        mathHelper = GetComponent<MathHelper>();
        Assert.IsNotNull(mathHelper);

        parameterHandler = GetComponent<ParameterHandler>();
        Assert.IsNotNull(parameterHandler);

        displacement = parameterHandler.displacement;
    }

    /**
     * Calculates the predictions for the sphere velocities for the prediction step of the algorithm.
     * @param sphereVelocities The velocity of the current frame of each sphere.
     * @param sphereInverseMasses The constant inverse masses  of each sphere.
     * @param sphereExternalForces The sum of all current external forces that are applied per particle/ sphere.
     * @return The predictions of the positions of the spheres, i.e. @p spherePositionPredictions.
     * @note The predictions are again stored in @p sphereVelocities.
     */
    public Vector3[] PredictSphereVelocities(Vector3[] sphereVelocities, float[] sphereInverseMasses, Vector3[] sphereExternalForces)
    {
        for (int sphereIndex = 0; sphereIndex < sphereVelocities.Length; sphereIndex++)
        {
            Vector3 acceleration = sphereInverseMasses[sphereIndex] * sphereExternalForces[sphereIndex];
            sphereVelocities[sphereIndex] += Time.fixedDeltaTime * acceleration;
        }

        return sphereVelocities;
    }

    /**
     * Calculates the predictions for the sphere positions for the prediction step of the algorithm using Verlet integration.
    * @param spherePositionPredictions The prediction of the position at the current frame of each sphere (in this case of the last frame).
    * @param spheresCount The count of all spheres of the guidewire. Equals the length of @p spherePositionPredictions.
    * @param spherePositions The position at the current frame of each sphere.
    * @param sphereVelocities The velocity of the current frame of each sphere.
    * @return The prediction of the position at the current frame of each sphere, i.e. spherePositionPredictions.
    */
    public Vector3[] PredictSpherePositions(Vector3[] spherePositionPredictions, int spheresCount, Vector3[] spherePositions, Vector3[] oldSpherePositions, Vector3[] sphereVelocities, float[] sphereInverseMasses, Vector3[] sphereExternalForces)
    {       
        if (parameterHandler.VerletIntegration)
        {   
            // For steady state, the first sphere needs to be fixed
            for (int sphereIndex = 1; sphereIndex < spheresCount; sphereIndex++)
            {   
                Vector3 acceleration = Time.fixedDeltaTime * Time.fixedDeltaTime *sphereInverseMasses[sphereIndex] * sphereExternalForces[sphereIndex];

                spherePositionPredictions[sphereIndex] = (2.0f-parameterHandler.damping)*spherePositions[sphereIndex] - (1.0f-parameterHandler.damping)*oldSpherePositions[sphereIndex] + acceleration;
            }
        }
        else // Euler Integration
        {
            for (int sphereIndex = 0; sphereIndex < spheresCount; sphereIndex++)
            {
                spherePositionPredictions[sphereIndex] = spherePositions[sphereIndex] + Time.fixedDeltaTime * sphereVelocities[sphereIndex];  
            }
            return spherePositionPredictions;
        }
        
        
        
        return spherePositionPredictions;
    }

    /**
     * Calculates the predictions for the angular velocities for the prediction step of the algorithm.
     * @param cylinderAngularVelocities The angular velocity of the current frame of each orientation element/ cylinder.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param inertiaTensor The inertia tensor. Entries are approximates as in the CoRdE paper.
     * @param cylinderExternalTorques The sum of all current external torques that are applied per orientation element/ cylinder.
     * @param inverseInertiaTensor The inverse of @p inertiaTensor.
     * @return The angular velocity of the current frame of each orientation element/ cylinder, i.e. @p cylinderAngularVelocities.
     * @note The predictions are again stored in @p cylinderAngularVelocities.
     */
    public Vector3[] PredictAngularVelocities(Vector3[] cylinderAngularVelocities, int cylinderCount, float[,] inertiaTensor,
                                              Vector3[] cylinderExternalTorques, float[,] inverseInertiaTensor)
    {
        for (int cylinderIndex = 0; cylinderIndex < cylinderCount; cylinderIndex++)
        {
            Vector3 Iw = mathHelper.MatrixVectorMultiplication(inertiaTensor, cylinderAngularVelocities[cylinderIndex]);
            Vector3 wXIw = Vector3.Cross(cylinderAngularVelocities[cylinderIndex], Iw);
            Vector3 TauwXIw = cylinderExternalTorques[cylinderIndex] - wXIw;
            Vector3 ITauwXIw = mathHelper.MatrixVectorMultiplication(inverseInertiaTensor, TauwXIw);
            Vector3 summand = Time.fixedDeltaTime * ITauwXIw;
            cylinderAngularVelocities[cylinderIndex] += summand;
        }

        return cylinderAngularVelocities;
    }

    /**
     * Calculates the predictions for the cylinder orientations for the prediction step of the algorithm.
     * @param cylinderOrientationPredictions The prediction of the orientation of each cylinder at its center of mass.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param cylinderAngularVelocities The angular velocity of the current frame of each orientation element/ cylinder.
     * @param cylinderOrientations The orientation of each cylinder at its center of mass.
     * @return The prediction of the orientation of each cylinder at its center of mass, i.e. @p cylinderOrientationPredictions.
     */
    public BSM.Quaternion[] PredictCylinderOrientations(BSM.Quaternion[] cylinderOrientationPredictions, int cylinderCount,
                                                        Vector3[] cylinderAngularVelocities, BSM.Quaternion[] cylinderOrientations)
    {
        for (int cylinderIndex = 0; cylinderIndex < cylinderCount; cylinderIndex++)
        {
            BSM.Quaternion embeddedAngularVelocity = mathHelper.EmbeddedVector(cylinderAngularVelocities[cylinderIndex]);
            BSM.Quaternion qw = BSM.Quaternion.Multiply(cylinderOrientations[cylinderIndex], embeddedAngularVelocity);
            BSM.Quaternion summand = BSM.Quaternion.Multiply(qw, 0.5f * Time.fixedDeltaTime);
            cylinderOrientationPredictions[cylinderIndex] = cylinderOrientations[cylinderIndex] + summand;
            
            cylinderOrientationPredictions[cylinderIndex].Normalize();
        }

        return cylinderOrientationPredictions;
    }
}
}
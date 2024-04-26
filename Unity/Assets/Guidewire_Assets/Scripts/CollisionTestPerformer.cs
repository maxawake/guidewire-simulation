using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GuidewireSim
{
public class CollisionTestPerformer : MonoBehaviour
{
    SimulationLoop simulationLoop; //!< The SimulationLoop component that executes all steps of the simulation loop.

    [SerializeField] Vector3 pullForce = new Vector3(0f, 0f, 5f); //!< External force that is applied in Force Test Three.

    [SerializeField] bool doCollisionTestOne = false;
    [SerializeField] bool doCollisionTestTwo = false;
    [SerializeField] bool doCollisionTestThree = false;
    [SerializeField] bool doCollisionTestFour = false;
    float startTime = 0f;

    private void Awake()
    {
        simulationLoop = GetComponent<SimulationLoop>();
        Assert.IsNotNull(simulationLoop);
    }

    private void Start()
    {
        startTime = Time.time;
        PerformCollisionTests();
    }

    /**
     * Performs each Torque Test whose respective serialized boolean is set to true in the Unity inspector.
     */
    private void PerformCollisionTests()
    {
        if (doCollisionTestOne) PerformCollisionTestOne();
        else if (doCollisionTestTwo) StartCoroutine(PerformCollisionTestTwo());
        else if (doCollisionTestThree) StartCoroutine(PerformCollisionTestThree());
        else if (doCollisionTestFour) StartCoroutine(PerformCollisionTestFour());
    }

    /**
     * Performs torque test one. This test applies an external force to one end of the guidewire.
     */
    private void PerformCollisionTestOne()
    {
        for (int sphereIndex = 0; sphereIndex < (simulationLoop.SpheresCount - 1); sphereIndex++)
        {
            simulationLoop.sphereExternalForces[sphereIndex] = Vector3.zero;
        }

        simulationLoop.sphereExternalForces[simulationLoop.SpheresCount - 1] = pullForce;
    }

    // force gets applied for a fixed time
    // TODO: Check value
    private IEnumerator PerformCollisionTestTwo(float applyForceTime = 1.5f)
    {
        for (int sphereIndex = 0; sphereIndex < (simulationLoop.SpheresCount - 1); sphereIndex++)
        {
            simulationLoop.sphereExternalForces[sphereIndex] = Vector3.zero;
        }

        // TODO: Check if the force is applied to the correct sphere
        simulationLoop.sphereExternalForces[simulationLoop.SpheresCount - 1] = pullForce;

        yield return new WaitForSeconds(applyForceTime);

        // TODO: Check if the force is applied to the correct sphere
        simulationLoop.sphereExternalForces[simulationLoop.SpheresCount - 1] = Vector3.zero;

        float timeDiff = Time.time - startTime;
        Debug.Log("Elapsed time of collision test: " + timeDiff);
        Debug.Log("Velocity at test end: " + simulationLoop.sphereVelocities[1].ToString("e2"));
        Debug.Log("End of Pull Phase of Collision Test Two");
    }

    // force gets applied until a fixed velocity is reached
    // TODO: Check value
    private IEnumerator PerformCollisionTestThree(float exitVelocity = 4f)
    {
        for (int sphereIndex = 0; sphereIndex < (simulationLoop.SpheresCount - 1); sphereIndex++)
        {
            simulationLoop.sphereExternalForces[sphereIndex] = Vector3.zero;
        }

        simulationLoop.sphereExternalForces[simulationLoop.SpheresCount - 1] = pullForce;


        yield return new WaitUntil(() => simulationLoop.sphereVelocities[simulationLoop.SpheresCount - 1].z >= exitVelocity);

        simulationLoop.sphereExternalForces[simulationLoop.SpheresCount - 1] = Vector3.zero;

        float timeDiff = Time.time - startTime;
        Debug.Log("Elapsed time of collision test: " + timeDiff);
        Debug.Log("Velocity at test end: " + simulationLoop.sphereVelocities[1].ToString("e2"));
        Debug.Log("End of Pull Phase of Collision Test Three");
    }

    // force gets applied for the whole time
    // TODO: Check value
    private IEnumerator PerformCollisionTestFour(float pullForceFactor = 0.3f)
    {
        float appliedPullForce = pullForceFactor * pullForce.z;
        
        Debug.Log("Start of Collision Test Four");
        Debug.Log("Pull Force: " + appliedPullForce);

        for (int sphereIndex = 0; sphereIndex < (simulationLoop.SpheresCount - 1); sphereIndex++)
        {
            simulationLoop.sphereExternalForces[sphereIndex] = Vector3.zero;
        }

        // TODO: Check if the force is applied to the correct sphere
        simulationLoop.sphereExternalForces[simulationLoop.SpheresCount - 1] = pullForceFactor * pullForce;

        yield return null;
    }
}
}
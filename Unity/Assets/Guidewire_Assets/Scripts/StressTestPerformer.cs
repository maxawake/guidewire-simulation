using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GuidewireSim
{
/**
 * This class enables the user to test the impact of mutliple external forces and external torques with one button within the Unity inspector.
 * @attention In the current version, the user is not able to fix positions or orientations of the guidewire,
 * which is necessary e.g. for stress test one.
 */
public class StressTestPerformer : MonoBehaviour
{
    SimulationLoop simulationLoop; //!< The SimulationLoop component that executes all steps of the simulation loop.

    [Tooltip("Performs stress test one. This test fixes the position of one end of the guidewire, and applies @p pullForce at the other end for"
     + "@p applyForceTime seconds, and then applies - @p pullForce for another @p applyForceTime seconds.")]
    [SerializeField] bool doStressTestOne = false; /**< Whether to run Stress Test One. This test fixes the position of one end of the
                                                    *   guidewire, and applies @p pullForce at the other end for @p applyForceTime seconds,
                                                    *   and then applies - @p pullForce for another @p applyForceTime seconds.
                                                    */

    private void Awake()
    {
        simulationLoop = GetComponent<SimulationLoop>();
        Assert.IsNotNull(simulationLoop);
    }

    private void Start()
    {
        PerformStressTests();
    }

    /**
     * Performs each Stress Test whose respective serialized boolean is set to true in the Unity inspector.
     */
    private void PerformStressTests()
    {
        if (doStressTestOne) StartCoroutine(PerformStressTestOne());
    }

    /**
     * Performs stress test one. This test fixes the position of one end of the guidewire, and applies @p pullForce at the other end for
     * @p applyForceTime seconds, and then applies - @p pullForce for another @p applyForceTime seconds.
     * @param applyForceTime For how many seconds to apply the force to the particles.
     * @attention In the current version, the user is not able to fix positions or orientations of the guidewire,
     * which is necessary e.g. for stress test one.
     * @req Output a log message when no further forces are applied to the guidewire.
     */
    private IEnumerator PerformStressTestOne(float applyForceTime = 1f)
    {
        // fix the position of the first sphere. Does not fix the position, but nullifies forces acting on this particle.
        simulationLoop.sphereInverseMasses[0] = 0f;
        simulationLoop.sphereInverseMasses[1] = 0f;
        simulationLoop.cylinderScalarWeights[0] = 0f;

        Vector3 pullForce = new Vector3(0f, 2f, 0f);

        for (int sphereIndex = 0; sphereIndex < (simulationLoop.spheresCount - 1); sphereIndex++)
        {
            simulationLoop.sphereExternalForces[sphereIndex] = Vector3.zero;
        }

        simulationLoop.sphereExternalForces[simulationLoop.spheresCount - 1] = pullForce;

        yield return new WaitForSeconds(applyForceTime);

        simulationLoop.sphereExternalForces[simulationLoop.spheresCount - 1] = -pullForce;

        yield return new WaitForSeconds(applyForceTime);

        simulationLoop.sphereExternalForces[simulationLoop.spheresCount - 1] = Vector3.zero;

        Debug.Log("End of Stress Test One");
    }
}
}
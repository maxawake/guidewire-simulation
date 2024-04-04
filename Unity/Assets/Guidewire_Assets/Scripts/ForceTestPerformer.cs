using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GuidewireSim
{
/**
 * This class enables the user to test the impact of external forces with one button within the Unity inspector.
 */
public class ForceTestPerformer : MonoBehaviour
{
    SimulationLoop simulationLoop; //!< The SimulationLoop component that executes all steps of the simulation loop.

    [Tooltip("Applies gravity to all spheres.")]
    [SerializeField] bool doForceTestOne = false; //!< Whether to run Force Test One. This test applies gravity to all spheres.
    [Tooltip("Applies an external force to one end of the guidewire.")]
    [SerializeField] bool doForceTestTwo = false; //!< Whether to run Force Test Two. This test applies an external force to one end of the guidewire.
    [Tooltip("Applies an external force to one end of the guidewire for a fixed amount of time and then the opposite force"
    + "at the same sphere for the same amount of time.")]
    [SerializeField] bool doForceTestThree = false; /**< Whether to run Force Test Three. This test applies an external force to one end of the guidewire
                                                     *   for a fixed amount of time and then the opposite force at the same sphere for the same amount of time.
                                                     */
    [Tooltip("Applies an external force to one end of the guidewire and the opposite force at the other end of the guidewire.")]
    [SerializeField] bool doForceTestFour = false; /**< Whether to run Force Test Four. This test applies an external force to one end of the guidewire
                                                    *   and the opposite force at the other end of the guidewire.
                                                    */
    [Tooltip("Shifts one end of the guidewire and runs the simulation for exactly one loop iteration to test constraint solving.")]
    [SerializeField] bool doSingleLoopTest = false; /**< Whether to run the Single Loop Test. This test shifts one end of the guidewire and
                                                     *   runs the simulation for exactly one loop iteration to test constraint solving.
                                                     */
    [Tooltip("External force that is applied in Force Test Three.")]
    [SerializeField] Vector3 pullForceTestThree = new Vector3(0f, 3f, 0f); //!< External force that is applied in Force Test Three.

    private void Awake()
    {
        simulationLoop = GetComponent<SimulationLoop>();
        Assert.IsNotNull(simulationLoop);
    }

    private void Start()
    {
        PerformForceTests();
    }

    /**
     * Performs each Force Test whose respective serialized boolean is set to true in the Unity inspector.
     */
    private void PerformForceTests()
    {
        if (doForceTestOne) PerformForceTestOne();
        else if (doForceTestTwo) PerformForceTestTwo();
        else if (doForceTestThree) StartCoroutine(PerformForceTestThree(pullForceTestThree));
        else if (doForceTestFour) PerformForceTestFour();
        else if (doSingleLoopTest) PerformSingleLoopTest();
    }

    /**
     * Performs force test one. This test applies gravity to all spheres.
     */
    private void PerformForceTestOne()
    {
        Vector3 gravity = new Vector3(0f, -9.81f, 0f);

        for (int sphereIndex = 0; sphereIndex < simulationLoop.SpheresCount; sphereIndex++)
        {
            simulationLoop.sphereExternalForces[sphereIndex] = gravity;
        }
    }

    /**
     * Performs force test two. This test applies an external force to one end of the guidewire.
     */
    private void PerformForceTestTwo()
    {
        Vector3 pullForce = new Vector3(0f, 0.3f, 0f);

        for (int sphereIndex = 0; sphereIndex < (simulationLoop.SpheresCount - 1); sphereIndex++)
        {
            simulationLoop.sphereExternalForces[sphereIndex] = Vector3.zero;
        }

        simulationLoop.sphereExternalForces[simulationLoop.SpheresCount - 1] = pullForce;
    }

    /**
     * Performs force test three. This test applies an external force to one end of the guidewire
     * for a fixed amount of time and then the opposite force at the same sphere for the same amount of time.
     * @param applyForceTime For how many seconds to apply the force to the particles.
     */
    private IEnumerator PerformForceTestThree(Vector3 pullForce, float applyForceTime = 1f)
    {
        for (int sphereIndex = 0; sphereIndex < (simulationLoop.SpheresCount - 1); sphereIndex++)
        {
            simulationLoop.sphereExternalForces[sphereIndex] = Vector3.zero;
        }

        simulationLoop.sphereExternalForces[simulationLoop.SpheresCount - 1] = pullForce;

        yield return new WaitForSeconds(applyForceTime);

        simulationLoop.sphereExternalForces[simulationLoop.SpheresCount - 1] = -pullForce;

        yield return new WaitForSeconds(applyForceTime);

        simulationLoop.sphereExternalForces[simulationLoop.SpheresCount - 1] = Vector3.zero;

        Debug.Log("End of Force Test Three");
    }

    /**
     * Performs force test four. This test applies an external force to one end of the guidewire
     * and the opposite force at the other end of the guidewire.
     */
    private void PerformForceTestFour()
    {
        Vector3 pullForce = new Vector3(0f, 1f, 0f);

        simulationLoop.sphereExternalForces[0] = -pullForce;

        if (simulationLoop.SpheresCount > 2)
        {
            for (int sphereIndex = 1; sphereIndex < (simulationLoop.SpheresCount - 1); sphereIndex++)
            {
                simulationLoop.sphereExternalForces[sphereIndex] = Vector3.zero;
            }
        }

        simulationLoop.sphereExternalForces[simulationLoop.SpheresCount - 1] = pullForce;
    }

    /**
     * Performs the single loop test. This test shifts one end of the guidewire and runs the simulation for exactly
     * one loop iteration to test constraint solving.
     * @note Position of particle one stays at (0, 0, 0), while the section particle shifts to about (10, 2, 0). Expected result is that
     * both particles move a bit towards each other and reestablish a distance of 10 between them.
     */
    private void PerformSingleLoopTest()
    {
        Assert.IsTrue(simulationLoop.sphereVelocities.Length >= 2);

        // ARRANGE
        simulationLoop.ExecuteSingleLoopTest = true;
        simulationLoop.ConstraintSolverSteps = 1000;
        simulationLoop.sphereVelocities[simulationLoop.SpheresCount - 1] = new Vector3(0f, 100f, 0f);

        Debug.Log("Executing Single Loop Test.");
        Debug.Log("Constraint Solving Steps: " + simulationLoop.ConstraintSolverSteps);
        Debug.Log("The last sphere is displaced by velocity " + simulationLoop.sphereVelocities[simulationLoop.SpheresCount - 1]);
        Debug.Log("The distance between both spheres at rest state is "
                  + Vector3.Distance(simulationLoop.spherePositions[0], simulationLoop.spherePositions[simulationLoop.SpheresCount - 1]));

        // ACT
        simulationLoop.PerformSimulationLoop();

        // ASSERT
        Debug.Log("Sphere Positions after Update Step: " + simulationLoop.spherePositions[0] + simulationLoop.spherePositions[simulationLoop.SpheresCount - 1]);
        Debug.Log("The distance between both spheres after the update step is "
                  + Vector3.Distance(simulationLoop.spherePositions[0], simulationLoop.spherePositions[simulationLoop.SpheresCount - 1]));
    }
}
}
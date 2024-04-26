using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GuidewireSim
{
/**
 * This class enables the user to test the impact of external torques with one button within the Unity inspector.
 */
public class TorqueTestPerformer : MonoBehaviour
{
    SimulationLoop simulationLoop; //!< The SimulationLoop component that executes all steps of the simulation loop.

    [Tooltip("The external torque that is applied to the respective parts of the guidewire, depending on the test.")]
    [SerializeField] Vector3 pullTorque = new Vector3(0f, 0.3f, 0f); /**< The external torque that is applied to the respective parts of the guidewire,
                                                                      *   depending on the test.
                                                                      */
    [Tooltip("Applies an external torque to one end of the guidewire.")]
    [SerializeField] bool doTorqueTestOne = false; //!< Whether to run Torque Test One. This test applies an external torque to one end of the guidewire.
    [Tooltip("Applies an external torque to one end of the guidewire for a fixed amount of time and then the opposite torque"
    + "at the same orientation for the same amount of time.")]
    [SerializeField] bool doTorqueTestTwo = false; /**< Whether to run Torque Test Two. This test applies an external torque to one end of the guidewire for
                                                    *   a fixed amount of time and then the opposite torque at the same orientation for the same amount of time.
                                                    */
    [Tooltip("Applies an external torque to one end of the guidewire and at the same time the opposite torque at the other end of the guidewire."
    + "The applied torque starts at 0 and linearly interpolates until it reaches @p pullTorque at @p applyTorqueTime seconds.")]
    [SerializeField] bool doTorqueTestThree = false; /**< Whether to run Torque Test Three. This test applies an external torque to one end of the
                                                      *   guidewire and at the same time the opposite torque at the other end of the guidewire.
                                                      *   The applied torque starts at 0 and linearly interpolates until it reaches
                                                      *   @p pullTorque at @p applyTorqueTime seconds.
                                                      */

    private void Awake()
    {
        simulationLoop = GetComponent<SimulationLoop>();
        Assert.IsNotNull(simulationLoop);
    }

    private void Start()
    {
        PerformTorqueTests();
    }

    /**
     * Performs each Torque Test whose respective serialized boolean is set to true in the Unity inspector.
     */
    private void PerformTorqueTests()
    {
        if (doTorqueTestOne) PerformTorqueTestOne();
        else if (doTorqueTestTwo) StartCoroutine(PerformTorqueTestTwo(pullTorque));
        else if (doTorqueTestThree) StartCoroutine(PerformTorqueTestThree(pullTorque));
    }

    /**
     * Performs torque test one. This test applies an external torque to one end of the guidewire.
     */
    private void PerformTorqueTestOne()
    {
        for (int cylinderIndex = 0; cylinderIndex < (simulationLoop.CylinderCount - 1); cylinderIndex++)
        {
            simulationLoop.cylinderExternalTorques[cylinderIndex] = Vector3.zero;
        }

        simulationLoop.cylinderExternalTorques[simulationLoop.CylinderCount - 1] = pullTorque;
    }

    /**
     * Performs torque test two. This test applies an external torque to one end of the guidewire for a fixed amount of time
     * and then the opposite torque at the same orientation for the same amount of time.
     * @param pullTorque The external torque that is applied to one end of the guidewire.
     * @param applyTorqueTime For how many seconds to apply the torque to the orientations.
     * @req Output a log message when no further torques are applied to the guidewire.
     */
    private IEnumerator PerformTorqueTestTwo(Vector3 pullTorque, float applyTorqueTime = 1f)
    {
        for (int cylinderIndex = 0; cylinderIndex < (simulationLoop.CylinderCount - 1); cylinderIndex++)
        {
            simulationLoop.cylinderExternalTorques[cylinderIndex] = Vector3.zero;
        }

        simulationLoop.cylinderExternalTorques[simulationLoop.CylinderCount - 1] = pullTorque;

        yield return new WaitForSeconds(applyTorqueTime);

        simulationLoop.cylinderExternalTorques[simulationLoop.CylinderCount - 1] = -pullTorque;

        yield return new WaitForSeconds(applyTorqueTime);

        simulationLoop.cylinderExternalTorques[simulationLoop.CylinderCount - 1] = Vector3.zero;

        Debug.Log("End of Torque Test Two");
    }

    /**
     * Performs torque test three. This test applies an external torque to one end of the guidewire and at the same time the
     * opposite torque at the other end of the guidewire. The applied torque starts at 0 and linearly interpolates until it reaches
     * @p pullTorque at @p applyTorqueTime seconds.
     * @param pullTorque The external torque that is applied to one end of the guidewire.
     * @param applyTorqueTime For how many seconds to apply the torque to the orientations.
     * @req Output a log message when no further torques are applied to the guidewire.
     */
    private IEnumerator PerformTorqueTestThree(Vector3 pullTorque, float applyTorqueTime = 10f)
    {
        yield return new WaitForSeconds(1f);

        for (int cylinderIndex = 1; cylinderIndex < (simulationLoop.CylinderCount - 1); cylinderIndex++)
        {
            simulationLoop.cylinderExternalTorques[cylinderIndex] = Vector3.zero;
        }

        float elapsedTime = 0f;

        while (elapsedTime < applyTorqueTime)
        {
            Vector3 effectiveTorque = Vector3.Lerp(Vector3.zero, pullTorque, elapsedTime / applyTorqueTime);

            simulationLoop.cylinderExternalTorques[0] = - effectiveTorque;
            simulationLoop.cylinderExternalTorques[simulationLoop.CylinderCount - 1] = effectiveTorque;

            yield return null;

            elapsedTime += Time.deltaTime;
        }

        simulationLoop.cylinderExternalTorques[0] = Vector3.zero;
        simulationLoop.cylinderExternalTorques[simulationLoop.CylinderCount - 1] = Vector3.zero;

        Debug.Log("End of Torque Test Three");
    }
}
}
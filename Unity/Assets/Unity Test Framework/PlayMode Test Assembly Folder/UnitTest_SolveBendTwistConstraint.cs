using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Assertions;
using BSM = BulletSharp.Math;

/**
 * This class provides unit tests that test the method SolveBendTwistConstraint() of ConstraintSolvingStep.
 * Executing this test once generates @p sampleSize many random value pairs and executes the unit test with each of these pairs.
 */
public class UnitTest_SolveBendTwistConstraint
{
    int sampleSize = 10; /**< The number of value-pairs the test is executed with. E.g. if #sampleSize is 10, then the unit test
                          *   is executed with 10 randomly drawn value-pairs. A higher number needs more time to execute.
                          */
    int constraintSolverSteps = 50; //!< How often the constraint solver iterates over each constraint during the Constraint Solving Step.

    /**
     * Arranges all necessary data, generates #sampleSize many random value pairs, and then passes all data to Test_SolveBendTwistConstraint(),
     * where the unit tests are executed.
     * @note Only tests the case that all rod elements are aligned at rest state. If you want to test deformed rods at rest state, change
     * @p discreteRestDarbouxVector accordingly.
     */
    [UnityTest]
    public IEnumerator PerformUnitTests()
    {
        Debug.Log("Constraint Solver Steps: " + constraintSolverSteps);

        // ARRANGE
        GameObject simulationLoop = new GameObject("Simulation");
        simulationLoop.AddComponent<GuidewireSim.MathHelper>();
        simulationLoop.AddComponent<GuidewireSim.ConstraintSolvingStep>();

        UnityEngine.Assertions.Assert.IsNotNull(simulationLoop);
        GuidewireSim.MathHelper mathHelper = simulationLoop.GetComponent<GuidewireSim.MathHelper>();
        UnityEngine.Assertions.Assert.IsNotNull(mathHelper);
        GuidewireSim.ConstraintSolvingStep constraintSolvingStep = simulationLoop.GetComponent<GuidewireSim.ConstraintSolvingStep>();
        UnityEngine.Assertions.Assert.IsNotNull(constraintSolvingStep);

        yield return null;

        BSM.Quaternion orientationIdentity = new BSM.Quaternion(0f, 0f, 0f, 1f);
        Vector3 discreteRestDarbouxVector;
        float rodElementLength = 10f;

        discreteRestDarbouxVector = mathHelper.DiscreteDarbouxVector(orientationIdentity, orientationIdentity, rodElementLength);

        for (int sampleIteration = 0; sampleIteration < sampleSize; sampleIteration++)
        {
            BSM.Quaternion orientationOne = mathHelper.RandomUnitQuaternion();
            BSM.Quaternion orientationTwo = mathHelper.RandomUnitQuaternion();

            // ACT
            Test_SolveBendTwistConstraint(constraintSolverSteps, orientationOne, orientationTwo, rodElementLength, discreteRestDarbouxVector,
                                          mathHelper, constraintSolvingStep);
        }
    }

    /**
     * Executes SolveBendTwistConstraint() of ConstraintSolvingStep @p iterations many times for one values pair, and then asserts whether
     * the results of the algorithm of SolveBendTwistConstraint() converged towards the expected values.
     * @param iterations The number of iterations that SolveBendTwistConstraint() of ConstraintSolvingStep is executed.
     * @param orientationOne The first orientation for SolveBendTwistConstraint().
     * @param orientationOne The second orientation for SolveBendTwistConstraint().
     * @param rodElementLength The rod element length for SolveBendTwistConstraint().
     * @param discreteRestDarbouxVector The discrete Darboux Vector at rest state for SolveBendTwistConstraint().
     * @param mathHelper The component MathHelper.
     * @param constraintSolvingStep The component ConstraintSolvingStep.
     * @req @p orientationOne and orientationTwo are still unit quaternions at the end of the test.
     * @req The deviation between the bend twist constraint and zero is lower than a reasonable tolerance, i.e. close to zero.,
     * which means that the algorithm of SolveBendTwistConstraint() converges towards the fulfillment of the bend twist constraint.
     */
    private void Test_SolveBendTwistConstraint(int iterations, BSM.Quaternion orientationOne, BSM.Quaternion orientationTwo, float rodElementLength,
                                               Vector3 discreteRestDarbouxVector, GuidewireSim.MathHelper mathHelper,
                                               GuidewireSim.ConstraintSolvingStep constraintSolvingStep)
    {
        // ARRANGE
        BSM.Quaternion deltaOrientationOne;
        BSM.Quaternion deltaOrientationTwo;

        Debug.Log("New Unit Test");
        Debug.Log("Start value orientation one: " + orientationOne.ToString("e2"));
        Debug.Log("Start value orientation two: " + orientationTwo.ToString("e2"));
        float startDeviation = mathHelper.BendTwistConstraintDeviation(orientationOne, orientationTwo, rodElementLength, discreteRestDarbouxVector);
        Debug.Log("Start Bend Twist Constraint Deviation: " + startDeviation.ToString("e2"));

        for (int iteration = 1; iteration < (iterations+1); iteration++)
        {
            // ACT
            constraintSolvingStep.SolveBendTwistConstraint(orientationOne, orientationTwo, discreteRestDarbouxVector, rodElementLength,
                                                           out deltaOrientationOne, out deltaOrientationTwo);
            orientationOne += deltaOrientationOne;
            orientationTwo += deltaOrientationTwo;

            orientationOne.Normalize();
            orientationTwo.Normalize();
        }

        // ASSERT
        float endDeviation = mathHelper.BendTwistConstraintDeviation(orientationOne, orientationTwo, rodElementLength, discreteRestDarbouxVector);
        Debug.Log("End Bend Twist Constraint Deviation: " + endDeviation.ToString("e2"));

        UnityEngine.Assertions.Assert.AreApproximatelyEqual(1f, mathHelper.QuaternionLength(orientationOne), tolerance: 0.01f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(1f, mathHelper.QuaternionLength(orientationTwo), tolerance: 0.01f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0f, mathHelper.BendTwistConstraintDeviation(orientationOne, orientationTwo, rodElementLength,
                                                                                 discreteRestDarbouxVector), tolerance: 0.1f);
    }
}
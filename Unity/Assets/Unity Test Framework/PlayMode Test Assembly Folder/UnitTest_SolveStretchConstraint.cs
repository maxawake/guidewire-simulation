using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Assertions;
using BSM = BulletSharp.Math;

/**
 * This class provides unit tests that test the method SolveStretchConstraint() of ConstraintSolvingStep.
 * Executing this test once generates @p sampleSize many random value pairs and executes the unit test with each of these pairs.
 */
public class UnitTest_SolveStretchConstraint
{
    float maximalDistanceOffset = 1f; /**< The maximal deviation from the rest @p rodElementLength.
                                       *   @exampletext Let @p rodElementLength be 10 and #maximalDistanceOffset be 2.
                                       *   Then the two random particle positions drawn will have a distance between 8 and 12.
                                       */
    int sampleSize = 10; /**< The number of value-pairs the test is executed with. E.g. if #sampleSize is 10, then the unit test
                          *   is executed with 10 randomly drawn value-pairs. A higher number needs more time to execute.
                          */
    int constraintSolverSteps = 1000; //!< How often the constraint solver iterates over each constraint during the Constraint Solving Step.
    float rodElementLength = 10f; //!< The distance between two spheres, also the distance between two orientations.

    /**
     * Arranges all necessary data, generates #sampleSize many random value pairs, and then passes all data to Test_SolveStretchConstraint(),
     * where the unit tests are executed.
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

        for (int sampleIteration = 0; sampleIteration < sampleSize; sampleIteration++)
        {
            Vector3 particlePositionOne, particlePositionTwo;
            PickRandomPositions(out particlePositionOne, out particlePositionTwo);

            // ACT
            Test_SolveStretchConstraint(1000, particlePositionOne, particlePositionTwo, new BSM.Quaternion(0f, 0f, 0f, 1f), mathHelper, constraintSolvingStep);
        }
    }

    /**
     * Picks the first particle position uniformly distributed with \f$ x,y,z \in [-5, 5] \f$ and the second uniformly distributed
     * around the first position with a uniformly distributed distance in
     * \f$ [rodElementLength - maximalDistanceOffset, rodElementLength + maximalDistanceOffset] \f$.
     * @note The method for picking the second position is inspired by https://math.stackexchange.com/q/50482
     * @param[out] particlePositionOne The first particle position that got picked.
     * @param[out] particlePositionTwo The second particle position that got picked.
     * @req Picks the first particle position uniformly distributed so that \f$ x,y,z \in [-5, 5] \f$.
     * @req Picks a distance between the two particles that is uniformly distributed in the interval
     * \f$ [rodElementLength - maximalDistanceOffset, rodElementLength + maximalDistanceOffset] \f$.
     * @req Picks the second particle position uniformly distributed on the surface of the sphere with
     * center @p particlePositionOne and radius @p startDistance.
     */
    private void PickRandomPositions(out Vector3 particlePositionOne, out Vector3 particlePositionTwo)
    {
        float startDistance = rodElementLength + UnityEngine.Random.Range(-maximalDistanceOffset, maximalDistanceOffset);

        float positionOneX = UnityEngine.Random.Range(-5f, 5f);
        float positionOneY = UnityEngine.Random.Range(-5f, 5f);
        float positionOneZ = UnityEngine.Random.Range(-5f, 5f);

        particlePositionOne = new Vector3(positionOneX, positionOneY, positionOneZ);

        // Pick a point uniformly distributed on the surface of a sphere with center @p particlePositionOne and radius @p startDistance.
        float positionOnUnitSphereZ = UnityEngine.Random.Range(-1f, 1f);
        float deltaAngle = UnityEngine.Random.Range(0f, 2 * Mathf.PI);
        float r = Mathf.Sqrt(1 - Mathf.Pow(positionOnUnitSphereZ, 2));

        float positionOnUnitSphereX = r * Mathf.Cos(deltaAngle);
        float positionOnUnitSphereY = r * Mathf.Sin(deltaAngle);

        float positionTwoX = positionOneX + startDistance * positionOnUnitSphereX;
        float positionTwoY = positionOneY + startDistance * positionOnUnitSphereY;
        float positionTwoZ = positionOneZ + startDistance * positionOnUnitSphereZ;

        particlePositionTwo = new Vector3(positionTwoX, positionTwoY, positionTwoZ);
    }

    /**
     * Executes SolveStretchConstraint() of ConstraintSolvingStep @p iterations many times for one values pair, and then asserts whether
     * the results of the algorithm of SolveStretchConstraint() converged towards the expected values.
     * @param iterations The number of iterations that SolveStretchConstraint() of ConstraintSolvingStep is executed.
     * @param particlePositionOne The first particle position for SolveStretchConstraint().
     * @param particlePositionTwo The second particle position for SolveStretchConstraint().
     * @param orientation The orientation for SolveStretchConstraint().
     * @param mathHelper The component MathHelper.
     * @param constraintSolvingStep The component ConstraintSolvingStep.
     * @req @p orientation is still a unit quaternion at the end of the test.
     * @req The deviation between the stretch constraint and zero is lower than the tolerance 0.1, which means that the algorithm of
     * SolveStretchConstraint() converges towards the fulfillment of the stretch constraint.
     * @req The deviation between the actual distance of @p particlePositionOne and @p particlePositionTwo and the rest rod element length is
     * lower than a reasonable tolerance, i.e. close to zero.
     * @attention The fulfillment of the requirement that the rod element length converges towards the rest rod element length depends on the
     * initial deviation of both particle positions from each other and is just a byproduct of converging towards the constraint fulfillment.
     * If this requirement is not fulfilled, the initial offset or the number of iterations was probably simply to high or low, respectively.
     */
    private void Test_SolveStretchConstraint(int iterations, Vector3 particlePositionOne, Vector3 particlePositionTwo, BSM.Quaternion orientation,
                                             GuidewireSim.MathHelper mathHelper, GuidewireSim.ConstraintSolvingStep constraintSolvingStep)
    {
        // ARRANGE
        BSM.Quaternion e_3 = new BSM.Quaternion(0f, 0f, 1f, 0f);
        float rodElementLength = 10f;
        Vector3 deltaPositionOne;
        Vector3 deltaPositionTwo;
        BSM.Quaternion deltaOrientation;

        Debug.Log("New Unit Test");
        Debug.Log("Start value particle position one: " + particlePositionOne);
        Debug.Log("Start value particle position two: " + particlePositionTwo);
        Debug.Log("Start value orientation: " + orientation);
        float startDeviation = mathHelper.StretchConstraintDeviation(particlePositionOne, particlePositionTwo, orientation, e_3, rodElementLength);
        Debug.Log("Start Stretch Constraint Deviation: " + startDeviation.ToString("e2"));


        for (int iteration = 1; iteration < (iterations+1); iteration++)
        {
            // ACT
            constraintSolvingStep.SolveStretchConstraint(particlePositionOne, particlePositionTwo, orientation, e_3, rodElementLength,
                                                         out deltaPositionOne, out deltaPositionTwo, out deltaOrientation);
            particlePositionOne += deltaPositionOne;
            particlePositionTwo += deltaPositionTwo;
            orientation += deltaOrientation;

            orientation.Normalize();
        }

        // ASSERT
        float endDeviation = mathHelper.StretchConstraintDeviation(particlePositionOne, particlePositionTwo, orientation, e_3, rodElementLength);
        Debug.Log("End Stretch Constraint Deviation: " + endDeviation.ToString("e2"));
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(1f, mathHelper.QuaternionLength(orientation), tolerance: 0.01f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0f, mathHelper.RodElementLengthDeviation(particlePositionOne, particlePositionTwo, rodElementLength),
                                                            tolerance: 0.03f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0f, mathHelper.StretchConstraintDeviation(particlePositionOne, particlePositionTwo,
                                                            orientation, e_3, rodElementLength), tolerance: 0.1f);
    }
}
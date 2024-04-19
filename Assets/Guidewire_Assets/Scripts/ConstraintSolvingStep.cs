using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using BSM = BulletSharp.Math;
using System.IO; 

namespace GuidewireSim
{
/**
 * This class executes and implements various algorithms of the constraint solving step of the algorithm and manages all coherent data.
 */
public class ConstraintSolvingStep : MonoBehaviour
{   
    // TODO: Check if can be outsourced
    private SimulationLoop simulationLoop; // Declare simulationLoop
    private float rodElementLength; // Declare rodElementLength

    MathHelper mathHelper; //!< The component MathHelper that provides math related helper functions.

    Vector3 deltaPositionOne = new Vector3(); //!< The correction of @p particlePositionOne in method SolveStretchConstraint().
    Vector3 deltaPositionTwo = new Vector3(); //!< The correction of @p particlePositionTwo in method SolveStretchConstraint().
    BSM.Quaternion deltaOrientation = new BSM.Quaternion(); //!< The correction of @p orientation in method SolveStretchConstraint().
    BSM.Quaternion deltaOrientationOne = new BSM.Quaternion(); //!< The correction of @p orientationOne in method SolveBendTwistConstraint().
    BSM.Quaternion deltaOrientationTwo = new BSM.Quaternion(); //!< The correction of @p orientationTwo in method SolveBendTwistConstraint().

    // TODO: Check value
    float stretchStiffness = 0.1f;
    float bendStiffness = 0.1f;

    [Tooltip("Whether to solve both constraints in bilateral interleaving order. Naive order is used when false.")]
    [SerializeField] bool executeInBilateralOrder = false; //!< Whether to solve both constraints in bilateral interleaving order. Naive order is used when false.

    private void Awake()
    {
        mathHelper = GetComponent<MathHelper>();
        Assert.IsNotNull(mathHelper);
        // TODO: Check if can be outsourced
        simulationLoop = GetComponent<SimulationLoop>(); // Initialize simulationLoop
        rodElementLength = simulationLoop.GetRodElementLength(); // Initialize rodElementLength
    }

    /**
     * Is responsible for executing one iteration of the constraint solving step for the stretch constraint, i.e. corrects each particle
     * position prediction one time and also each orientation prediction one time.
     * @note Can be executed in naive order or bilateral interleaving order.
     * @param spherePositionPredictions The array of position predictions that get corrected in this step.
     * @param cylinderOrientationPredictions The array of orientation predictions that get corrected in this step.
     * @param spheresCount The count of all spheres of the guidewire. Equals the length of @p spherePositionPredictions.
     * @param worldSpaceBasis The three basis vectors of the world coordinate system as embedded quaternions with scalar part 0.
     * @param rodElementLength The distance between two spheres, also the distance between two orientations.
     * @req @p spheresCount should be at least one.
     * @req @p rodElementLength should be positive.
     * @req Executes the constraint solving step in bilateral interleaving order if #executeInBilateralOrder and otherwise in naive order.
     */
    public void SolveStretchConstraints(Vector3[] spherePositionPredictions, BSM.Quaternion[] cylinderOrientationPredictions, int spheresCount, BSM.Quaternion[] worldSpaceBasis, float rodElementLength)
    {
        Assert.IsTrue(spheresCount >= 1);
        Assert.IsTrue(rodElementLength > 0f);

        BSM.Quaternion e_3 = worldSpaceBasis[2];

        if (executeInBilateralOrder)
        {
            SolveStretchConstraintsInBilateralOrder(spherePositionPredictions, cylinderOrientationPredictions, spheresCount, rodElementLength, e_3);
        }
        else
        {
            SolveStretchConstraintsInNaiveOrder(spherePositionPredictions, cylinderOrientationPredictions, spheresCount, rodElementLength, e_3);
        }
    }

    /**
     * Is responsible for executing one iteration of the constraint solving step for the bend twist constraint, i.e. corrects each orientation
     * prediction one time.
     * @note Can be executed in naive order or bilateral interleaving order.
     * @param cylinderOrientationPredictions The array of orientation predictions that get corrected in this step.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param discreteRestDarbouxVectors The array of all discrete Darboux Vectors at the rest configuration, i.e. at frame 0. Has (n-1) elements,
     * if n is the number of orientations of the guidewire, because the darboux vector is taken of two adjacent orientations.
     * @param rodElementLength The distance between two spheres, also the distance between two orientations.
     * @req @p cylinderCount should be at least one.
     * @req @p rodElementLength should be positive.
     * @req Executes the constraint solving step in bilateral interleaving order if #executeInBilateralOrder and otherwise in naive order.
     */
    public void SolveBendTwistConstraints(BSM.Quaternion[] cylinderOrientationPredictions, int cylinderCount, Vector3[] discreteRestDarbouxVectors,
                                          float rodElementLength)
    {
        Assert.IsTrue(cylinderCount >= 1);
        Assert.IsTrue(rodElementLength > 0f);

        if (executeInBilateralOrder)
        {
            SolveBendTwistConstraintsInBilateralOrder(cylinderOrientationPredictions, cylinderCount, discreteRestDarbouxVectors, rodElementLength);
        }
        else
        {
            SolveBendTwistConstraintsInNaiveOrder(cylinderOrientationPredictions, cylinderCount, discreteRestDarbouxVectors, rodElementLength);
        }
    }

    /**
     * Executes one iteration of the constraint solving step for the stretch constraint in bilateral order, i.e. corrects each particle
     * position prediction one time and also each orientation prediction one time.
     * @note You can read more about bilateral order in the 2016 paper "Position and Orientation Based Cosserat Rods".
     * @attention The index shifting of this algorithm is not easy to understand, but got deeply tested.
     * @param spherePositionPredictions The array of position predictions that get corrected in this step.
     * @param cylinderOrientationPredictions The array of orientation predictions that get corrected in this step.
     * @param spheresCount The count of all spheres of the guidewire. Equals the length of @p spherePositionPredictions.
     * @param rodElementLength The distance between two spheres, also the distance between two orientations.
     * @param e_3 The third basis vector of the world coordinate system as embedded quaternions with scalar part 0.
     * @req @p spheresCount should be at least one.
     * @req @p rodElementLength should be positive.
     */
    private void SolveStretchConstraintsInBilateralOrder(Vector3[] spherePositionPredictions, BSM.Quaternion[] cylinderOrientationPredictions,
                                                         int spheresCount, float rodElementLength, BSM.Quaternion e_3)
    {
        Assert.IsTrue(spheresCount >= 1);
        Assert.IsTrue(rodElementLength > 0f);
        
        int upcountingIndex = 0;
        int downcountingIndex;
        int lastAscendingIndex = spheresCount - 2;

        if (spheresCount % 2 == 0) // e.g. spheresCount is an even number
        {
            downcountingIndex = lastAscendingIndex - 1;

            SolveStretchConstraint(spherePositionPredictions[lastAscendingIndex], spherePositionPredictions[lastAscendingIndex + 1],
                                   cylinderOrientationPredictions[lastAscendingIndex], e_3, rodElementLength, out deltaPositionOne,
                                   out deltaPositionTwo, out deltaOrientation);
            CorrectStretchPredictions(lastAscendingIndex, spherePositionPredictions, cylinderOrientationPredictions);
        }
        else // spheresCount % 2 == 1, e.g. spheresCount is an odd number
        {
            downcountingIndex = lastAscendingIndex;
        }

        while (upcountingIndex < lastAscendingIndex)
        {
            // upcounting
            SolveStretchConstraint(spherePositionPredictions[upcountingIndex], spherePositionPredictions[upcountingIndex + 1],
                                   cylinderOrientationPredictions[upcountingIndex], e_3, rodElementLength, out deltaPositionOne,
                                   out deltaPositionTwo, out deltaOrientation);
            CorrectStretchPredictions(upcountingIndex, spherePositionPredictions, cylinderOrientationPredictions);

            // downcounting
            SolveStretchConstraint(spherePositionPredictions[downcountingIndex], spherePositionPredictions[downcountingIndex + 1],
                                   cylinderOrientationPredictions[downcountingIndex], e_3, rodElementLength, out deltaPositionOne,
                                   out deltaPositionTwo, out deltaOrientation);
            CorrectStretchPredictions(downcountingIndex, spherePositionPredictions, cylinderOrientationPredictions);

            upcountingIndex += 2;
            downcountingIndex -= 2;
        }
    }

    /**
     * Executes one iteration of the constraint solving step for the stretch constraint in naive order, i.e. corrects each particle
     * position prediction one time and also each orientation prediction one time.
     * @note Naive order means the predictions are updated beginning from one end of the guidewire to the other end of the guidewire.
     * @param spherePositionPredictions The array of position predictions that get corrected in this step.
     * @param cylinderOrientationPredictions The array of orientation predictions that get corrected in this step.
     * @param spheresCount The count of all spheres of the guidewire. Equals the length of @p spherePositionPredictions.
     * @param rodElementLength The distance between two spheres, also the distance between two orientations.
     * @param e_3 The third basis vector of the world coordinate system as embedded quaternions with scalar part 0.
     * @req @p spheresCount should be at least one.
     * @req @p rodElementLength should be positive.
     */
    private void SolveStretchConstraintsInNaiveOrder(Vector3[] spherePositionPredictions, BSM.Quaternion[] cylinderOrientationPredictions,
                                                     int spheresCount, float rodElementLength, BSM.Quaternion e_3)
    {
        Assert.IsTrue(spheresCount >= 1);
        Assert.IsTrue(rodElementLength > 0f);

        for (int sphereIndex = 0; sphereIndex < spheresCount - 1; sphereIndex++)
        {
            SolveStretchConstraint(spherePositionPredictions[sphereIndex], spherePositionPredictions[sphereIndex + 1],
                                   cylinderOrientationPredictions[sphereIndex], e_3, rodElementLength, out deltaPositionOne,
                                   out deltaPositionTwo, out deltaOrientation);
            CorrectStretchPredictions(sphereIndex, spherePositionPredictions, cylinderOrientationPredictions);
        }
    }

    /**
     * Is responsible for executing one iteration of the constraint solving step for the bend twist constraint in bilateral order,
     * i.e. corrects each orientation prediction one time.
     * @note You can read more about bilateral order in the 2016 paper "Position and Orientation Based Cosserat Rods".
     * @attention The index shifting of this algorithm is not easy to understand, but got deeply tested.
     * @param cylinderOrientationPredictions The array of orientation predictions that get corrected in this step.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param discreteRestDarbouxVectors The array of all discrete Darboux Vectors at the rest configuration, i.e. at frame 0. Has (n-1) elements,
     * if n is the number of orientations of the guidewire, because the darboux vector is taken of two adjacent orientations.
     * @param rodElementLength The distance between two spheres, also the distance between two orientations.
     * @req @p cylinderCount should be at least one.
     * @req @p rodElementLength should be positive.
     */
    private void SolveBendTwistConstraintsInBilateralOrder(BSM.Quaternion[] cylinderOrientationPredictions, int cylinderCount,
                                                           Vector3[] discreteRestDarbouxVectors, float rodElementLength)
    {
        Assert.IsTrue(cylinderCount >= 1);
        Assert.IsTrue(rodElementLength > 0f);

        int upcountingIndex = 0;
        int downcountingIndex;
        int lastAscendingIndex = cylinderCount - 2;

        if (cylinderCount % 2 == 0) // e.g. cylinderCount is an even number
        {
            downcountingIndex = lastAscendingIndex - 1;

            SolveBendTwistConstraint(cylinderOrientationPredictions[lastAscendingIndex], cylinderOrientationPredictions[lastAscendingIndex + 1],
                                     discreteRestDarbouxVectors[lastAscendingIndex], rodElementLength, out deltaOrientationOne,
                                     out deltaOrientationTwo);
            CorrectBendTwistPredictions(lastAscendingIndex, cylinderOrientationPredictions);
        }
        else // cylinderCount % 2 == 1, e.g. sphereLength is an odd number
        {
            downcountingIndex = lastAscendingIndex;
        }

        while (upcountingIndex < lastAscendingIndex)
        {
            // upcounting
            SolveBendTwistConstraint(cylinderOrientationPredictions[upcountingIndex], cylinderOrientationPredictions[upcountingIndex + 1],
                                     discreteRestDarbouxVectors[upcountingIndex], rodElementLength, out deltaOrientationOne,
                                     out deltaOrientationTwo);
            CorrectBendTwistPredictions(upcountingIndex, cylinderOrientationPredictions);

            // downcounting
            SolveBendTwistConstraint(cylinderOrientationPredictions[downcountingIndex], cylinderOrientationPredictions[downcountingIndex + 1],
                                     discreteRestDarbouxVectors[downcountingIndex], rodElementLength, out deltaOrientationOne,
                                     out deltaOrientationTwo);
            CorrectBendTwistPredictions(downcountingIndex, cylinderOrientationPredictions);

            upcountingIndex += 2;
            downcountingIndex -= 2;
        }
    }

    /**
     * Is responsible for executing one iteration of the constraint solving step for the bend twist constraint in naive order,
     * i.e. corrects each orientation prediction one time.
     * @note Naive order means the predictions are updated beginning from one end of the guidewire to the other end of the guidewire.
     * @param cylinderOrientationPredictions The array of orientation predictions that get corrected in this step.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param discreteRestDarbouxVectors The array of all discrete Darboux Vectors at the rest configuration, i.e. at frame 0. Has (n-1) elements,
     * if n is the number of orientations of the guidewire, because the darboux vector is taken of two adjacent orientations.
     * @param rodElementLength The distance between two spheres, also the distance between two orientations.
     * @req @p cylinderCount should be at least one.
     * @req @p rodElementLength should be positive.
     */
    private void SolveBendTwistConstraintsInNaiveOrder(BSM.Quaternion[] cylinderOrientationPredictions, int cylinderCount,
                                                       Vector3[] discreteRestDarbouxVectors, float rodElementLength)
    {
        Assert.IsTrue(cylinderCount >= 1);
        Assert.IsTrue(rodElementLength > 0f);

        for (int cylinderIndex = 0; cylinderIndex < cylinderCount - 1; cylinderIndex++)
        {
            Vector3 discreteRestDarbouxVector = discreteRestDarbouxVectors[cylinderIndex];

            SolveBendTwistConstraint(cylinderOrientationPredictions[cylinderIndex], cylinderOrientationPredictions[cylinderIndex + 1],
                                     discreteRestDarbouxVector, rodElementLength, out deltaOrientationOne,
                                     out deltaOrientationTwo);
            CorrectBendTwistPredictions(cylinderIndex, cylinderOrientationPredictions);
        }
    }

    /**
     * Solves the stretch constraint by calculating the corrections @p deltaPositionOne and @p deltaPositionTwo, @p deltaOrientation.
     * @note To be more precise, the stretch constraint is not solved but minimized, i.e. the constraint will after
     * correcting with the corrections be closer to zero.
     * @param particlePositionOne The first particle position prediction of the centerline element to be corrected.
     * @param particlePositionTwo The second particle position prediction of the centerline element to be corrected.
     * @param orientation The  orientation quaternion prediction of the orientation element between the particle positions to be corrected.
     * @param e_3 The third basis vector of the world space coordinates embedded as a quaternion with scalar part 0.
     * @param rodElementLength The distance between two spheres, also the distance between two orientations.
     * @param[out] deltaPositionOne The correction of @p particlePositionOne.
     * @param[out] deltaPositionTwo The correction of @p particlePositionTwo.
     * @param[out] deltaOrientation The correction of @p orientation.
     * @param inverseMassOne The inverse mass scalar for @p particlePositionOne. Use a value of 1 for a moving particle and 0 for a fixed particle.
     * @param inverseMassTwo The inverse mass scalar for @p particlePositionTwo. Use a value of 1 for a moving particle and 0 for a fixed particle.
     * @param inertiaWeight The inertia weight scalar for @p orientation. Use a value of 1 for a moving orientation and 0 for a fixed orientation.
     * @req @p orientation should be a unit quaternions, i.e. have length approximately equal to one.
     * @req @p e_3 should be a unit quaternions, i.e. have length approximately equal to one.
     * @req @p rodElementLength should be positive.
     * @req @p inverseMassOne, @p inverseMassTwo and @p inertiaWeight should be values between 0 and 1.
     */
    public void SolveStretchConstraint(Vector3 particlePositionOne, Vector3 particlePositionTwo, BSM.Quaternion orientation,
                                       BSM.Quaternion e_3, float rodElementLength, out Vector3 deltaPositionOne,
                                       out Vector3 deltaPositionTwo, out BSM.Quaternion deltaOrientation, float inverseMassOne = 1f,
                                       float inverseMassTwo = 1f, float inertiaWeight = 1f)
    {
        // TODO: Check if needed
        //float inverseMassValue = ((1000/rodElementLength)+1)/10f; 
        // TODO: Why is value changed?
        Assert.AreApproximatelyEqual(1f, mathHelper.QuaternionLength(orientation), tolerance: 0.01f);
        Assert.AreApproximatelyEqual(1f, mathHelper.QuaternionLength(e_3), tolerance: 0.01f);
        Assert.IsTrue(rodElementLength > 0f);
        Assert.IsTrue(inverseMassOne >= 0f && inverseMassOne <= 1f);
        Assert.IsTrue(inverseMassTwo >= 0f && inverseMassTwo <= 1f);
        Assert.IsTrue(inertiaWeight >= 0f && inertiaWeight <= 1f);

        Vector3 thirdDirector = mathHelper.ImaginaryPart(orientation * e_3 * BSM.Quaternion.Conjugate(orientation));
        // TODO: Why do we need this?
        // float denominator = inverseMassValue + inverseMassValue + 4 * inertiaWeight * rodElementLength * rodElementLength;
        float denominator = inverseMassOne + inverseMassTwo + 4 * inertiaWeight * rodElementLength * rodElementLength;

        Vector3 factor = (1f / rodElementLength) * (particlePositionTwo - particlePositionOne) - thirdDirector;
        BSM.Quaternion embeddedFactor = mathHelper.EmbeddedVector(factor);
        
        float quaternionScalarFactor = 2f * inertiaWeight * rodElementLength * rodElementLength / denominator;
        BSM.Quaternion quaternionProduct = embeddedFactor * orientation * BSM.Quaternion.Conjugate(e_3);

        // TODO: Why is value changed?
        // deltaPositionOne = inverseMassValue * rodElementLength * factor / denominator;
        // deltaPositionTwo = - inverseMassValue * rodElementLength * factor / denominator;
        deltaPositionOne = inverseMassOne * rodElementLength * factor / denominator;
        deltaPositionTwo = - inverseMassTwo * rodElementLength * factor / denominator;

        deltaOrientation = quaternionScalarFactor * quaternionProduct;

        // TODO: See if can be oursourced
        //string debugFilePath = "/home/max/Temp/Praktikum/DebugConstraint.txt";
    	//string content = $"inverseMassValue: {inverseMassValue}, denominator: {denominator}\n";
    	//File.AppendAllText(debugFilePath, content);
    }

    /**
     * Solves the bend twist constraint by calculating the corrections @p deltaOrientationOne and @p deltaOrientationTwo.
     * @note To be more precise, the bend twist constraint is not solved but minimized, i.e. the constraint will after
     * correcting with the corrections be closer to zero.
     * @param orientationOne The first orientation quaternion prediction of the orientation element to be corrected.
     * @param orientationTwo The second orientation quaternion prediction of the orientation element to be corrected.
     * @param discreteRestDarbouxVector The discrete Darboux Vector at the rest configuration, i.e. at frame 0.
     * @param rodElementLength The distance between two spheres, also the distance between two orientations.
     * @param[out] deltaOrientationOne The correction of @p orientationOne.
     * @param[out] deltaOrientationTwo The correction of @p orientationTwo.
     * @param inertiaWeightOne The inertia weight scalar for @p orientationOne. Use a value of 1 for a moving orientation and 0 for a fixed orientation.
     * @param inertiaWeightTwo The inertia weight scalar for @p orientationTwo. Use a value of 1 for a moving orientation and 0 for a fixed orientation.
     * @req @p orientationOne and @p orientationTwo should be unit quaternions, i.e. have length approximately equal to one.
     * @req @p rodElementLength should be positive.
     * @req @p inertiaWeightOne and @p inertiaWeightTwo should be values between 0 and 1.
     */
    public void SolveBendTwistConstraint(BSM.Quaternion orientationOne, BSM.Quaternion orientationTwo, Vector3 discreteRestDarbouxVector,
                                         float rodElementLength, out BSM.Quaternion deltaOrientationOne,
                                         out BSM.Quaternion deltaOrientationTwo, float inertiaWeightOne = 1f, float inertiaWeightTwo = 1f)
    {
        Assert.AreApproximatelyEqual(1f, mathHelper.QuaternionLength(orientationOne), tolerance: 0.01f);
        Assert.AreApproximatelyEqual(1f, mathHelper.QuaternionLength(orientationTwo), tolerance: 0.01f);
        Assert.IsTrue(rodElementLength > 0f);
        Assert.IsTrue(inertiaWeightOne >= 0f && inertiaWeightOne <= 1f);
        Assert.IsTrue(inertiaWeightTwo >= 0f && inertiaWeightTwo <= 1f);

        float denominator = inertiaWeightOne + inertiaWeightTwo;
        Vector3 discreteDarbouxVector = mathHelper.DiscreteDarbouxVector(orientationOne, orientationTwo, rodElementLength);
        float darbouxSignFactor = mathHelper.DarbouxSignFactor(discreteDarbouxVector, discreteRestDarbouxVector);
        
        Vector3 darbouxDifference = discreteDarbouxVector - darbouxSignFactor * discreteRestDarbouxVector;
        BSM.Quaternion embeddedDarbouxDifference = mathHelper.EmbeddedVector(darbouxDifference);

        deltaOrientationOne = inertiaWeightOne * orientationTwo * embeddedDarbouxDifference / denominator;
        deltaOrientationTwo = - inertiaWeightTwo * orientationOne * embeddedDarbouxDifference / denominator;
    }

    /**
     * Corrects the predictions of the stretch constraint by adding @p deltaPositionOne, @p deltaPositionTwo and @p deltaOrientation.
     * @note Note that @p deltaOrientation may has a length unequal one by definition.
     * @param sphereIndex The index of the first element of @p spherePositionPredictions that gets corrected.
     * @param spherePositionPredictions The array of position predictions of which two positions get corrected in this method.
     * @param cylinderOrientationPredictions The array of orientation predictions of which one quaternions gets corrected in this method.
     * @req The relevant entries of @p cylinderOrientationPredictions should be unit quaternions, i.e. have length approximately equal to one.
     * @req After the quaternion prediction got corrected, it should again be a unit quaternions, i.e. have length approximately equal to one.
     */
private void CorrectStretchPredictions(int sphereIndex, Vector3[] spherePositionPredictions, BSM.Quaternion[] cylinderOrientationPredictions)
{
    Assert.IsTrue(sphereIndex >= 0);
    Assert.AreApproximatelyEqual(1f, mathHelper.QuaternionLength(cylinderOrientationPredictions[sphereIndex]), tolerance: 0.01f);

    spherePositionPredictions[sphereIndex] += stretchStiffness * deltaPositionOne;
    spherePositionPredictions[sphereIndex + 1] += stretchStiffness * deltaPositionTwo;
    cylinderOrientationPredictions[sphereIndex] += stretchStiffness * deltaOrientation;

    cylinderOrientationPredictions[sphereIndex].Normalize();

    Assert.AreApproximatelyEqual(1f, mathHelper.QuaternionLength(cylinderOrientationPredictions[sphereIndex]), tolerance: 0.01f);
    // TODO: Check if can be outsourced
    string path = "/home/max/Temp/Praktikum/LogConstraints.txt";

    using (StreamWriter writer = new StreamWriter(path, true)) // true to append data to the file
    {
        // Log the deltaPositionOne and deltaPositionTwo to the file
        //writer.WriteLine($"Delta Position One: {1000 * deltaPositionOne}");
        //writer.WriteLine($"Delta Position Two: {1000 * deltaPositionTwo}");
    }
}

    /**
     * Corrects the predictions of the bend twist constraint by adding @p deltaOrientationOne and @p deltaOrientationTwo.
     * @note Note that @p deltaOrientationOne and @p deltaOrientationTwo may have a length unequal one by definition.
     * @param cylinderIndex The index of the first element of @p cylinderOrientationPredictions that gets corrected.
     * @param cylinderOrientationPredictions The array of orientation predictions of which two quaternions get corrected in this method.
     * @req The relevant entries of @p cylinderOrientationPredictions should be unit quaternions, i.e. have length approximately equal to one.
     * @req After the quaternion predictions got corrected, they should again be unit quaternions, i.e. have length approximately equal to one.
     */
    private void CorrectBendTwistPredictions(int cylinderIndex, BSM.Quaternion[] cylinderOrientationPredictions)
    {
        Assert.IsTrue(cylinderIndex >= 0);
        Assert.AreApproximatelyEqual(1f, mathHelper.QuaternionLength(cylinderOrientationPredictions[cylinderIndex]), tolerance: 0.01f);
        Assert.AreApproximatelyEqual(1f, mathHelper.QuaternionLength(cylinderOrientationPredictions[cylinderIndex + 1]), tolerance: 0.01f);

        cylinderOrientationPredictions[cylinderIndex] += bendStiffness * deltaOrientationOne;
        cylinderOrientationPredictions[cylinderIndex + 1] += bendStiffness * deltaOrientationTwo;

        cylinderOrientationPredictions[cylinderIndex].Normalize();
        cylinderOrientationPredictions[cylinderIndex + 1].Normalize();

        Assert.AreApproximatelyEqual(1f, mathHelper.QuaternionLength(cylinderOrientationPredictions[cylinderIndex]), tolerance: 0.01f);
        Assert.AreApproximatelyEqual(1f, mathHelper.QuaternionLength(cylinderOrientationPredictions[cylinderIndex + 1]), tolerance: 0.01f);
    }
}
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using BSM = BulletSharp.Math;

namespace GuidewireSim
{
/**
 * This class executes the outer simuluation loop of the algorithm and calls the implementations of each algorithm step and manages all coherent data.
 */
[RequireComponent(typeof(InitializationStep))]
[RequireComponent(typeof(MathHelper))]
public class SimulationLoop : MonoBehaviour
{
    InitializationStep initializationStep; //!< The component InitializationStep that is responsible for initializing the simulation.
    PredictionStep predictionStep; //!< The component PredictionStep that is responsible for executing the Prediction Step of the algorithm.
    ConstraintSolvingStep constraintSolvingStep; /**< The component ConstraintSolvingStep that is responsible for correcting the predictions
                                                  *   with the collision and model constraints.
                                                  */
    UpdateStep updateStep; //!< The component UpdateStep that is responsible for executing the Update Step of the algorithm.
    ObjectSetter objectSetter; //!< The component ObjectSetter that is responsible for setting all positions and rotations the the GameObjects.
    MathHelper mathHelper; //!< The component MathHelper that provides math related helper functions.
    CollisionSolvingStep collisionSolvingStep; //!< The component CollisionSolvingStep that solves all collisions.
    CollisionHandler collisionHandler; //!< The component CollisionHandler that tracks all collisions.

    [SerializeField] public GameObject[] spheres; /**< All spheres that are part of the guidewire.
                                        *   @attention The order in which the spheres are assigned matters. Assign them such that
                                        *   two adjacent spheres are adjacent in the array as well.
                                        */
    [SerializeField] GameObject[] cylinders; /**< All cylinders that are part of the guidewire.
                                              *   @attention The order in which the cylinders are assigned matters. Assign them such that
                                              *   two adjacent cylinders are adjacent in the array as well.
                                              */

    [HideInInspector] public Vector3[] spherePositions; //!< The position at the current frame of each sphere.
    [HideInInspector] public Vector3[] sphereVelocities; //!< The velocity of the current frame of each sphere. Initalized with zero entries.
    [HideInInspector] public float[] sphereInverseMasses; /**< The constant inverse masses  of each sphere.
                                                           *   @note Set to 1 for moving spheres and to 0 for fixed spheres.
                                                           */
    [HideInInspector] public Vector3[] sphereExternalForces; //!< The sum of all current external forces that are applied per particle/ sphere.
    Vector3[] spherePositionPredictions; //!< The prediction of the position at the current frame of each sphere.
    
    [HideInInspector] public Vector3[] cylinderPositions; //!< The center of mass of each cylinder.
    BSM.Quaternion[] cylinderOrientations; //!< The orientation of each cylinder at its center of mass.
    BSM.Quaternion[] cylinderOrientationPredictions; //!< The prediction of the orientation of each cylinder at its center of mass.
    Vector3[] discreteRestDarbouxVectors; /**< The discrete Darboux Vector at the rest configuration, i.e. at frame 0.
                                        *   @note It is important to only take the imaginary part in the calculation
                                        *   for the discrete Darboux Vector, thus we only save it as a Vector3.
                                        *   To use it in a quaternion setting, embedd the Vector3 with scalar part 0, i.e.
                                        *   with EmbeddedVector().
                                        *   @attention There is only CylinderCount - 1 many darboux vectors. The i-th Darboux Vector
                                        *   is between orientation i and orientation i+1.
                                        */
    Vector3[] cylinderAngularVelocities; /**< The angular velocity of the current frame of each orientation element/ cylinder.
                                        *   Initalized with zero entries.
                                        */
    [HideInInspector] public float[] cylinderScalarWeights; /**< The constant scalar weights of each orientation/ quaternion similar to #sphereInverseMasses.
                                    *   @note Set to 1 for moving orientations (so that angular motion can be applied)
                                    *   and to 0 for fixed orientations.
                                    */
    float[,] inertiaTensor; //!< The inertia tensor. Entries are approximates as in the CoRdE paper.
    float[,] inverseInertiaTensor; //!< The inverse of #inertiaTensor.
    [HideInInspector] public Vector3[] cylinderExternalTorques; //!< The sum of all current external torques that are applied per orientation element/ cylinder.
    public Vector3[][] directors; /**< The orthonormal basis of each orientation element / cylinder, also called directors.
                                   *   @note In the 0th row are the first directors of each orientation element, not in the 1th row.
                                   *   Example: The (i, j)th element holds the (i-1)th director of orientation element j.
                                   */
    BSM.Quaternion[] worldSpaceBasis; /**< The three basis vectors of the world coordinate system as embedded quaternions with scalar part 0.
                                       *   E.g. the first basis vector is (1, 0, 0), the second (0, 1, 0) and the third (0, 0, 1).
                                       */

    public bool ExecuteSingleLoopTest { get; set; } = false; /**< Whether or not to execute the Single Loop Test, in which the outer simulation loop
                                                              *   is exactly executed once. 
                                                              */
    public bool solveStretchConstraints = true; //!< Whether or not to perform the constraint solving of the stretch constraint.
    public bool solveBendTwistConstraints = true; //!< Whether or not to perform the constraint solving of the bend twist constraint.
    public bool solveCollisionConstraints = true; //!< Whether or not to perform the constraint solving of collision constraints.

    float rodElementLength = 10f; /**< The distance between two spheres, also the distance between two orientations.
                                *   Also the length of one cylinder.
                                *   @note This should be two times the radius of a sphere.
                                *   @attention Make sure that the guidewire setup fulfilles that the distance between two adjacent
                                *   spheres is #rodElementLength.
                                */

    [SerializeField] [Range(1, 1000)] int constraintSolverSteps = 1000; /**< How often the constraint solver iterates over each constraint during
                                                                                 *   the Constraint Solving Step.
                                                                                 *   @attention This value must be positive.
                                                                                 */
    public int ConstraintSolverSteps { get { return constraintSolverSteps; }
                                       set { constraintSolverSteps = value; } }
    [SerializeField] [Range(0.002f, 0.04f)] float timeStep = 0.01f; /**< The fixed time step in seconds at which the simulation runs.
                                                                      *  @note A lower timestep than 0.002 can not be guaranteed by
                                                                      *  the test hardware to be executed in time. Only choose a lower timestep if
                                                                      *  you are certain your hardware can handle it.
                                                                      */
    public float TimeStep { get { return timeStep; }}

    public int SpheresCount { get; private set; } //!< The count of all #spheres of the guidewire.
    public int CylinderCount { get; private set; } //!< The count of all #cylinders of the guidewire.

    private void Awake()
    {
        Assert.IsFalse(spheres.Length == 0);
        Assert.IsFalse(cylinders.Length == 0);
        Assert.IsTrue(spheres.Length == cylinders.Length + 1);

        initializationStep = GetComponent<InitializationStep>();
        Assert.IsNotNull(initializationStep);

        predictionStep = GetComponent<PredictionStep>();
        Assert.IsNotNull(predictionStep);

        constraintSolvingStep = GetComponent<ConstraintSolvingStep>();
        Assert.IsNotNull(constraintSolvingStep);

        updateStep = GetComponent<UpdateStep>();
        Assert.IsNotNull(updateStep);

        objectSetter = GetComponent<ObjectSetter>();
        Assert.IsNotNull(objectSetter);

        mathHelper = GetComponent<MathHelper>();
        Assert.IsNotNull(mathHelper);

        collisionSolvingStep = GetComponent<CollisionSolvingStep>();
        Assert.IsNotNull(collisionSolvingStep);

        collisionHandler = GetComponent<CollisionHandler>();
        Assert.IsNotNull(collisionHandler);
    }

    private void Start()
    {
        Time.fixedDeltaTime = timeStep;

        PerformInitializationStep();

        // SetSpherePositions(); // are already set
        objectSetter.SetCylinderPositions(cylinders, CylinderCount, cylinderPositions);
        objectSetter.SetCylinderOrientations(cylinders, CylinderCount, cylinderOrientations, directors);
    }

    /**
     * @req Execute the simulation loop if and only if #ExecuteSingleLoopTest is false.
     */
    private void FixedUpdate()
    {
        if (ExecuteSingleLoopTest) return;

        PerformSimulationLoop();
    }

    /**
     * Calls every step that is mandatory to declare and initialize all data.
     * @req Set #SpheresCount to the length of #spheres.
     * @req Set #CylinderCount to the length of #cylinders.
     * @req Call every init method of #initializationStep.
     */
    private void PerformInitializationStep()
    {
        SpheresCount = spheres.Length;
        CylinderCount = cylinders.Length;

        Assert.IsTrue(SpheresCount == CylinderCount + 1);

        initializationStep.InitSpherePositions(spheres, SpheresCount, out spherePositions);
        initializationStep.InitSphereVelocities(SpheresCount, out sphereVelocities);
        initializationStep.InitSphereInverseMasses(SpheresCount, out sphereInverseMasses);
        initializationStep.InitCylinderPositions(CylinderCount, spherePositions, out cylinderPositions);
        initializationStep.InitCylinderOrientations(CylinderCount, out cylinderOrientations);
        initializationStep.InitDiscreteRestDarbouxVectors(CylinderCount, cylinderOrientations, out discreteRestDarbouxVectors, rodElementLength);
        initializationStep.InitCylinderAngularVelocities(CylinderCount, out cylinderAngularVelocities);
        initializationStep.InitCylinderScalarWeights(CylinderCount, out cylinderScalarWeights);
        initializationStep.InitSphereExternalForces(SpheresCount, out sphereExternalForces);
        initializationStep.InitSpherePositionPredictions(spheres, SpheresCount, out spherePositionPredictions);
        initializationStep.InitInertiaTensor(out inertiaTensor);
        initializationStep.InitInverseInertiaTensor(inertiaTensor, out inverseInertiaTensor);
        initializationStep.InitCylinderExternalTorques(CylinderCount, out cylinderExternalTorques);
        initializationStep.InitCylinderOrientationPredictions(CylinderCount, out cylinderOrientationPredictions);
        initializationStep.InitWorldSpaceBasis(out worldSpaceBasis);
        initializationStep.InitDirectors(CylinderCount, worldSpaceBasis, out directors);
        initializationStep.InitSphereColliders(SpheresCount, spheres);
    }

    /**
     * Performs the outer simulation loop of the algorithm.
     * @note In a late version, CollisionDetection and GenerateCollisionConstraints will be added to the algorithm.
     */
    public void PerformSimulationLoop()
    {
        PerformConstraintSolvingStep();
        PerformUpdateStep();
        PerformPredictionStep();
        AdaptCalculations();
        SetCollidersStep();
    }

    /**
     * Performs the constraint solving step of the algorithm.
     * @req Performs the constraint solving of every constraint #solverStep many times.
     * @req Solve stretch constraints, if and only if #solveStretchConstraints is true.
     * @req Solve bend twist constraints, if and only if #solveBendTwistConstraints is true.
     * @req If #solveStretchConstraints, then #SpheresCount is at least two.
     * @req If #solveStretchConstraints, then #CylinderCount is at least one.
     * @req If #solveBendTwistConstraints, then #SpheresCount is at least three.
     * @req If #solveBendTwistConstraints, then #CylinderCount is at least two.
     * @req If #solveStretchConstraints, after the step is complete the deviation between the actual rod element length and the
     * default (rest state) #rodElementLength should be close to zero.
     * @req If #solveStretchConstraints, after the step is complete the deviation of the stretch constraint to zero should be
     * close to zero.
     * @req If #solveCollisionConstraints, after this step is complete clear the list @p registeredCollisions, since these collisions
     * are now resolved.
     */
    private void PerformConstraintSolvingStep()
    {
        if (solveStretchConstraints)
        {
            Assert.IsTrue(SpheresCount >= 2);
            Assert.IsTrue(CylinderCount >= 1);
        }
        if (solveBendTwistConstraints)
        {
            Assert.IsTrue(CylinderCount >= 2);
            Assert.IsTrue(SpheresCount >= 3);
        }

            // if (solveCollisionConstraints)
            // {
            //     collisionSolvingStep.SolveCollisionConstraints(spherePositionPredictions);
            // }

        for (int solverStep = 0; solverStep < ConstraintSolverSteps; solverStep++)
        {
            if (solveStretchConstraints)
            {
                constraintSolvingStep.SolveStretchConstraints(spherePositionPredictions, cylinderOrientationPredictions, SpheresCount,
                                                              worldSpaceBasis, rodElementLength);
            }

            if (solveBendTwistConstraints)
            {
                constraintSolvingStep.SolveBendTwistConstraints(cylinderOrientationPredictions, CylinderCount, discreteRestDarbouxVectors,
                                                                rodElementLength);
            }

            if (solveCollisionConstraints)
            {
                collisionSolvingStep.SolveCollisionConstraints(spherePositionPredictions, solverStep, ConstraintSolverSteps);
            }
        }

        if (solveCollisionConstraints)
        {
            collisionHandler.ResetRegisteredCollisions();
        }

        if (solveStretchConstraints)
        {
            // Debug.Log(mathHelper.RodElementLengthDeviation(spherePositionPredictions[0],
            //                                                 spherePositionPredictions[1],
            //                                                 rodElementLength));
            for (int sphereIndex = 0; sphereIndex < SpheresCount - 1; sphereIndex++)
            {
                // Assert.AreApproximatelyEqual(0f, mathHelper.RodElementLengthDeviation(spherePositionPredictions[sphereIndex],
                //                                                                       spherePositionPredictions[sphereIndex + 1],
                //                                                                       rodElementLength), tolerance: 0.05f);
                // Assert.AreApproximatelyEqual(0f, mathHelper.StretchConstraintDeviation(spherePositionPredictions[sphereIndex],
                //                                                                        spherePositionPredictions[sphereIndex + 1],
                //                                                                        cylinderOrientationPredictions[sphereIndex],
                //                                                                        worldSpaceBasis[2],
                //                                                                        rodElementLength), tolerance: 0.1f);
            }
        }
    }

    /**
     * Performs the update step of the algorithm.
     * @req Upate #sphereVelocities.
     * @req Upate #spherePositions.
     * @req Upate #cylinderAngularVelocities.
     * @req Upate #cylinderOrientations.
     * @req Upate #directors.
     */
    private void PerformUpdateStep()
    {
        sphereVelocities = updateStep.UpdateSphereVelocities(sphereVelocities, SpheresCount, spherePositionPredictions, spherePositions);
        spherePositions = updateStep.UpdateSpherePositions(spherePositions, SpheresCount, spherePositionPredictions);
        cylinderAngularVelocities = updateStep.UpdateCylinderAngularVelocities(cylinderAngularVelocities, CylinderCount, cylinderOrientations,
                                                                               cylinderOrientationPredictions);
        cylinderOrientations = updateStep.UpdateCylinderOrientations(cylinderOrientations, CylinderCount, cylinderOrientationPredictions);
        directors = mathHelper.UpdateDirectors(CylinderCount, cylinderOrientations, directors, worldSpaceBasis);
    }

    /**
        * Performs the prediction step of the algorithm.
        * @req Predict the #sphereVelocities.
        * @req Predict the #spherePositionPredictions.
        * @req Predict the #cylinderAngularVelocities.
        * @req Predict the #cylinderOrientationPredictions.
        */
    private void PerformPredictionStep()
    {
        sphereVelocities = predictionStep.PredictSphereVelocities(sphereVelocities, sphereInverseMasses, sphereExternalForces);
        spherePositionPredictions = predictionStep.PredictSpherePositions(spherePositionPredictions, SpheresCount, spherePositions, sphereVelocities, sphereExternalForces, sphereInverseMasses);
        cylinderAngularVelocities = predictionStep.PredictAngularVelocities(cylinderAngularVelocities, CylinderCount, inertiaTensor,
                                                                            cylinderExternalTorques, inverseInertiaTensor);
        cylinderOrientationPredictions = predictionStep.PredictCylinderOrientations(cylinderOrientationPredictions, CylinderCount,
                                                                                    cylinderAngularVelocities, cylinderOrientations);

        if (ExecuteSingleLoopTest) Debug.Log("The distance between both sphere position predictions is "
                                             + Vector3.Distance(spherePositionPredictions[0], spherePositionPredictions[1]));
    }

    /**
     * Adapts the data to the Unity GameObjects. For example, sets the positions of the GameObjects #spheres to #spherePositions.
     * @req Sets the positions of the GameObjects #spheres to #spherePositions.
     * @req Calculates #cylinderPositions based on #spherePositions.
     * @req Sets the positions of the GameObjects #cylinders to #cylinderPositions.
     * @req Sets the rotations of the GameObjects #cylinders to #cylinderOrientations.
     */
    private void AdaptCalculations()
    {
        objectSetter.SetSpherePositions(spheres, SpheresCount, spherePositions);
        mathHelper.CalculateCylinderPositions(CylinderCount, spherePositions, cylinderPositions);
        objectSetter.SetCylinderPositions(cylinders, CylinderCount, cylinderPositions);
        objectSetter.SetCylinderOrientations(cylinders, CylinderCount, cylinderOrientations, directors);
    }
    
    /**
     * Sets the position of the collider of each sphere of the guidewire to the sphere's position prediciton.
     */
    private void SetCollidersStep()
    {
        collisionHandler.SetCollidersToPredictions(SpheresCount, spherePositionPredictions, spherePositions);
    }
}
}
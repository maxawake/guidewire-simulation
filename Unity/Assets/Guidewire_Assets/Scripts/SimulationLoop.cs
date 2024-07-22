using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using BSM = BulletSharp.Math;
using System.IO;
using System.Diagnostics;

namespace GuidewireSim
{
    /**
     * This class executes the outer simuluation loop of the algorithm and calls the implementations of each algorithm step and manages all coherent data.
     */
    // TODO: Do we need these RequireComponent attributes?
    [RequireComponent(typeof(InitializationStep))]
    [RequireComponent(typeof(MathHelper))]
    public class SimulationLoop : MonoBehaviour
    {
        private Stopwatch stopwatch;
        //public Camera followingCamera;  // Drag your camera here in the Unity Editor
        //private Vector3 cameraOffset = new Vector3(0, 781, 0); // The offset of the camera in the y direction is 781

        // OWN STUFF:
        ParameterHandler parameterHandler; // The parameter handler
        DataLogger logger;
        CommandLineHandler cli;

        private Vector3 spherePositionInitial; /**< The initial position of the sphere at the beginning of the Constraint Solving Step.
                                        *   @note This is needed to calculate the deviation of the rod element length to the default
                                        *   rod element length.
                                        */
        public bool Screenshots = false; /**< Whether or not to take screenshots of the simulation. */
        // OWN STUFF:

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
        CylinderCollisionHandler cylinderCollisionHandler; //!< The component CylinderCollisionHandler that manages all cylinder collisions.

        [SerializeField] public GameObject[] spheres; /**< All spheres that are part of the guidewire.
                                *   @attention The order in which the spheres are assigned matters. Assign them such that
                                *   two adjacent spheres are adjacent in the array as well.
                                */
        [SerializeField] public GameObject[] cylinders; /**< All cylinders that are part of the guidewire.
                                        *   @attention The order in which the cylinders are assigned matters. Assign them such that
                                        *   two adjacent cylinders are adjacent in the array as well.
                                        */

        public Vector3[] spherePositions; //!< The position at the current frame of each sphere.
        public Vector3[] sphereVelocities; //!< The velocity of the current frame of each sphere. Initalized with zero entries.
        public float[] sphereInverseMasses; // The constant inverse masses  of each sphere.

        public Vector3[] spherePositionsTemp;
        public Vector3[] sphereExternalForces; //!< The sum of all current external forces that are applied per particle/ sphere.
        public Vector3[] spherePositionPredictions; //!< The prediction of the position at the current frame of each sphere.

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

        private float rodElementLength; /**< The distance between two spheres, also the distance between two orientations.
                                    *   Also the length of one cylinder.
                                    *   @note This should be two times the radius of a sphere.
                                    *   @attention Make sure that the guidewire setup fulfilles that the distance between two adjacent
                                    *   spheres is #rodElementLength.
                                    */

        private int constraintSolverSteps;/** = 1000; /**< How often the constraint solver iterates over each constraint during
                                                                                 *   the Constraint Solving Step.
                                                                                 *   @attention This value must be positive.
                                                                                 */
        public int ConstraintSolverSteps
        {
            get { return constraintSolverSteps; }
            set { constraintSolverSteps = value; }
        }

        private float timeStep;/** = 0.01f; /**< The fixed time step in seconds at which the simulation runs.
                                                                      *  @note A lower timestep than 0.002 can not be guaranteed by
                                                                      *  the test hardware to be executed in time. Only choose a lower timestep if
                                                                      *  you are certain your hardware can handle it.
                                                                      */
        public float TimeStep { get { return timeStep; } }

        public int spheresCount { get; private set; } //!< The count of all #spheres of the guidewire.
        public int CylinderCount { get; private set; } //!< The count of all #cylinders of the guidewire.

        public bool Logging = true; /**< Whether or not to log the positions of the spheres. */

        private float totalTime = 0.0f; /**< The total time the simulation has been running. */
        public bool readFile = true; /**< Whether or not to use a file to read the parameters from. */
        private int simulationStep = 0; /**< The current simulation step. */
        
        public float deltaThreshold = 0.1f;

        /**
        * Default constructor.
        */
        private void Awake()
        {
            parameterHandler = GetComponent<ParameterHandler>();
            Assert.IsNotNull(parameterHandler);

            cli = GetComponent<CommandLineHandler>();
            Assert.IsNotNull(cli);

            GetParameterHandler();

            // create screenshot folder if not exits
            if (Screenshots)
            {
                string screenshotPath = parameterHandler.logFilePath + "screenshots/";
                if (!Directory.Exists(screenshotPath))
                {
                    Directory.CreateDirectory(screenshotPath);
                }
            }

            // Get the parameters from the parameter handler
            timeStep = parameterHandler.timeStep;
            constraintSolverSteps = parameterHandler.constraintSolverSteps;

            stopwatch = new Stopwatch();
            //GetGuidewireFromScene();

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

            cylinderCollisionHandler = GetComponent<CylinderCollisionHandler>();
            Assert.IsNotNull(cylinderCollisionHandler);

            logger = GetComponent<DataLogger>();
            Assert.IsNotNull(logger);

            cli = GetComponent<CommandLineHandler>();
            Assert.IsNotNull(cli);
        }

        /**
         * Initializes the simulation loop.
         */
        private void Start()
        {
            Time.fixedDeltaTime = timeStep;

            PerformInitializationStep();

            objectSetter.SetCylinderPositions(cylinders, CylinderCount, cylinderPositions);
            objectSetter.SetCylinderOrientations(cylinders, CylinderCount, cylinderOrientations, directors);

            // Save the initial positions
            if (Logging)
            {
                PerformLogging(false);
                simulationStep++;
            }

            PerformOffsetting();


        }

        /**
         * @req Execute the simulation loop if and only if #ExecuteSingleLoopTest is false.
         */
        private void Update()
        {
            if (ExecuteSingleLoopTest) return;

            CopySpherePositions(spheresCount, spherePositionPredictions, out spherePositionsTemp);

            stopwatch.Restart();
            PerformSimulationLoop();
            stopwatch.Stop();

            //Debug.Log("Delta: " + delta);
            // if (delta < 0.01f) {
            //     Debug.Log("Delta is less than 0.01");
            //     Vector3 direction = spherePositions[1] - spherePositions[0];
            //     direction.Normalize();
            //     spherePositions[0] = spherePositions[0] + parameterHandler.displacement*direction; // Used zDisplacement here
            // }

            // Save state to log file
            if (Logging)
            {
                PerformLogging();
            }

            totalTime += Time.fixedDeltaTime;
            simulationStep++;

            if (Input.GetKey("escape"))
            {
                Quit();
            }
        }

        /**
         * Performs the logging of the positions of the spheres.
         */
        private void PerformLogging(bool insideLoop = true)
        {
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            float delta = CalculateDelta(spherePositionsTemp, spherePositionPredictions);
            logger.savePositionsAsJson(spherePositions, sphereVelocities, spheresCount, simulationStep, totalTime, elapsedMilliseconds, delta);

            // Take screenshot
            if (Screenshots && simulationStep % 100 == 0)
            {
                string screenshotPath = parameterHandler.logFilePath + "screenshots/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".jpg";
                ScreenCapture.CaptureScreenshot(screenshotPath);
            }

            //if (simulationStep > 1000) {
            if (delta < parameterHandler.deltaThreshold && insideLoop)
            {
                UnityEngine.Debug.Log("Simulation step: " + simulationStep);
                Quit();
            }
        }

        /**
         * Performs the offsetting of the guidewire.
         */
        private void PerformOffsetting()
        {
            Vector3 direction = spherePositions[1] - spherePositions[0];
            direction.Normalize();
            spherePositionPredictions[0] = spherePositions[0] + parameterHandler.displacement * direction;
        }


        private void PerformInitializationStep()
        {
            Assert.IsTrue(spheresCount == CylinderCount + 1);

            initializationStep.InitSpherePositions(spheres, spheresCount, out spherePositions);
            initializationStep.InitSphereVelocities(spheresCount, out sphereVelocities);
            initializationStep.InitSphereInverseMasses(spheresCount, out sphereInverseMasses);
            initializationStep.InitCylinderPositions(CylinderCount, spherePositions, out cylinderPositions);
            initializationStep.InitCylinderOrientations(CylinderCount, out cylinderOrientations);
            initializationStep.InitDiscreteRestDarbouxVectors(CylinderCount, cylinderOrientations, out discreteRestDarbouxVectors, rodElementLength);
            initializationStep.InitCylinderAngularVelocities(CylinderCount, out cylinderAngularVelocities);
            initializationStep.InitCylinderScalarWeights(CylinderCount, out cylinderScalarWeights);
            initializationStep.InitSphereExternalForces(spheresCount, out sphereExternalForces);
            initializationStep.InitSpherePositionPredictions(spheres, spheresCount, out spherePositionPredictions);
            initializationStep.InitInertiaTensor(out inertiaTensor);
            initializationStep.InitInverseInertiaTensor(inertiaTensor, out inverseInertiaTensor);
            initializationStep.InitCylinderExternalTorques(CylinderCount, out cylinderExternalTorques);
            initializationStep.InitCylinderOrientationPredictions(CylinderCount, out cylinderOrientationPredictions);
            initializationStep.InitWorldSpaceBasis(out worldSpaceBasis);
            initializationStep.InitDirectors(CylinderCount, worldSpaceBasis, out directors);
            initializationStep.InitSphereColliders(spheresCount, spheres);
        }

        /*
         Performs the outer simulation loop of the algorithm.
         @note In a late version, CollisionDetection and GenerateCollisionConstraints will be added to the algorithm.
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
         * @req If #solveStretchConstraints, then #spheresCount is at least two.
         * @req If #solveStretchConstraints, then #CylinderCount is at least one.
         * @req If #solveBendTwistConstraints, then #spheresCount is at least three.
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
                Assert.IsTrue(spheresCount >= 2);
                Assert.IsTrue(CylinderCount >= 1);
            }

            if (solveBendTwistConstraints)
            {
                Assert.IsTrue(CylinderCount >= 2);
                Assert.IsTrue(spheresCount >= 3);
            }

            for (int solverStep = 0; solverStep < ConstraintSolverSteps; solverStep++)
            {
                //logger.write($"Start of Constraint Solving - Last sphere position: {spherePositionPredictions[0]}"); 
                spherePositionInitial = spherePositionPredictions[0];

                if (solveStretchConstraints)
                {
                    constraintSolvingStep.SolveStretchConstraints(spherePositionPredictions, cylinderOrientationPredictions, spheresCount,
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

                // Save the last sphere position
                spherePositionPredictions[0] = spherePositionInitial;
            }

            if (solveCollisionConstraints)
            {
                collisionHandler.ResetRegisteredCollisions();
                cylinderCollisionHandler.ResetRegisteredCollisions();
            }
        }

        /**
         * Copies the sphere positions from the spherePositionPredictions to the spherePositions.
         * @param spheresCount The count of all spheres of the guidewire.
         * @param spherePositionPredictions The prediction of the position at the current frame of each sphere.
         * @param spherePositions The position at the current frame of each sphere.
         */
        public void CopySpherePositions(int spheresCount, Vector3[] spherePositionPredictions, out Vector3[] spherePositions)
        {
            spherePositions = new Vector3[spheresCount];
            for (int sphereIndex = 0; sphereIndex < spheresCount; sphereIndex++)
            {
                spherePositions[sphereIndex] = spherePositionPredictions[sphereIndex];
            }
            //return spherePositions;
        }

        /**
         * Calculates the delta between two sets of sphere positions.
         * @param spherePositions1 The first set of sphere positions.
         * @param spherePositions2 The second set of sphere positions.
         * @return The delta between the two sets of sphere positions.
         */
        private float CalculateDelta(Vector3[] spherePositions1, Vector3[] spherePositions2)
        {
            float delta = 0.0f;
            for (int i = 1; i < spherePositions1.Length; i++)
            {
                delta += Vector3.Distance(spherePositions1[i], spherePositions2[i]);
            }
            return delta;
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
            sphereVelocities = updateStep.UpdateSphereVelocities(sphereVelocities, spheresCount, spherePositionPredictions, spherePositions);
            spherePositions = updateStep.UpdateSpherePositions(spherePositions, spheresCount, spherePositionPredictions);
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
            spherePositionPredictions = predictionStep.PredictSpherePositions(spherePositionPredictions, spheresCount, spherePositions, sphereVelocities, sphereInverseMasses, sphereExternalForces);
            sphereVelocities = predictionStep.PredictSphereVelocities(sphereVelocities, sphereInverseMasses, sphereExternalForces);
            cylinderAngularVelocities = predictionStep.PredictAngularVelocities(cylinderAngularVelocities, CylinderCount, inertiaTensor,
            cylinderExternalTorques, inverseInertiaTensor);
            cylinderOrientationPredictions = predictionStep.PredictCylinderOrientations(cylinderOrientationPredictions, CylinderCount,
            cylinderAngularVelocities, cylinderOrientations);
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
            objectSetter.SetSpherePositions(spheres, spheresCount, spherePositions);
            mathHelper.CalculateCylinderPositions(CylinderCount, spherePositions, cylinderPositions);
            objectSetter.SetCylinderPositions(cylinders, CylinderCount, cylinderPositions);
            objectSetter.SetCylinderOrientations(cylinders, CylinderCount, cylinderOrientations, directors);
        }

        /**
         * Sets the position of the collider of each sphere of the guidewire to the sphere's position prediciton.
         */
        private void SetCollidersStep()
        {
            collisionHandler.SetCollidersToPredictions(spheresCount, spherePositionPredictions, spherePositions);
        }

        /**
         * Sets the spheres of the guidewire.
         * @param spheresArray The spheres of the guidewire.
         */
        public void SetSpheres(GameObject[] spheresArray)
        {
            spheres = spheresArray;
            spheresCount = spheres.Length;
        }

        /**
         * Sets the cylinders of the guidewire.
         * @param cylindersArray The cylinders of the guidewire.
         */
        public void SetCylinders(GameObject[] cylindersArray)
        {
            cylinders = cylindersArray;
            CylinderCount = cylinders.Length;
        }

        /**
         * Get the guidewire from the scene
        */
        public void GetGuidewireFromScene()
        {
            // Get the guidewire from the scene
            GameObject guidewire = GameObject.Find("Guidewire");
            Transform parentTransform = guidewire.transform;

            // Use lists because we dont know how many spheres beforehand
            List<GameObject> spheresList = new List<GameObject>();
            List<GameObject> cylindersList = new List<GameObject>();

            // Loop through each child GameObject
            for (int i = 0; i < parentTransform.childCount; i++)
            {
                // Get the child GameObject at index i
                GameObject childObject = parentTransform.GetChild(i).gameObject;

                // Add to spheres or cylinder list, depending on name
                if (childObject.name.Contains("Sphere"))
                {
                    spheresList.Add(childObject);
                }

                if (childObject.name.Contains("Cylinder"))
                {
                    cylindersList.Add(childObject);
                }
            }

            // Add them to the class variables
            SetSpheres(spheresList.ToArray());
            SetCylinders(cylindersList.ToArray());
        }

        /**
     * Saves the current positions of the spheres to the log file.
     */
        // public void SaveCurrentPositions()
        // {
        //     for (int i = 0; i < spheres.Length; i++)
        //     {
        //         Vector3 spherePosition = spheres[i].transform.position;
        //         logger.write("Sphere " + (i + 1) + " Position: " + spherePosition.x + "," + spherePosition.y + "," + spherePosition.z);
        //     }
        // }

        /**
         * Updates the camera position to the last sphere.
         */
        private void UpdateCameraPosition()
        {
            if (spheres != null && spheres.Length > 0)
            {
                GameObject lastSphere = spheres[spheres.Length - 1];
                //Vector3 newCameraPosition = lastSphere.transform.position + cameraOffset;
                //followingCamera.transform.position = newCameraPosition;
            }
        }

        private void GetParameterHandler()
        {
            if (readFile)
            {
                UnityEngine.Debug.Log("Read the parameters from a file");
                string saveFile = cli.GetArg("-parameters");
                UnityEngine.Debug.Log("Save file: " + saveFile);
                if (File.Exists(saveFile))
                {
                    string jsonString = File.ReadAllText(saveFile);
                    parameterHandler.CreateFromJSON(jsonString);
                }
            }
            else
            {
                UnityEngine.Debug.Log("Write the parameters to a file");
                string saveFile = parameterHandler.logFilePath + "parameters.json";
                string jsonString = parameterHandler.SaveToString();
                File.WriteAllText(saveFile, jsonString);
            }

            // print the parameters
            parameterHandler.printParameters();
        }

        /**
        * Returns the rod element length.
        */
        public float GetRodElementLength()
        {
            return rodElementLength;
        }

        public void SetRodElementLength(float rodElementLength)
        {
            this.rodElementLength = rodElementLength;
        }

        public void Quit()
        {
#if UNITY_STANDALONE
            Application.Quit();
#endif
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
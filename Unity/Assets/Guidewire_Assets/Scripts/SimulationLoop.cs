using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using BSM = BulletSharp.Math;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace GuidewireSim
{
    
    public class SimulationLoop : MonoBehaviour
    {

    		private bool isFirstCall = true; 
    		//public int count; //hier aus CreationScript
    		//CreationScript creationScript; //hier -""-
    		private Stopwatch stopwatch;
    		public Camera followingCamera;  // Drag your camera here in the Unity Editor
    		private Vector3 cameraOffset = new Vector3(0, 781, 0); // The offset of the camera in the y direction is 781
    		private string logFilePath = "";

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
    		public float[] sphereInverseMasses; /**< The constant inverse masses  of each sphere.
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

    public float GetRodElementLength(){
    	return rodElementLength;
    }   
    float rodElementLength =10f;




//The distance between two spheres, also the distance between two orientations.

     // The distance between two spheres, also the distance between two orientations.
                                //  Also the length of one cylinder.
                                //  @note This should be two times the radius of a sphere.
                               //  @attention Make sure that the guidewire setup fulfilles that the distance between two adjacent
                                //   spheres is #rodElementLength.
                                

    [SerializeField] [Range(1, 1000)] int constraintSolverSteps = 1000; /**< How often the constraint solver iterates over each constraint during
                                                                                 *   the Constraint Solving Step.
                                                                                 *   @attention This value must be positive.
                                                                                 */
    public int ConstraintSolverSteps { get { return constraintSolverSteps; }
                                       set { constraintSolverSteps = value; } }
    private float timeStep;   /**< The fixed time step in seconds at which the simulation runs.
                                                                      *  @note A lower timestep than 0.002 can not be guaranteed by
                                                                      *  the test hardware to be executed in time. Only choose a lower timestep if
                                                                      *  you are certain your hardware can handle it.
                                                                      */
    public float TimeStep { get { return timeStep; }}

    public int SpheresCount { get; private set; } //!< The count of all #spheres of the guidewire.
    public int CylinderCount { get; private set; } //!< The count of all #cylinders of the guidewire.
    
    private void Awake()
    {        

    	//GameObject creationObject = GameObject.Find("GameObjectCreationScript");    //hier aus creation script, alle 3 zeilen
    	//creationScript = creationObject.GetComponent<CreationScript>();
    	//int count = creationScript.spheresCount;



    	

    	string[] args = System.Environment.GetCommandLineArgs();
    	for (int i = 0; i < args.Length; i++)
    	{
        	if (args[i] == "-logFilePath" && args.Length > i + 1)
        	{
            		logFilePath = args[i + 1];
        	}
        	else if (args[i] == "-timeStep" && args.Length > i + 1)
        	{
           		if (float.TryParse(args[i + 1], out float parsedTimeStep))
            		{
                		timeStep = parsedTimeStep;
            		}
            		else
            		{
                		Debug.LogError("Failed to parse timeStep from command-line arguments. Using default value.");
                		timeStep = 0.01f;
            		}
        	}
    	}
    	
    	for (int i = 0; i < args.Length; i++)
    	{
        
        	if (args[i] == "-rodElementLength" && args.Length > i + 1)
        	{
            		if (float.TryParse(args[i + 1], out float parsedRodElementLength))
            		{
                		rodElementLength = parsedRodElementLength;
            		}
            		else
            		{
                		Debug.LogError("Failed to parse rodElementLength from command-line arguments. Using default value.");
                		rodElementLength = 50f;
            		}
       	}
    	}            
            stopwatch = new Stopwatch();
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

            // Get command line arguments
            //string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-logFilePath" && args.Length > i + 1)
                {
                    logFilePath = args[i + 1];
                }
            }
        }

        private void Start()
        {
            Time.fixedDeltaTime = timeStep;

            PerformInitializationStep();
            //InvokeRepeating("SavePositionsToFile", 0f, 0.01f);

            objectSetter.SetCylinderPositions(cylinders, CylinderCount, cylinderPositions);
            objectSetter.SetCylinderOrientations(cylinders, CylinderCount, cylinderOrientations, directors);
        }

     
        
       

//private static int fixedUpdateCounter = 0;

private void FixedUpdate()
{
    stopwatch.Restart();
    SavePositionsToFile();
    if (ExecuteSingleLoopTest) return;

    PerformSimulationLoop();
    UpdateCameraPosition();

   
  

    stopwatch.Stop();
    long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
    UnityEngine.Debug.Log($"FixedUpdate took {elapsedMilliseconds} ms");
    
    string filePath = "/home/akreibich/TestRobinCode37/TimeCalculationsFixedUpdate.txt";
    using (StreamWriter writer = new StreamWriter(filePath, true))
    {
        //writer.WriteLine($"FixedUpdate took {elapsedMilliseconds} ms");
    }
    
    string filePath2 = "/home/akreibich/TestRobinCode37/LogConstraints.txt";
    using (StreamWriter writer = new StreamWriter(filePath2, true))
    {
        //writer.WriteLine($"FixedUpdate took place");
    }      
}

public void SavePositionsToFile()
{
    string logFilePath1 = $"/home/akreibich/TestRobinCode37/Position#All.txt";
    using (StreamWriter writer = new StreamWriter(logFilePath1, true))
    {
        for (int i = 0; i < spheres.Length; i++)
        {
            Vector3 spherePosition = spheres[i].transform.position;
            writer.WriteLine("Sphere " + (i + 1) + " Position: " + spherePosition.x + "," + spherePosition.y + "," + spherePosition.z);
        }
    }
    //fixedUpdateCounter++; // Increment the counter for the next file name
}

        private void UpdateCameraPosition()
    	{
        	if (spheres != null && spheres.Length > 0)
        	{
            		GameObject lastSphere = spheres[spheres.Length-1];
            		Vector3 newCameraPosition = lastSphere.transform.position + cameraOffset;
            		followingCamera.transform.position = newCameraPosition;
        	}
    	}


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

        public void PerformSimulationLoop()
        {
            PerformConstraintSolvingStep();
            PerformUpdateStep();
            PerformPredictionStep();
            AdaptCalculations();
            SetCollidersStep();
    	    if (solveStretchConstraints)
    	    {
        	//List<Vector3> allDeltaPositionOnes = constraintSolvingStep.GetAllDeltaPositionOnes();
       	//List<Vector3> allDeltaPositionTwos = constraintSolvingStep.GetAllDeltaPositionTwos();
       	//List<BSM.Quaternion> allDeltaOrientations = constraintSolvingStep.GetAllDeltaOrientations();
        	//Debug.Log($"All Stretch Corrections - DeltaPositionOnes: {string.Join(", ", allDeltaPositionOnes)}, DeltaPositionTwos: {string.Join(", ", allDeltaPositionTwos)}, DeltaOrientations: {string.Join(", ", allDeltaOrientations)}");
    	    }
    	 }



        

        private void PerformConstraintSolvingStep()
        {
            if (solveStretchConstraints)
            {
                Assert.IsTrue(SpheresCount >= 2);
                Assert.IsTrue(CylinderCount >= 1);
            }

            if (solveBendTwistConstraints)
            {
                Assert.IsTrue(SpheresCount >= 3);
                Assert.IsTrue(CylinderCount >= 2);
            }

            for (int solverStep = 0; solverStep < ConstraintSolverSteps; solverStep++)
            {
                
                
                string filePath = "/home/akreibich/TestRobinCode37/LastSphereConstraintPositions";
                using (StreamWriter writer = new StreamWriter(filePath, true))  // 'true' parameter appends to the file
    		{
        		//writer.WriteLine($"Start of Constraint Solving - Last sphere position: {spherePositionPredictions[0]}");
    		}
    		Vector3 initialLastSpherePosition = spherePositionPredictions[0];
                if (solveStretchConstraints)
                {
                    constraintSolvingStep.SolveStretchConstraints(spherePositionPredictions, cylinderOrientationPredictions, SpheresCount, worldSpaceBasis, rodElementLength);
                }

                if (solveBendTwistConstraints)
                {
                    constraintSolvingStep.SolveBendTwistConstraints(cylinderOrientationPredictions, CylinderCount, discreteRestDarbouxVectors, rodElementLength);
                }

                if (solveCollisionConstraints)
                {
                    collisionSolvingStep.SolveCollisionConstraints(spherePositionPredictions, solverStep, ConstraintSolverSteps);
                }
                if (isFirstCall)
        		{
            			spherePositionPredictions[0] = initialLastSpherePosition;
            			using (StreamWriter writer = new StreamWriter(filePath, true))  // 'true' parameter appends to the file
        			{
            				//writer.WriteLine($"FirstCall");
        			}
            			
        		}
        
               using (StreamWriter writer = new StreamWriter(filePath, true))  // 'true' parameter appends to the file
        		{
            			//writer.WriteLine($"End of Constraint Solving - Last sphere position: {spherePositionPredictions[0]}");
        		}
    			}

    			// Set flag to false after the first call
    	       if (isFirstCall)
    			{
        			isFirstCall = true;
    			}
             
            	

            if (solveCollisionConstraints)
            {
                collisionHandler.ResetRegisteredCollisions();
            }
       }

        private void PerformUpdateStep()
        {
            sphereVelocities = updateStep.UpdateSphereVelocities(sphereVelocities, SpheresCount, spherePositionPredictions, spherePositions);
            spherePositions = updateStep.UpdateSpherePositions(spherePositions, SpheresCount, spherePositionPredictions);
            cylinderAngularVelocities = updateStep.UpdateCylinderAngularVelocities(cylinderAngularVelocities, CylinderCount, cylinderOrientations, cylinderOrientationPredictions);
            cylinderOrientations = updateStep.UpdateCylinderOrientations(cylinderOrientations, CylinderCount, cylinderOrientationPredictions);
            directors = mathHelper.UpdateDirectors(CylinderCount, cylinderOrientations, directors, worldSpaceBasis);
        }

        private void PerformPredictionStep()
        {
            sphereVelocities = predictionStep.PredictSphereVelocities(sphereVelocities, sphereInverseMasses, sphereExternalForces);
            spherePositionPredictions = predictionStep.PredictSpherePositions(spherePositionPredictions, SpheresCount, spherePositions, sphereVelocities);
            cylinderAngularVelocities = predictionStep.PredictAngularVelocities(cylinderAngularVelocities, CylinderCount, inertiaTensor, cylinderExternalTorques, inverseInertiaTensor);
            cylinderOrientationPredictions = predictionStep.PredictCylinderOrientations(cylinderOrientationPredictions, CylinderCount, cylinderAngularVelocities, cylinderOrientations);
        }

        private void AdaptCalculations()
        {
            objectSetter.SetSpherePositions(spheres, SpheresCount, spherePositions);
            mathHelper.CalculateCylinderPositions(CylinderCount, spherePositions, cylinderPositions);
            objectSetter.SetCylinderPositions(cylinders, CylinderCount, cylinderPositions);
            objectSetter.SetCylinderOrientations(cylinders, CylinderCount, cylinderOrientations, directors);
        }

        private void SetCollidersStep()
        {
            collisionHandler.SetCollidersToPredictions(SpheresCount, spherePositionPredictions, spherePositions);
        }

        public void SetSpheres(GameObject[] spheresArray)
        {
            spheres = spheresArray;
        }

        public void SetCylinders(GameObject[] cylindersArray)
        {
            cylinders = cylindersArray;
        }
    }
}


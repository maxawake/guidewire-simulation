using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GuidewireSim {
    [Serializable]
    public class ParameterHandler: MonoBehaviour
    {   
        public string logFilePath = "/home/max/Temp/Praktikum/experiments/";

        public int constraintSolverSteps = 100; /**< How often the constraint solver iterates over each constraint during
                                                                                 *   the Constraint Solving Step.
                                                                                 *   @attention This value must be positive.
                                                                                 */
        public float timeStep = 0.1f; /**< The fixed time step in seconds at which the simulation runs.
                                                                      *  @note A lower timestep than 0.002 can not be guaranteed by
                                                                      *  the test hardware to be executed in time. Only choose a lower timestep if
                                                                      *  you are certain your hardware can handle it.
                                                                      */

        // For the rod
        public float displacement = 0.0f;
        //public float rodElementLength = 10f;
        public float guidewireLength = 100f;
        public int numberRodElements = 10;
        public float totalMass = 100.0f;
        public float guidewireOffset = -200.0f;
        
        // For model placement (not used right now)
        // public Vector3 position = Vector3.zero;
		// public Vector3 scale = Vector3.one;
		// public Vector3 rotation = Vector3.zero;

        // Constraint solving step
        public float bendStiffness = 1.0f;
        public float stretchStiffness = 1.0f;

        // Collision solving step
        public float sphereRadius = 5.0f;
        public float collisionMargin = 5.0f;
        public float collisionStiffness = 0.001f;

        public float deltaThreshold = 0.001f; /**< The theshold for the delta criterion. */
        public float damping = 0.01f; /**< The damping factor for the Verlet integration. */

        public bool VerletIntegration = true; /**< If true, Verlet integration is used for the prediction step. If false, Euler integration is used. */
        public bool SteadyState = false; /**< If true, the simulation runs until the steady state is reached. */

        public string SaveToString()
        {
            return JsonUtility.ToJson(this);
        }

        public void CreateFromJSON(string jsonString)
        {
            JsonUtility.FromJsonOverwrite(jsonString, this);
        }

        public void printParameters() 
        {
            Debug.Log("logFilePath: " + logFilePath);
            Debug.Log("constraintSolverSteps: " + constraintSolverSteps);
            Debug.Log("timeStep: " + timeStep);
            Debug.Log("displacement: " + displacement);
            //Debug.Log("rodElementLength: " + rodElementLength);
            Debug.Log("guidewireLength: " + guidewireLength);
            Debug.Log("numberRodElements: " + numberRodElements);
            Debug.Log("totalMass: " + totalMass);
            Debug.Log("guidewireOffset: " + guidewireOffset);
            // Debug.Log("position: " + position);
            // Debug.Log("scale: " + scale);
            // Debug.Log("rotation: " + rotation);
            Debug.Log("bendStiffness: " + bendStiffness);
            Debug.Log("stretchStiffness: " + stretchStiffness);
            Debug.Log("sphereRadius: " + sphereRadius);
            Debug.Log("collisionMargin: " + collisionMargin);
            Debug.Log("collisionStiffness: " + collisionStiffness);
            Debug.Log("deltaThreshold: " + deltaThreshold);
            Debug.Log("damping: " + damping);
            Debug.Log("VerletIntegration: " + VerletIntegration);
            Debug.Log("SteadyState: " + SteadyState);
        }
    }
}
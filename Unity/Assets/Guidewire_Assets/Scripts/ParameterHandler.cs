using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GuidewireSim {
    [Serializable]
    public class ParameterHandler: MonoBehaviour
    {   
        public string logFilePath = "/home/max/Temp/Praktikum/guidewire-log.txt";

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
        public float zDisplacement = 0.0f;
        public float rodElementLength = 10f;
        
        // For model placement
        public Vector3 position = Vector3.zero;
		public Vector3 scale = Vector3.one;
		public Vector3 rotation = Vector3.zero;

        // Constraint solving step
        public float bendStiffness = 1.0f;
        public float stretchStiffness = 1.0f;

        // Collision solving step
        public float sphereRadius = 5.0f;
        public float collisionMargin = 5.0f;
        public float collisionStiffness = 0.001f;

        public string SaveToString()
        {
            return JsonUtility.ToJson(this);
        }

        public void CreateFromJSON(string jsonString)
        {
            JsonUtility.FromJsonOverwrite(jsonString, this);
        }

        public float GetRodElementLength()
        {
            return rodElementLength;
        }
    }
}
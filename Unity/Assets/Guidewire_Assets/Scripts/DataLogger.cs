using System;
using UnityEngine;
using System.IO;
using UnityEngine.Assertions;

namespace GuidewireSim
{
    /**
     * This class manages all collisions that should be resolved, i.e. the collisions of the last frame.
     */
    [Serializable]
    public class DataLogger : MonoBehaviour
    {
        ParameterHandler parameterHandler;
        string jsonPath;
        string json = "{}";

        private void Awake()
        {
            Debug.Log("Logger Awake");

            parameterHandler = GetComponent<ParameterHandler>();
            Assert.IsNotNull(parameterHandler);
        }

        private void Start()
        {
            Debug.Log("Logger Start");
        }

        /**
         * Adds a new entry to the json file. Explicitely as string because the JSON utility from Unity sucks.
         * @param spherePositions The position of each sphere.
         * @param sphereVelocities The velocity of each sphere.
         * @param spheresCount The count of all spheres of the guidewire.
         * @param totalTime The total time of the simulation.
         * @return The json string that should be added to the json file.
         */
        private string AddEntry(Vector3[] spherePositions, Vector3[] sphereVelocities, int spheresCount, int simulationStep, float totalTime, float elapsedMilliseconds, float delta)
        {
            string json = "";
            json += "\"" + simulationStep + "\": {";
            json += "\"totalTime\":" + totalTime + ",";
            json += "\"elapsedMilliseconds\":" + elapsedMilliseconds + ",";
            json += "\"delta\":" + delta + ",";
            json += "\"sphere\": {";
            for (int i = 0; i < spheresCount; i++)
            {
                json += "\"" + i + "\": {";

                json += "\"x\":" + spherePositions[i].x + ",";
                json += "\"y\":" + spherePositions[i].y + ",";
                json += "\"z\":" + spherePositions[i].z + ",";

                json += "\"vx\":" + sphereVelocities[i].x + ",";
                json += "\"vy\":" + sphereVelocities[i].y + ",";
                json += "\"vz\":" + sphereVelocities[i].z + "}";

                if (i < spheresCount - 1)
                {
                    json += ",";
                }
            }
            json += "}}";
            

            return json;
        }

        /**
         * Saves the positions of the spheres as a json file.
         * @param spherePositions The position of each sphere.
         * @param sphereVelocities The velocity of each sphere.
         * @param spheresCount The count of all spheres of the guidewire.
         * @param totalTime The total time of the simulation.
         */
        public void savePositionsAsJson(Vector3[] spherePositions, Vector3[] sphereVelocities, int spheresCount, int simulationStep, float totalTime, float elapsedMilliseconds, float delta)
        {
            // Read the json file
            if (File.Exists(jsonPath))
            {
                json = File.ReadAllText(jsonPath);
                json = json.Substring(0, json.Length - 1);
                json += ",";
            }

            // Remove the last comma if the json is empty
            if (simulationStep == 0)
            {   
                jsonPath = parameterHandler.logFilePath + "positions.json";
                File.WriteAllText(jsonPath, json);
                json = json.Substring(0, json.Length - 1);
            }

            // Add the new entry
            json += AddEntry(spherePositions, sphereVelocities, spheresCount, simulationStep, totalTime, elapsedMilliseconds, delta);
            json += "}";

            // Write the json file
            File.WriteAllText(jsonPath, json);
        }
    }
}
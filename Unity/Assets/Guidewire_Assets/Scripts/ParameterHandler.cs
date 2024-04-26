using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GuidewireSim
{
    /**
     * This class manages all collisions that should be resolved, i.e. the collisions of the last frame.
     */
    public class ParameterHandler : MonoBehaviour
    {   
        float rodElementLength = 10f;
        [SerializeField] [Range(0.002f, 0.04f)] float timeStep = 0.01f;
        private string logFilePath = "";
        // TODO: What is this for?
        private float zDisplacement = 0.0f;

        private void Awake() 
        {

            // TODO: Can this be outsourced?
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

            // TODO: why is this here?
            // Get command line arguments
            //string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-logFilePath" && args.Length > i + 1)
                {
                    logFilePath = args[i + 1];
                }
            }
            // TODO: end

            //string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)   
            {
                if (args[i] == "-zDisplacement")
                {
                    zDisplacement = float.Parse(args[i + 1]);
                }
            }
        }

        public float GetRodElementLength()
        {
            return rodElementLength;
        }
    }
}        
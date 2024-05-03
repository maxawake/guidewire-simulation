using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


namespace GuidewireSim
{   
    // Helper class for handling command line arguments
    public class CommandLineHandler : MonoBehaviour
    {   
        private void Awake() {
            Debug.Log("CommandLineHandler Awake");
        }
        // Helper function for getting the command line arguments
        public string GetArg(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    }
}        
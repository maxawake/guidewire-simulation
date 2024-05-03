using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;

namespace GuidewireSim
{
    /**
     * This class manages all collisions that should be resolved, i.e. the collisions of the last frame.
     */
    public class DataLogger : MonoBehaviour
    {   
        // TODO: Get the path from the parameters file
        private string filePath = "/home/max/Temp/Praktikum/guidewire-log.txt";
        
        private void Awake()
        {
            Debug.Log("Logger Awake");
        }

        private void Start()
        {
            Debug.Log("Logger Start");
        }

        public void write(string message)
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            //using (StreamWriter writer = File.AppendText("log.txt"))
            {
                writer.WriteLine(message);
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GuidewireSim {
    [Serializable]
    public class ParameterHandler: MonoBehaviour
    {   
        public float rodElementLength = 5f;
        public float timeStep = 0.01f;
        public float zDisplacement = 0.0f;
        public string logFilePath = "/home/max/Temp/Praktikum/guidewire-log.txt";
        public Vector3 position = Vector3.zero;
		public Vector3 scale = Vector3.one;
		public Vector3 rotation = Vector3.zero;

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
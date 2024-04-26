using UnityEngine;
using System.IO;
using GuidewireSim;
using System;

// TODO: Check this script
public class CreationScript : MonoBehaviour
{
    private PredictionStep predictionStep;
    private string logFilePath = "";
    private Vector3 lastSpherePreviousPosition = Vector3.zero;
    public GameObject spherePrefab;
    public GameObject cylinderPrefab;
    private int spheresCount;
    private GameObject[] spheres;
    private GameObject[] cylinders;
    private SimulationLoop simulationLoop;
    private float initialCheckDelay = 3f;
    private int firstCallResetCounter = 0;
    private const int MaxFirstCallResets = 1;
    private Vector3[] lastSpherePositions;
    private Vector3[] lastSphereVelocities;



    void Start()
    {
        predictionStep = GameObject.Find("Simulation").GetComponent<PredictionStep>();
        simulationLoop = GameObject.Find("Simulation").GetComponent<SimulationLoop>();
        InvokeRepeating("SavePositionsToFile", 0f, 0.01f);
   
    }

    private void Awake()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-logFilePath" && args.Length > i + 1)
            {
                logFilePath = args[i + 1];
            }
        }
    }

    void FixedUpdate()
    {
        if (Time.time > initialCheckDelay)
        {
            CheckVelocityDifference();
        }
    }


    public void CreateGuidewire(int numberElements)
    {
        spheres = new GameObject[numberElements];
        cylinders = new GameObject[numberElements - 1];
        float rEL = simulationLoop.GetRodElementLength();
        lastSpherePositions = new Vector3[numberElements];

        for (int i = 0; i < numberElements; ++i)
        {
            GameObject sphere = Instantiate(spherePrefab);
            sphere.transform.position = new Vector3(0, 0, 44.5f + i * rEL);    //-444 fuer 1000 lang, -123 fur 2?, 44.5f fur 2!
            sphere.transform.parent = this.transform;
            spheres[i] = sphere;

            if (i < numberElements - 1)
            {
                GameObject cylinder = Instantiate(cylinderPrefab);
                cylinder.layer = 6;
                cylinder.transform.parent = this.transform;
                cylinders[i] = cylinder;
            }
      
        }

        spheresCount = numberElements;
        simulationLoop.SetSpheres(spheres);
        simulationLoop.SetCylinders(cylinders);
        
    }

    public void SavePositionsToFile()
    {
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            if (spheres != null && spheres.Length > 0)
            {
                Vector3 firstSpherePosition = spheres[0].transform.position;
                writer.WriteLine("First Sphere: " + firstSpherePosition.x + "," + firstSpherePosition.y + "," + firstSpherePosition.z);

                if (spheres.Length > 1)
                {
                    Vector3 lastSpherePosition = spheres[spheres.Length - 1].transform.position;
                    writer.WriteLine("Last Sphere: " + lastSpherePosition.x + "," + lastSpherePosition.y + "," + lastSpherePosition.z);
                }
            }
        }
    }


    public void UpdateSphereVelocities(Vector3[] velocities)
    {
        lastSphereVelocities = velocities;
    }

    private void CheckVelocityDifference()
    {
        string debugVelocityFilePath = "/home/max/Temp/Praktikum/DebugVelocities.txt";
        string positionFilePath = "/home/max/Temp/Praktikum/Position#N.txt";

        if (spheres != null && spheres.Length > 1 && lastSphereVelocities != null) 
        {
            bool allSpheresBelowVelocityThreshold = true;
            float epsilon = 0.000001f;
            float velocityDifferenceThreshold = 1f; // Define your velocity threshold here

            using (StreamWriter velocityWriter = new StreamWriter(debugVelocityFilePath, true),
                            positionWriter = new StreamWriter(positionFilePath, true))
            {
                for (int i = 1; i < spheres.Length; i++) 
                {
                    float velocityMagnitude = lastSphereVelocities[i].magnitude;
                    //velocityWriter.WriteLine("Sphere " + i + " Velocity Magnitude: " + velocityMagnitude);

                    if (velocityMagnitude >= velocityDifferenceThreshold + epsilon)
                    {
                        allSpheresBelowVelocityThreshold = false;
                        break;
                    }
                }

                if (allSpheresBelowVelocityThreshold)
                {
                    if (firstCallResetCounter < MaxFirstCallResets)
                    {
                        predictionStep.ResetFirstCall();
                        firstCallResetCounter++;
                        DateTime now = DateTime.Now;
                        string timestamp = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        velocityWriter.WriteLine($"Threshold reached at {timestamp}, triggering displacement.");
                        positionWriter.WriteLine($"Threshold reached at {timestamp}, triggering displacement.");
                    }

                    if (firstCallResetCounter >= MaxFirstCallResets)
                    {
                        Application.Quit(1);
                    }
                }
            }
        }
    }

    public GameObject GetLastSphere()
    {
        if (spheres != null && spheres.Length > 0)
        {
            return spheres[spheres.Length - 1];
        }
        return null;
    }

    public GameObject[] GetSpheres()
    {
        return spheres;
    }

    public GameObject[] GetCylinders()
    {
        return cylinders;
    }
}    

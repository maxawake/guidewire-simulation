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
    }

    private void Awake()
    {

    }

    void FixedUpdate()
    {

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
            // TODO: What is happening here? 
            sphere.transform.position = new Vector3(0, 0, -300f+i * rEL);    //-444 fuer 1000 lang, -123 fur 2?, 44.5f fur 2!
            sphere.transform.parent = this.transform;
            spheres[i] = sphere;

            if (i < numberElements - 1)
            {
                GameObject cylinder = Instantiate(cylinderPrefab);
                cylinder.layer = 6;
                cylinder.transform.parent = this.transform;
                cylinder.transform.localScale = new Vector3(1,1,2.5f);
                cylinders[i] = cylinder;
            }

        }

        spheresCount = numberElements;
        simulationLoop.SetSpheres(spheres);
        simulationLoop.SetCylinders(cylinders);

    }

    public void UpdateSphereVelocities(Vector3[] velocities)
    {
        lastSphereVelocities = velocities;
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

using UnityEngine;
using System.IO;
using GuidewireSim;
using System;
using UnityEngine.Assertions;

public class CreationScript : MonoBehaviour
{
    private PredictionStep predictionStep;
    private ParameterHandler parameterHandler;
    public GameObject spherePrefab;
    public GameObject cylinderPrefab;
    private GameObject[] spheres;
    private GameObject[] cylinders;
    private SimulationLoop simulationLoop;
    private float rodElementLength;
    public float guidewireOffset;
    private Vector3[] lastSpherePositions;
    private Vector3[] lastSphereVelocities;

    private void Awake()
    {
        predictionStep = GameObject.Find("Simulation").GetComponent<PredictionStep>();
        Assert.IsNotNull(predictionStep);

        simulationLoop = GameObject.Find("Simulation").GetComponent<SimulationLoop>();
        Assert.IsNotNull(simulationLoop);

        parameterHandler = GameObject.Find("Simulation").GetComponent<ParameterHandler>();
        Assert.IsNotNull(parameterHandler);
    }

    public void CreateGuidewire(int numberElements)
    {
        spheres = new GameObject[numberElements];
        cylinders = new GameObject[numberElements - 1];
        rodElementLength = simulationLoop.GetRodElementLength();
        guidewireOffset = parameterHandler.guidewireOffset;
        lastSpherePositions = new Vector3[numberElements];
        
        for (int i = 0; i < numberElements; ++i)
        {
            GameObject sphere = Instantiate(spherePrefab);

            // Set the position of the sphere on the guidewire
            sphere.transform.position = new Vector3(0, 0, guidewireOffset + i * rodElementLength);
            sphere.transform.parent = this.transform;
            spheres[i] = sphere;

            if (i < numberElements - 1)
            {
                GameObject cylinder = Instantiate(cylinderPrefab);
                cylinder.transform.parent = this.transform;

                // Stretch the cylinder to the correct length
                cylinder.transform.localScale = new Vector3(1,1,rodElementLength/10f);
                cylinders[i] = cylinder;
            }

        }

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

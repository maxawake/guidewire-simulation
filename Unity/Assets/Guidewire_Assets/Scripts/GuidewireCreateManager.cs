using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GuidewireSim;
using UnityEngine.Assertions;


// TODO: Check this script
public class GuidewireCreateManager : MonoBehaviour
{
    private CreationScript creationScript;
    private SimulationLoop simulationLoop;
    private ParameterHandler parameterHandler;


    public GameObject Simulation;
    private float guidewireLength;
    private int numberRodElements;
    private float rodElementLength;


    private void Awake() 
    {
        simulationLoop = Simulation.GetComponent<SimulationLoop>();
        Assert.IsNotNull(simulationLoop);

        parameterHandler = Simulation.GetComponent<ParameterHandler>();
        Assert.IsNotNull(parameterHandler);
    }

    private void Start()
    {
        //again we need to get the rod element length from the simulation loop script; 
        numberRodElements = parameterHandler.numberRodElements;
        guidewireLength = parameterHandler.guidewireLength;

        rodElementLength = guidewireLength / numberRodElements;

        //simulationLoop.rodElementLength(rodElementLength);

        //This will cause the total length of the guidewire to stay the same
        creationScript = FindAnyObjectByType<CreationScript>();
        if (creationScript != null)
        {
            creationScript.CreateGuidewire(numberRodElements+1);

            //Get the created spheres and cylinders from the CreationScript
            GameObject[] createdSpheres = creationScript.GetSpheres();
            GameObject[] createdCylinders = creationScript.GetCylinders();

            // Link them to the arrays in the SimulationLoop script
            simulationLoop.SetSpheres(createdSpheres);
            simulationLoop.SetCylinders(createdCylinders);
        }
        else
        {
            Debug.LogError("CreationScript component not found in the scene!");
        }
    }
}


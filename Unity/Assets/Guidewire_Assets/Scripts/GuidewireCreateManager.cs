using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GuidewireSim;

// TODO: Check this script
public class GuidewireCreateManager : MonoBehaviour
{
    private CreationScript creationScript;
    private SimulationLoop simulationLoop;

    public GameObject Simulation;
    public int L_0 = 100;
    private float REL;

    private void Start()
    {
        // Find the SimulationLoop component from the Simulation GameObject
        simulationLoop = Simulation.GetComponent<SimulationLoop>();

        if (simulationLoop == null)
        {
            Debug.LogError("SimulationLoop component not found in the Simulation GameObject!");
            return; // Exit if SimulationLoop is not found
        }

        //again we need to get the rod element length from the simulation loop script; 
        REL = 10.0f;//simulationLoop.GetRodElementLength();
        int numberOfElements = (int)(L_0 / REL) + 1;//This calculates the number of elements that now make up the discretized guidewire. The number of elements depends on the rod element length 

        //now find the CreationScript component in the scene, to create the guidewire with the wanted number of elements and rod element length.
        //This will cause the total length of the guidewire to stay the same
        creationScript = FindObjectOfType<CreationScript>();
        if (creationScript != null)
        {
            creationScript.CreateGuidewire(numberOfElements);

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


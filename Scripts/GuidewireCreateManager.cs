using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GuidewireCreateManager : MonoBehaviour
{
    private CreationScript creationScript;

    private void Start()
    {
        int numberOfElements = 5; //Here we can set the desired number of elements. This is something I will change later and replace with the length of the whole Guidewire, that is supposed to stay constant divided by the rodElementLength (rEL) of this Iteration.
        
        //Here the script is looking for the CreationScript component in the scene that this script is saved in
        creationScript = FindObjectOfType<CreationScript>();

        //Here the script is looking for the CreationScript component in the scene that this script is saved in
        if (creationScript != null)
        {
            creationScript.CreateGuidewire(numberOfElements);
        }
        else
        {
            Debug.LogError("CreationScript component not found in the scene!");
        }
    }
}

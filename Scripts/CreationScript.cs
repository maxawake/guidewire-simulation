using UnityEngine;
using System.IO;
using GuidewireSim;

public class CreationScript : MonoBehaviour
{
    public GameObject spherePrefab;       //This is for choosing a Prefab for the sphere element. One can directly gain this in Robins code from: Assets/Guidewire_Assets/Prefabs
    public GameObject cylinderPrefab;     //This is for choosing a Prefab for the cylinder element. One can directly gain this in Robins code from: Assets/Guidewire_Assets/Prefabs

    private int spheresCount;             //This variable is used to store the number of spheres in the guidewire, so the script can keep knowledge on how many spheres were created
    private int cylinderCount;            //the same concept as -''-

    private GameObject[] spheres;         //This array is used to store references to the sphere game objects in the guidewire. 
    private GameObject[] cylinders;       //-''- 

    void Update()
    {
        SavePositionsToFile();
    }
//This code segment is responsible for creating and positioning the spheres and cylinders 
    public void CreateGuidewire(int numberElements)
    {
        GameObject[] spheres = new GameObject[numberElements];
        GameObject[] cylinders = new GameObject[numberElements - 1];
        //Here we use GetComponent to recieve the value of the variable rodElementLength that is stored in the SimulationLoop script. As this variable needs to be private, we use a getter Method and therefore also call this instead of calling the variable directly.
        float rEL = GetComponent<SimulationLoop>().GetRodElementLength();

        Debug.Log(rEL);

        for (int i = 0; i < numberElements; ++i)
        {
            GameObject sphere = Instantiate(spherePrefab);
            #This defines, how the positions of the spheres are calculated, therefore the distance between two spheres is the rodElementLength
            sphere.transform.position = new Vector3(0, 0, i * rEL);

            spheres[i] = sphere;

            if (i < numberElements - 1)
            {
                GameObject cylinder = Instantiate(cylinderPrefab);
                cylinder.layer = 6;
                cylinders[i] = cylinder;
            }
        }

        spheresCount = numberElements;
        this.spheres = spheres;
        this.cylinders = cylinders;
    }
//This code segment is responsible for destroying the spheres and cylinders, but is not really used yet, as I am running the loop in a different way
    public void DestroyGuidewire()
    {
        for (int i = 0; i < spheresCount; ++i)
        {
            Destroy(spheres[i]);

            if (i < cylinderCount)
            {
                Destroy(cylinders[i]);
            }
        }

        spheresCount = 0;
        cylinderCount = 0;
    }

    public void SavePositionsToFile()
    {
    //Here I save the positions of the spheres to a .txt file. The file path has to be adapted to the local network for the file one wants to save the positons to
        string path = "/home/akreibich/TestRobinCode2/PositionsTest1.txt";
        StreamWriter writer = new StreamWriter(path, true);

        for (int i = 0; i < spheresCount; ++i)
        {
            Vector3 position = spheres[i].transform.position;
            writer.WriteLine(position.x + "," + position.y + "," + position.z);
        }

        writer.Close();
    }
}

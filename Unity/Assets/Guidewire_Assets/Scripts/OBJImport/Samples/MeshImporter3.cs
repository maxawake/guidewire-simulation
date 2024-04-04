using UnityEngine;
using System.IO;
using Dummiesman;
using System;

public class MeshImporter3 : MonoBehaviour
{
    void Awake()
    {
        string logFilePath = "/home/akreibich/TestRobinCode37/DebugLogs.txt";
        File.AppendAllText(logFilePath, "Awake started.\n");

        try
        {
            string[] args = Environment.GetCommandLineArgs();
            File.AppendAllText(logFilePath, "Received arguments: " + string.Join(", ", args) + "\n");

            string meshPath ="";
            string secondMeshPath =  "";
		//if run via the python loop, it should be like this
		Vector3 position = Vector3.zero;
		Vector3 scale = Vector3.one;
		Vector3 rotation = Vector3.zero;

		//Change it to this for testing
		//Vector3 position = new Vector3(1311f, -392f, -1197f);
		//Vector3 scale = new Vector3(27.2f, 27.2f, 27.2f);
		//Vector3 rotation = new Vector3(0f, -86.6f, 0f);

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-objPath")
                {
                    meshPath = args[i + 1];
                }
                else if (args[i] == "-secondObjPath")
                {
                    secondMeshPath = args[i + 1];
                }
                else if (args[i] == "-position")
                {
                    string[] posArgs = args[i + 1].Split(',');
                    position = new Vector3(float.Parse(posArgs[0]), float.Parse(posArgs[1]), float.Parse(posArgs[2]));
                }
                else if (args[i] == "-scale")
                {
                    string[] scaleArgs = args[i + 1].Split(',');
                    scale = new Vector3(float.Parse(scaleArgs[0]), float.Parse(scaleArgs[1]), float.Parse(scaleArgs[2]));
                }
                else if (args[i] == "-rotation")
                {
                    string[] rotArgs = args[i + 1].Split(',');
                    rotation = new Vector3(float.Parse(rotArgs[0]), float.Parse(rotArgs[1]), float.Parse(rotArgs[2]));
                }
            }

            if (string.IsNullOrEmpty(meshPath))
            {
                Debug.LogError("Primary mesh path not specified");
                File.AppendAllText(logFilePath, "Primary mesh path not specified\n");
                return;
            }

            Mesh mesh = LoadMesh(meshPath, logFilePath);
            Mesh secondMesh = string.IsNullOrEmpty(secondMeshPath) ? null : LoadMesh(secondMeshPath, logFilePath);

            ImportMesh(mesh, secondMesh, position, scale, rotation);
        }
        catch (Exception e)
        {
            Debug.LogError("An exception occurred: " + e.ToString());
            File.AppendAllText(logFilePath, "An exception occurred: " + e.ToString() + "\n");
        }
    }

    Mesh LoadMesh(string meshPath, string logFilePath)
    {
        OBJLoader objLoader = new OBJLoader();
        GameObject loadedObj = objLoader.Load(meshPath);

        if (loadedObj == null)
        {
            Debug.LogError("Failed to load mesh from path: " + meshPath);
            File.AppendAllText(logFilePath, "Failed to load mesh from path: " + meshPath + "\n");
            return null;
        }

        File.AppendAllText(logFilePath, "Successfully loaded mesh from path: " + meshPath + "\n");

        MeshFilter meshFilter = loadedObj.GetComponentInChildren<MeshFilter>();
        Destroy(loadedObj);

        if (meshFilter == null)
        {
            Debug.LogError("No MeshFilter component found in loaded GameObject");
            File.AppendAllText(logFilePath, "No MeshFilter component found in loaded GameObject\n");
            return null;
        }

        return meshFilter.sharedMesh;
    }

    void ImportMesh(Mesh primaryMesh, Mesh secondaryMesh, Vector3 position, Vector3 scale, Vector3 rotation)
    {
        if (primaryMesh == null)
        {
            Debug.LogError("Primary mesh is null");
            return;
        }

        GameObject obj = new GameObject(primaryMesh.name);
        obj.transform.position = position;
        obj.transform.localScale = scale;
        obj.transform.eulerAngles = rotation;

        MeshFilter filter = obj.AddComponent<MeshFilter>();
        filter.mesh = primaryMesh;

        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
        Material mat = Resources.Load<Material>("Aorta Material");

        if (mat != null)
        {
            renderer.material = mat;
        }
        else
        {
            Debug.LogError("Failed to load material");
        }

        MeshCollider primaryCollider = obj.AddComponent<MeshCollider>();
        primaryCollider.sharedMesh = primaryMesh;

        if (secondaryMesh != null)
        {
            MeshCollider secondaryCollider = obj.AddComponent<MeshCollider>();
            secondaryCollider.sharedMesh = secondaryMesh;
        }

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        int layerIndex = LayerMask.NameToLayer("Blood Vessel");

        if (layerIndex == -1)
        {
            Debug.LogError("Layer 'Blood Vessel' not found");
        }
        else
        {
            obj.layer = layerIndex;
        }
    }
}


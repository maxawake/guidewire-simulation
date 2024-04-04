using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace vascularIntervention
{
    /// <summary>
    /// This singleton class is responsible for managing the ctdata and saved treatments.
    /// </summary>
    public class Data : MonoBehaviour
    {   
        public static Data Instance { get; private set; } // Instance for accessing the data.

        private List<CTData> ctDataList; // List contains the ct data.
        private int currentIndex; // Index saves access to current ct data.
        private int maxCapacity = 15; // Maximum capacity of the ct data list.
        private List<string> paths; // List of paths to saved treatments.

        /**
        * Default settings (default body, default aorta, default starting point)
        */
        [SerializeField] private Texture3D defaultCTBody;
        [SerializeField] private Texture3D defaultCTAorta;
        [SerializeField] private Mesh defaultAortaModelMesh;
        [SerializeField] private Vector3 defaultStartingPoint;

        // Awake is called when script object is loaded. 
        private void Awake()
        {
            // Access singleton instance.
            if (Instance == null)
            {
                Instance = this; 
            }

            Load(); // Load default data.

            Assert.IsNotNull(ctDataList, "CT data list is null");
            Assert.IsNotNull(paths, "Path list is null");
            Assert.IsNotNull(defaultCTBody, "Default CT body is null");
            Assert.IsNotNull(defaultCTAorta, "Default CT aorta is null");
            Assert.IsNotNull(defaultAortaModelMesh, "Default aorta model mesh is null");
        }

        /// <summary>
        /// Get the current number of the ct data.
        /// </summary>
        /// <returns></returns>
        public int getDataCount()
        {
            return ctDataList.Count;
        }

        /// <summary>
        /// Get ct data by accessing the ct data list.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public CTData getCTData(int index)
        {
            Assert.IsTrue(index >= 0 && index < maxCapacity, "index for accessing the ctdata is not valid");
            return ctDataList[index];
        }

        /// <summary>
        /// Add ct data to the ct data list.
        /// </summary>
        /// <param name="data"></param>
        public void addCTData(CTData data)
        {
            if (ctDataList.Count < ctDataList.Capacity)
            {
                ctDataList.Add(data);
            }
            else
            {
                Debug.LogError("Error: Max Capacity reached");
            }
        }

        /// <summary>
        /// Get path to saved treatment by accessing the path list.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string getPath(int index)
        {
            Assert.IsTrue(index >= 0 && index < maxCapacity, "index for accessing the path is not valid");
            return paths[index];
        }

        /// <summary>
        /// Get current number of saved treatments.
        /// </summary>
        /// <returns></returns>
        public int getPathCount()
        {
            return paths.Count;
        }

        /// <summary>
        /// Add path of saved treatment to path list.
        /// </summary>
        /// <param name="path"></param>
        public void addPath(string path)
        {
            paths.Add(path);
        }

        /// <summary>
        /// Get index of current ct data.
        /// </summary>
        /// <returns></returns>
        public int getCurrentIndex()
        {
            return currentIndex;
        }

        /// <summary>
        /// Set index of current ct data.
        /// </summary>
        /// <param name="index"></param>
        public void setCurrentIndex(int index)
        {
            Assert.IsTrue(index >= 0 && index < maxCapacity, "index for accessing the ct data list is not valid");
            currentIndex = index;
        }


        /// <summary>
        /// Load default data. Called in awake function.
        /// </summary>
        private void Load()
        {
            ctDataList = new List<CTData>(maxCapacity); // Create ct data list with maximum capacity.
            paths = new List<string>(maxCapacity); // Create path list with maximum capacity.

            AortaModel aortaModel = new AortaModel(defaultAortaModelMesh, defaultStartingPoint); // Create aorta model.
            CTData cTData = new CTData(defaultCTBody, defaultCTAorta, aortaModel, "default"); // Create ct data.
            Assert.IsNotNull(cTData, "default ctdata is null");
            
            ctDataList.Add(cTData);
            Assert.IsTrue(ctDataList.Count == 1, "Number of ct data is not 1");
        }
    }

    /// <summary>
    /// This class is resposible for creating a ct data.
    /// </summary>
    public class CTData
    {
        private Texture3D ctBody; // ct data of body is saved in a 3D texture.
        private Texture3D ctAorta; // ct data of aorta (segmentation) is saved in a 3D texture.
        private AortaModel aortaModel; // 3D aorta model with a triangular mesh structure.
        private string name; // Name of ct data.

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ctBody"></param>
        /// <param name="ctAorta"></param>
        /// <param name="aortaModel"></param>
        /// <param name="name"></param>
        public CTData(Texture3D ctBody, Texture3D ctAorta, AortaModel aortaModel, string name)
        {
            this.ctBody = ctBody;
            this.ctAorta = ctAorta;
            this.name = name;
            this.aortaModel = aortaModel;

            Assert.IsNotNull(this.ctBody, "ct body texture is null");
            Assert.IsNotNull(this.ctAorta, "ct aorta texture is null");
            Assert.IsNotNull(this.aortaModel, "3D aorta model is null");
        }

        /// <summary>
        /// Get name of ct data.
        /// </summary>
        /// <returns></returns>
        public string getName()
        {
            return name;
        }

        /// <summary>
        /// Get ct body texture.
        /// </summary>
        /// <returns></returns>
        public Texture3D getctBody()
        {
            return ctBody;
        }

        /// <summary>
        /// Get ct aorta texture.
        /// </summary>
        /// <returns></returns>
        public Texture3D getctAorta()
        {
            return ctAorta;
        }

        /// <summary>
        /// Get 3D aorta model.
        /// </summary>
        /// <returns></returns>
        public AortaModel getAortaModel()
        {
            return aortaModel;
        }
    }

    /// <summary>
    /// This class is responsible for creating a 3D aorta model. 
    /// </summary>
    public class AortaModel
    {
        private Mesh mesh; // Mesh of 3D aorta model.
        private Vector3 startingPoint; // Starting point of the aorta to move the guidewire.

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="aortaMesh"></param>
        /// <param name="startingPoint"></param>
        public AortaModel(Mesh aortaMesh, Vector3 startingPoint)
        {
            mesh = new Mesh(); 
            mesh.name = "aorta";
            mesh.vertices = aortaMesh.vertices;
            mesh.uv = aortaMesh.uv;
            mesh.normals = aortaMesh.normals;
            mesh.triangles = aortaMesh.triangles;

            this.startingPoint = startingPoint;

            Assert.IsNotNull(mesh, "Mesh of 3D aorta model is null");
        }

        /// <summary>
        /// Get triangular mesh of aorta model.
        /// </summary>
        /// <returns></returns>
        public Mesh getMesh()
        {
            return mesh;
        }

        /// <summary>
        /// Get the starting point for moving the guide wire.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetStartingPoint()
        {
            return startingPoint;
        }

        /// <summary>
        /// Static function to create a 3D aorta model from a .obj file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="startingPoint"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        public static AortaModel CreateAortaModel(string path, Vector3 startingPoint)
        {
            Assert.IsTrue(File.Exists(path), "Path to obj file not exists");
            /*
             * Scale the starting point.
             */
            startingPoint.x = startingPoint.x * 2000;
            startingPoint.y = startingPoint.y * 2000;
            startingPoint.z = startingPoint.z * 2000;

            string[] lines = File.ReadAllLines(path); // save data from .obj file in a string list.

            // Create lists for the mesh attributes: vertices, uv coordinates, normals and triangles.
            List<Vector3> vertices = new List<Vector3>(); 
            List<Vector2> uvCoordinates = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();

            // Read data from string list.
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.StartsWith("v ")) // Line starting with "v" contains vertex data.
                {
                    string[] lineContent = line.Split(' '); // vertex coordinates are separated with spaces.
                    Vector3 vertex = new Vector3(
                        float.Parse(lineContent[1], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(lineContent[2], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(lineContent[3], CultureInfo.InvariantCulture.NumberFormat) 
                    );
                    vertices.Add(vertex); 
                }
                else if (line.StartsWith("f ")) // Line starting with "f" contains triangle data.
                {
                    string[] lineContent = line.Split(' '); 
                    int[] triangle = new int[3];
                    for (int j = 1; j < 4; j++)
                    {
                        string[] indices = lineContent[j].Split('/'); // Triangle indices are separated with slashes.
                        triangle[j - 1] = int.Parse(indices[0]) - 1; // Subtraction is performed because the index starts with 1 in the .obj format.
                    }
                    triangles.Add(triangle[0]);
                    triangles.Add(triangle[1]);
                    triangles.Add(triangle[2]);
                }
                else if (line.StartsWith("vt ")) // Line starting with "vt" contains the uv coordinates.
                {
                    string[] lineContent = line.Split(' '); // Uv coordinates are separated with spaces.
                    Vector2 uv = new Vector2(
                        float.Parse(lineContent[1], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(lineContent[2], CultureInfo.InvariantCulture.NumberFormat)
                    );
                    uvCoordinates.Add(uv);
                }
                else if (line.StartsWith("vn ")) // Line starting with "vn" contains normal data.
                {
                    string[] lineContent = line.Split(' ');
                    Vector3 normal = new Vector3(
                        float.Parse(lineContent[1], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(lineContent[2], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(lineContent[3], CultureInfo.InvariantCulture.NumberFormat)
                    );
                    normals.Add(normal);
                }
            }

            /*
             * Create mesh and assign attributes.
             */
            Mesh mesh = new Mesh();
            mesh.name = "aortaMesh";
            mesh.vertices = vertices.ToArray();
            mesh.uv = uvCoordinates.ToArray();
            mesh.normals = normals.ToArray();
            mesh.triangles = triangles.ToArray();

            AortaModel aortaModel = new AortaModel(mesh, startingPoint);

            return aortaModel;
        }
    }
}
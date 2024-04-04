using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace vascularIntervention
{
    /// <summary>
    /// This class is responsible for managing the treatment.
    /// </summary>
    public class TreatmentOptions : MonoBehaviour
    {
        private Navigation navigation;  // Navigation script for the rotation of the CArm and translation of the XRayTable.
        private GuidewireTexture guideWireTexture; // GuidewireTexture script which is responsible for drawing the guidewire.
        private GuidewireAPI guidewireAPI; // GuidewireAPI script which is responsible for accessing the guidewire model.
        private SceneManager sceneManager; // SceneManager script which is responsible for managing the whole scene.

        [SerializeField] private GameObject aortaObject; // Prefab of aorta object without mesh. Using a current ct data, the corresponding mesh of the aorta will be attached to the object.
        [SerializeField] Toggle recordToggle; // Toggle to enable/disable record.
        private GameObject aortaObjectInstance;

        [SerializeField] private GameObject guideWireSimulation; // Game Object containing the main script of the guidewire model.


        // Awake is called when script instance is loaded
        private void Awake()
        {
            Assert.IsNotNull(guideWireSimulation, "Guidewire Simulation Object not assigned");

            navigation = this.GetComponent<Navigation>();
            guideWireTexture = this.GetComponent<GuidewireTexture>();
            guidewireAPI = this.GetComponent<GuidewireAPI>();
            sceneManager = this.GetComponent<SceneManager>();

            Assert.IsNotNull(navigation, "Navigation script component not attached");
            Assert.IsNotNull(guideWireTexture, "GuideWireTexture script component not attached");
            Assert.IsNotNull(guidewireAPI, "GuidewireAPI script component not attached");
            Assert.IsNotNull(guidewireAPI, "SceneManager script component not attached");

            Assert.IsTrue(aortaObject.layer == 3, "Aorta object has no layer Blood Vessel");
            Assert.IsNotNull(aortaObject.GetComponent<MeshFilter>(), "Aorta object prefab has no Meshfilter");
            Assert.IsNotNull(aortaObject.GetComponent<MeshRenderer>(), "Aorta object prefab has no MeshRenderer");
            Assert.IsNotNull(aortaObject.GetComponent<MeshCollider>(), "Aorta object prefab has no Meshcollider");
            Assert.IsNotNull(aortaObject.GetComponent<Rigidbody>(), "Aorta object prefab has no Rigidbody");
            Assert.IsFalse(aortaObject.GetComponent<Rigidbody>().useGravity, "Rigidbody of aorta object prefab uses gravity");

            setEnableScriptsForTreatment(false);
        }
        
        // Start is called before the first frame update
        void Start()
        {
            guideWireSimulation.SetActive(false);
        }

        /// <summary>
        /// Listener function to start the treatment. If all options were set, the treatment can be started.
        /// </summary>
        public void OnStartInOptionsClicked()
        {
            if (this.GetComponent<GuidewireAPI>().canStartTreatment) // treatment can be started if GuidewireAPI 
            {
                sceneManager.startTreatment();
                guideWireSimulation.SetActive(true);

                setEnableScriptsForTreatment(true);

                attachAorta(Data.Instance.getCurrentIndex());

                guidewireAPI.ActivateGuidewire(); // Active guidewire simulation

                if (recordToggle.isOn) // check if toggle is on to record.
                {
                    this.GetComponent<Recorder>().startRecording();
                    int pathCount = Data.Instance.getPathCount();
                    string path = Application.persistentDataPath + "/ImageSequences/ImageSequence" + (pathCount);
                    this.GetComponent<Recorder>().setPath(path);
                    Data.Instance.addPath(path); 
                }
            }
        }

        /// <summary>
        /// Enable or disable scripts which are used for the treatment
        /// </summary>
        /// <param name="enable"></param>
        void setEnableScriptsForTreatment(bool enable)
        {
            navigation.enabled = enable;
            guideWireTexture.enabled = enable;
        }

        /// <summary>
        /// Listener function to end the treatment.
        /// </summary>
        public void OnExitInTreatmentClicked()
        {
            sceneManager.endTreatment();
            this.GetComponent<GuidewireAPI>().DeactivateGuidewire();

            setEnableScriptsForTreatment(false);

            guideWireSimulation.SetActive(false);

            this.GetComponent<Recorder>().stopRecording();
            this.GetComponent<Navigation>().Reset();
        }

        /// <summary>
        /// Attach mesh of aorta to the aorta object prefab
        /// </summary>
        /// <param name="currentIndex"></param>
        void attachAorta(int currentIndex)
        {
            Mesh aortaMesh = Data.Instance.getCTData(currentIndex).getAortaModel().getMesh();

            Assert.IsNotNull(aortaMesh, "aorta mesh is null");

            if(aortaObjectInstance!=null)
                Destroy(aortaObjectInstance); 

            aortaObjectInstance = Instantiate(aortaObject);

            Assert.IsNotNull(aortaObjectInstance, "aorta object instance is null");

            aortaObjectInstance.transform.position -= Data.Instance.getCTData(currentIndex).getAortaModel().GetStartingPoint();

            aortaObjectInstance.GetComponent<MeshFilter>().mesh = aortaMesh;
            aortaObjectInstance.GetComponent<MeshCollider>().sharedMesh = aortaMesh;

            this.GetComponent<GuidewireTexture>().setAortaPosition(aortaObjectInstance.transform.position); // Guidewire texture needs position of aorta object to viusalize the guidewire correctly.
        }

        /// <summary>
        /// Change position of the aorta.
        /// </summary>
        /// <param name="position"></param>
        public void changePositionOfAorta(Vector3 position)
        {
            aortaObjectInstance.transform.position += position;
        }
    }
}
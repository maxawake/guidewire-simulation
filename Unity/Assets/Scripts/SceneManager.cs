using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GuidewireSim;
using UnityEngine.Assertions;

namespace vascularIntervention
{
    /// <summary>
    /// This class is responsible for managing the whole 3D scene.
    /// </summary>
    public class SceneManager : MonoBehaviour
    {
        /**
         * Panels
         */
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject dataManagementMenu;
        [SerializeField] private GameObject treatmentMenu;
        [SerializeField] private GameObject startOptionsMenu;
        [SerializeField] private GameObject viewTreamentsMenu;

        private DataManagementMenu dataManagement;
        private ViewTreatmentsMenu viewTreatments;

        // Awake is called when script instance is loaded
        public void Awake()
        {
            dataManagement = this.GetComponent<DataManagementMenu>();
            viewTreatments = this.GetComponent<ViewTreatmentsMenu>();

            Assert.IsNotNull(dataManagement, "datamanagementMenu script not attached.");
            Assert.IsNotNull(viewTreatments, "viewTreatments script not attached.");

            Assert.IsNotNull(mainMenu, "Main menu panel not assigned.");
            Assert.IsNotNull(dataManagementMenu, "Data management menu panel not assigned.");
            Assert.IsNotNull(treatmentMenu, "Treatment menu panel not assigned.");
            Assert.IsNotNull(startOptionsMenu, "Start options menu panel not assigned.");
            Assert.IsNotNull(viewTreamentsMenu, "View treatments menu panel not assigned.");
        }

        // Start is called before first frame
        public void Start()
        {
            mainMenu.SetActive(true); 
            treatmentMenu.SetActive(false);
            dataManagementMenu.SetActive(false);
            startOptionsMenu.SetActive(false);
            viewTreamentsMenu.SetActive(false);

            Assert.IsTrue(mainMenu.activeSelf, "Main menu not opened");
            Assert.IsFalse(treatmentMenu.activeSelf, "Treatments menu is not closed");
            Assert.IsFalse(dataManagementMenu.activeSelf, "Data managment menu is not closed");
            Assert.IsFalse(startOptionsMenu.activeSelf, "Start options menu is not closed");
            Assert.IsFalse(viewTreamentsMenu.activeSelf, "View treatments menu is not closed");
        }

        /// <summary>
        /// Listener function to open the start options menu before starting the treatment.
        /// </summary>
        public void OnStartButtonClicked()
        {
            mainMenu.SetActive(false);
            startOptionsMenu.SetActive(true);

            Assert.IsFalse(mainMenu.activeSelf, "Main menu ís not closed");
            Assert.IsFalse(dataManagementMenu.activeSelf, "Data managment menu is not closed");
            Assert.IsFalse(viewTreamentsMenu.activeSelf, "View treatments menu is not closed");
            Assert.IsFalse(treatmentMenu.activeSelf, "Treatments menu is not closed");
            Assert.IsTrue(startOptionsMenu.activeSelf, "Start options menu not opened");
        }


        /// <summary>
        /// Listener function to end the program.
        /// </summary>
        public void OnExitInMainMenuClicked()
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }

        /// <summary>
        /// Listener function to open the data managment menu.
        /// </summary>
        public void onDataManagementMenuClicked()
        {
            dataManagementMenu.SetActive(true);
            mainMenu.SetActive(false);

            dataManagement.updateContentList();

            Assert.IsFalse(mainMenu.activeSelf, "Main menu ís not closed");
            Assert.IsFalse(viewTreamentsMenu.activeSelf, "View treatments menu is not closed");
            Assert.IsFalse(startOptionsMenu.activeSelf, "Start options menu not closed");
            Assert.IsFalse(treatmentMenu.activeSelf, "Treatments menu is not closed");
            Assert.IsTrue(dataManagementMenu.activeSelf, "Data managment menu is not opened");

            ShaderProperties.Instance.sendXrayOn(true);
            ShaderProperties.Instance.sendContrastDyeInserted(false);
            ShaderProperties.Instance.sendRotationData(0, 0, 0, 0);
            ShaderProperties.Instance.sendContrastDyeInserted(true);
        }

        /// <summary>
        /// Listener function to exit the data management menu
        /// </summary>
        public void onExitInDataManagementClicked()
        {
            /*
             *  Turn of the view mode in the data management menu and send the current ct data to the xray shader material.
             */
            CTData cTData = Data.Instance.getCTData(Data.Instance.getCurrentIndex());
            ShaderProperties.Instance.sendCTTextures(cTData.getctBody(), cTData.getctAorta());
            ShaderProperties.Instance.sendXrayOn(false);
            ShaderProperties.Instance.sendContrastDyeInserted(false);

            mainMenu.SetActive(true);
            dataManagementMenu.SetActive(false);

            Assert.IsFalse(viewTreamentsMenu.activeSelf, "View treatments menu is not closed");
            Assert.IsFalse(startOptionsMenu.activeSelf, "Start options menu not closed");
            Assert.IsFalse(treatmentMenu.activeSelf, "Treatments menu is not closed");
            Assert.IsFalse(dataManagementMenu.activeSelf, "Data managment menu is not closed");
            Assert.IsTrue(mainMenu.activeSelf, "Main menu ís not opened");
        }


        /// <summary>
        /// Listener function to open view treatments menu.
        /// </summary>
        public void onViewTreatmentMenuClicked()
        {
            viewTreamentsMenu.SetActive(true);
            mainMenu.SetActive(false);
            treatmentMenu.SetActive(false);
            dataManagementMenu.SetActive(false);

            viewTreatments.updateContentList();
        }


        /// <summary>
        /// Listener function to exit view treatments menu.
        /// </summary>
        public void onExitInViewTreatmentsClicked()
        {
            viewTreamentsMenu.SetActive(false);
            mainMenu.SetActive(true);
            treatmentMenu.SetActive(false);
            dataManagementMenu.SetActive(false);
        }

        /// <summary>
        /// Function to switch to the treament view.
        /// </summary>
        public void startTreatment()
        {
            treatmentMenu.SetActive(true);
            startOptionsMenu.SetActive(false);

            Assert.IsFalse(mainMenu.activeSelf, "Main menu ís not closed");
            Assert.IsFalse(dataManagementMenu.activeSelf, "Data managment menu is not closed");
            Assert.IsFalse(viewTreamentsMenu.activeSelf, "View treatments menu is not closed");
            Assert.IsFalse(startOptionsMenu.activeSelf, "Start options menu not closed");
            Assert.IsTrue(treatmentMenu.activeSelf, "Treatments menu is not opened");
        }

        /// <summary>
        /// Function to return to the main menu.
        /// </summary>
        public void endTreatment()
        {
            mainMenu.SetActive(true);
            treatmentMenu.SetActive(false);
        }
    }
}
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace vascularIntervention
{
    /// <summary>
    /// This class is responsible for managing the data management menu.
    /// </summary>
    public class DataManagementMenu : MonoBehaviour
    {
        [SerializeField] private GameObject dataContentList; // Content list of the scroll view at data management menu.
        [SerializeField] private GameObject dataContentPrefab; // Prefab of a data content.

        // Awake is called when script instance is loaded
        private void Awake()
        {
            Assert.IsNotNull(dataContentList, "Data content list object not assigned");
            Assert.IsNotNull(dataContentPrefab, "Data content prefab not assigned");
        }

        // Start is called before the first frame update
        private void Start()
        {
            updateContentList(); // Update the content list which contains all available ct data. The User can view the ct data on the screen and can select one for the treatment.
        }

        /// <summary>
        /// Update the content list in data management menu.
        /// </summary>
        public void updateContentList()
        {
            foreach (Transform go in dataContentList.transform)
            {
                Destroy(go.gameObject);
            }

            for (int i = 0; i < Data.Instance.getDataCount(); ++i)
            {
                GameObject content = Instantiate(dataContentPrefab, dataContentList.transform);
                Assert.IsNotNull(content, "CT data content is null.");
                content.name = i.ToString();
                content.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Text>().text = Data.Instance.getCTData(i).getName();

                Button viewButton = content.transform.GetChild(0).GetChild(1).GetChild(0).gameObject.GetComponent<Button>();
                viewButton.onClick.AddListener(() => onViewInDataManagementClicked(int.Parse(content.name)));

                Button selectButton = content.transform.GetChild(0).GetChild(1).GetChild(1).gameObject.GetComponent<Button>();
                selectButton.onClick.AddListener(() => onSelectInDataManagementClicked(int.Parse(content.name)));
            }
        }

        /// <summary>
        /// Listener function to view a ct data in data management menu.
        /// </summary>
        /// <param name="index"></param>
        public void onViewInDataManagementClicked(int index)
        {
            Assert.IsTrue(index >= 0 && index < Data.Instance.getDataCount(), "Index for viewing ct data is not valid.");
            CTData ctData = Data.Instance.getCTData(index);
            ShaderProperties.Instance.sendCTTextures(ctData.getctBody(), ctData.getctAorta());
        }

        /// <summary>
        /// Listener function to select a ct data in data management menu.
        /// </summary>
        /// <param name="index"></param>
        public void onSelectInDataManagementClicked(int index)
        {
            Assert.IsTrue(index >= 0 && index < Data.Instance.getDataCount(), "Index for selecting ct data is not valid.");
            CTData ctData = Data.Instance.getCTData(index);
            ShaderProperties.Instance.sendCTTextures(ctData.getctBody(), ctData.getctAorta());

            Data.Instance.setCurrentIndex(index);
        }

        /// <summary>
        /// Listener function to upload ct data in data management menu.
        /// </summary>
        public void onUploadInDataManagementClicked()
        {
            string pathBody = EditorUtility.OpenFilePanel("Select CT Data: Body", "", "nrrd");
            string pathAorta = EditorUtility.OpenFilePanel("Select CT Data: Aorta", "", "nrrd");
            string pathAorta3DMesh = EditorUtility.OpenFilePanel("Select 3D Mesh", "", "obj");

            if (File.Exists(pathBody) && File.Exists(pathAorta) && File.Exists(pathAorta3DMesh))
            {
                Texture3D textureBody = Reader.CreateTextureFromNRRD(pathBody);
                Texture3D textureAorta = Reader.CreateTextureFromNRRD(pathAorta);

                AortaModel aorta = AortaModel.CreateAortaModel(pathAorta3DMesh, new Vector3(0, 0, 0));
                CTData ctdata = new CTData(textureBody, textureAorta, aorta, "Upload");
                Data.Instance.addCTData(ctdata);

                updateContentList();
            }
        }
    }
}
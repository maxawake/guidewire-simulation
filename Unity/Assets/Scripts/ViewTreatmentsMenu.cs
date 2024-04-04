using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace vascularIntervention
{
    /// <summary>
    /// This class is responsible for managing the view treatments menu.
    /// </summary>
    public class ViewTreatmentsMenu : MonoBehaviour
    {

        [SerializeField] private GameObject treatmentContentList; // Content list of the scroll view at  view treatment menu.
        [SerializeField] private GameObject treatMentContentPrefab; // Prefab of treatment content.
        private void Awake()
        {
            Assert.IsNotNull(treatmentContentList, "Treatment content list object not assigned.");
            Assert.IsNotNull(treatMentContentPrefab, "Treatment content prefab not assigned.");
        }

        // Start is called before the first frame update
        void Start()
        {
            updateContentList();
        }

        /// <summary>
        /// Listener function to view treatment in view treatments menu.
        /// </summary>
        /// <param name="path"></param>
        public void OnViewTreatmentClicked(int index)
        {
            EditorUtility.RevealInFinder(Data.Instance.getPath(index));
        }

        public void updateContentList()
        {
            foreach (Transform go in treatmentContentList.transform)
            {
                Destroy(go.gameObject);
            }

            for (int i = 0; i < Data.Instance.getPathCount(); ++i)
            {
                GameObject treatMentContent = Instantiate(treatMentContentPrefab, treatmentContentList.transform);
                treatMentContent.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => OnViewTreatmentClicked(i-1));
            }
        }
    }
}
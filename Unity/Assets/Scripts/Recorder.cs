using System.IO;
using UnityEngine;

namespace vascularIntervention
{
    /// <summary>
    /// This class is responsible for recording the treatment.
    /// </summary>
    public class Recorder : MonoBehaviour
    {
        /*
         * Resolution of record.
         */
        private int frameWidth = 1920;
        private int frameHeight = 1080;
       
        private bool canRecord = false; // Checks if treatment can be recorded.
        private string path; // Path to saved treatment.

        public int frameRecord = 50; // Defines after how many frames are needed to record the treatment screen.
        private int counter = 0; // Count frame.

        // Update is called once per frame.
        void Update()
        {
            counter += 1;
            if (counter % 50 == 0 && canRecord) //
            {
                string updatedPath = path + "/" + counter + ".png";
                ScreenCapture.CaptureScreenshot(updatedPath);
            }
        }

        /// <summary>
        /// Reset Counter.
        /// </summary>
        public void resetCounter()
        {
            counter = 0;
        }

        /// <summary>
        /// Set the path where the treatment is saved.
        /// </summary>
        /// <param name="path"></param>
        public void setPath(string path)
        {
            this.path = path;
            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Start to record.
        /// </summary>
        public void startRecording()
        {
            counter = 0;
            canRecord = true;
        }

        /// <summary>
        /// Stop to record.
        /// </summary>
        public void stopRecording()
        {
            canRecord = false;
        }
    }
}

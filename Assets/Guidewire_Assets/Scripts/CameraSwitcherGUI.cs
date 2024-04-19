using UnityEngine;
using UnityEngine.UI;

// TODO: Check this script
public class CameraSwitcherGUI : MonoBehaviour
{
    public Camera camera1;
    public Camera camera2;
    public Button switchButton;  // Reference to the UI button

    private void Start()
    {
        // Initialize camera states
        camera1.enabled = true;
        camera2.enabled = false;

        // Add a click listener to the button
        switchButton.onClick.AddListener(SwitchCameras);
    }

    public void SwitchCameras()
    {
        camera1.enabled = !camera1.enabled;
        camera2.enabled = !camera2.enabled;
    }
}


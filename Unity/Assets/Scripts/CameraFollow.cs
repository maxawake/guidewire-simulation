using UnityEngine;

/// <summary>
/// This class is responsible for managing the camera to follow the aorta in the 3D space.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    private GameObject target; // Target of the camera to follow.
    private int switchCameraIndex = 0; // Index to switch the camera.

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            switch (switchCameraIndex)
            {
                case 0:
                    transform.position = target.transform.position + new Vector3(0,2,2);
                    transform.rotation = target.transform.rotation;
                    break;
                case 1:
                    transform.position = target.transform.position + new Vector3(50, 0, 0);
                    transform.LookAt(target.transform);
                    
                    break;
            }
        }
    }

    /// <summary>
    /// Switch camera view.
    /// </summary>
    public void onButtonSwitchCameraClicked()
    {
        switchCameraIndex = (switchCameraIndex + 1) % 2;
    }

    /// <summary>
    /// Set target of camera.
    /// </summary>
    /// <param name="target"></param>
    public void setTarget(GameObject target)
    {
        this.target = target;
    }
}

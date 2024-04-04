using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


/// <summary>
/// This class is responsible for handling the joystick to control the CArm and the XRay Table.
/// </summary>
public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Image background; // Background image of the joystick.
    private Image joystick; // Image of the joystick which will be moved.

    public Vector2 inputDirection { set; get; } // Input direction is between [0,1] for x and y-axis.

    // Start is called before first frame
    private void Start()
    {
        background = GetComponent<Image>();
        joystick = transform.GetChild(0).GetComponent<Image>();
        inputDirection = Vector2.zero;
    }

    /// <summary>
    /// When dragging the joystick, this function gets called.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos = Vector2.zero;

        float backgroundSizeX = background.rectTransform.sizeDelta.x;
        float backgroundSizeY = background.rectTransform.sizeDelta.y;

        /*
         * Get the position of the joystick.
         */
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(background.rectTransform, eventData.position, eventData.pressEventCamera, out pos))
        {
            pos.x /= backgroundSizeX;
            pos.y /= backgroundSizeY;
            inputDirection = new Vector2(pos.x, pos.y);
            inputDirection = inputDirection.magnitude > 1 ? inputDirection.normalized : inputDirection; // Normalize the input direction.
            joystick.rectTransform.anchoredPosition = new Vector2(inputDirection.x * backgroundSizeX / 3f, inputDirection.y * backgroundSizeY / 3f); // Move the joystick image.
        }

    }

    /// <summary>
    /// This function is called when the joystick is not used. 
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(PointerEventData eventData)
    {
        inputDirection = Vector2.zero;
        joystick.rectTransform.anchoredPosition = Vector2.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Not needed. 
    }
}

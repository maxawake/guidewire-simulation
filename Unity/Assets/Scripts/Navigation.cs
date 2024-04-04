using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace vascularIntervention
{
    /// <summary>
    /// This class is responsible for the navigation of the medical equipments (Xray Table, C-Arm, pedal).
    /// </summary>
    public class Navigation : MonoBehaviour
    {
        [SerializeField] private Transform cArmRotationX; // The transform object which is responsible for the rotation of the C-Arm about the x-axis.
        [SerializeField] private Transform cArmRotationZ; // The transform object which is responsible for the rotation of the C-Arm about the z-axis.
        [SerializeField] private Transform xTableTranslationY; // The transform object which is responsible for the rotation of the C-Arm about the z-axis.
        [SerializeField] private Transform xTableTranslationZ; // The transform object which is responsible for the rotation of the C-Arm about the z-axis.

        [SerializeField] private Sprite pedalOffSprite; // Pedal Off sprite.
        [SerializeField] private Sprite pedalOnSprite; // Pedal On sprite.

        [SerializeField] private Sprite newImage; // Sprite which is used to illustrate that the pedal was stepped on.
        [SerializeField] private Image oldImage; // Gameobject with component Image, where the sprite of the Image is changed when activating or deactivating Xray projection.

        [SerializeField] private GameObject pedalOn; // Pedal which is stepped on if xray is activated.
        [SerializeField] private GameObject pedalOff; // Pedal which is not stepped on if xray is deactivated.

        [SerializeField] private Text rotationXText; // Shows the rotation angle about the x-axis on the screen.
        [SerializeField] private Text rotationZText; // Shows the rotation angle about the z-axis on the screen.

        [SerializeField] private Joystick joystickCArm; // Joyatick which is responsible for the rotation of the CArm.
        [SerializeField] private Joystick joystickTable; // Joystick which is responsible for the translation of the XRay table. 

        private bool xrayOn = false; // boolean which checks if xRay is on 
        private bool contrastDyeInserted = false; // boolean which checks if contrast dye was inserted

        // Awake is called when script instance is loaded
        private void Awake()
        {
            Assert.IsNotNull(cArmRotationX, "Transform object cArmRotationX not assigned");
            Assert.IsNotNull(cArmRotationZ, "Transform object cArmRotationZ not assigned");
            Assert.IsNotNull(xTableTranslationY, "Transform object xTableTranslationY not assigned");
            Assert.IsNotNull(xTableTranslationZ, "Transform object xTableTranslationZ not assigned");
    
            Assert.IsNotNull(newImage, "Sprite for pedal stepped on not assigned");
            Assert.IsNotNull(oldImage, "Gameobjectg with component pedal Imga not assigned");

            Assert.IsNotNull(pedalOn, "Gameobject pedal stepped on not assigned");
            Assert.IsNotNull(pedalOff, "Gameobject pedal not stepped on not assigned");

            Assert.IsNotNull(rotationXText, "rotationXText not asssigned");
            Assert.IsNotNull(rotationZText, "rotationZText not asssigned");

            Assert.IsNotNull(joystickCArm, "joystickCArm not assigned");
            Assert.IsNotNull(joystickTable, "joystickTable not assigned");
        }

        // Physics update
        void FixedUpdate()
        {
            Vector2 inputDirectionCArm = joystickCArm.inputDirection; // Vector2 input of joystick for CArm between [-1,1] for each axis.
            Vector2 inputDirectionTable = joystickTable.inputDirection; // Vector2 input of joystick for XRay table between [-1,1] for each axis.
            
            Assert.IsTrue(inputDirectionCArm.x >= -1.0f && inputDirectionCArm.y <= 1.0f, "joystick input of CArm not in intervall [-1,1]");
            Assert.IsTrue(inputDirectionTable.x >= -1.0f && inputDirectionTable.y <= 1.0f, "joystick input of CArm not in intervall [-1,1]");

            float angleZ = Quaternion.ToEulerAngles(cArmRotationZ.rotation).z * Mathf.Rad2Deg;
            float angleX = Quaternion.ToEulerAngles(cArmRotationX.rotation).x * Mathf.Rad2Deg;

            Assert.IsTrue(angleZ >= -90.0f && angleZ < 90.0f, "rotation angle about z axis not between -90° and 90°"); 
            Assert.IsTrue(angleX >= -45.0f && angleX < 45.0f, "rotation angle about x axis not between -45° and 45°");

            /**
             * Send angle and translation data to the XRay Shader
             */
            ShaderProperties.Instance.sendRotationData(angleX, -angleZ, xTableTranslationY.transform.localPosition.y, xTableTranslationZ.transform.localPosition.z);
            /**
             *  yellow display shows the xray projection which is vertical to the one for the blue display
             */
            
            /**
             * Assign angles to the text components
             */
            string rX = "RotationX Angle: " + (int)angleX + "°";
            string rZ = "RotationX Angle: " + (int)angleZ + "°";
            rotationXText.text = rX;
            rotationZText.text = rZ;

            /**
             * Perform rotation of the CArm
             */
            if (inputDirectionCArm != Vector2.zero) // Check if the input direction for the CArm-joystick is not zero.
            {
                float epsilon = 1.0f;
                if (inputDirectionCArm.y > 0.0f) 
                {
                    if (angleZ <= 90 - epsilon)
                    {
                        cArmRotationZ.Rotate(0, 0, inputDirectionCArm.y);
                    }
                }

                if (inputDirectionCArm.y < 0.0f)
                {
                    if (angleZ >= -90 + epsilon)
                    {

                        cArmRotationZ.Rotate(0, 0, inputDirectionCArm.y);
                    }
                }
                if (inputDirectionCArm.x > 0.0f)
                {
                    if (angleX < 45.0f - epsilon)
                    {
                        cArmRotationX.Rotate(inputDirectionCArm.x, 0, 0);
                    }
                }

                if (inputDirectionCArm.x < 0.0f)
                {
                    if (angleX > -45.0f + epsilon)
                    {
                        cArmRotationX.Rotate(inputDirectionCArm.x, 0, 0);
                    }
                }
            }

            /**
             * Perform translation of the XRay Table
             */
            if (inputDirectionTable != Vector2.zero) // Check if the input direction for the CArm-joystick is not zero.
            {
                if (inputDirectionTable.y > 0.0f)
                {
                    if (xTableTranslationY.transform.localPosition.y < 0.2f)
                    {
                        xTableTranslationY.Translate(new Vector3(0, Time.deltaTime * inputDirectionTable.y, 0));
                    }

                }

                if (inputDirectionTable.y < 0.0f)
                {
                    if (xTableTranslationY.transform.localPosition.y > -0.2f) 
                    {
                        xTableTranslationY.Translate(new Vector3(0, Time.deltaTime * inputDirectionTable.y, 0));
                    }
                }

                if (inputDirectionTable.x > 0.0f)
                {  
                    if (xTableTranslationZ.transform.localPosition.z > -0.5f)
                    {
                        xTableTranslationZ.Translate(new Vector3(0, 0, -Time.deltaTime * inputDirectionTable.x));
                    }
                }

                if (inputDirectionTable.x < 0.0f)
                {
                    if (xTableTranslationZ.transform.localPosition.z < 0.5)
                    {
                       xTableTranslationZ.Translate(new Vector3(0, 0, -Time.deltaTime * inputDirectionTable.x));
                    }
                }

            }
        }


        /// <summary>
        /// Listener function which is called if the button to start XRay was clicked
        /// </summary>
        public void OnButtonStartXRayClicked()
        {
            xrayOn = !xrayOn; 

            /**
             * Switch sprites
             */
            Sprite tempImage = newImage;
            newImage = oldImage.sprite;
            oldImage.sprite = tempImage;

            /**
             * Send data to XRay shader
             */
            ShaderProperties.Instance.sendXrayOn(xrayOn);

            /**
             * Activate/Deactive the gameobjects
             */
            pedalOn.SetActive(xrayOn);
            pedalOff.SetActive(!xrayOn);

            if(xrayOn)
                Assert.IsTrue(xrayOn && pedalOn.activeSelf && !pedalOff.activeSelf, "sprites and status are not synchronized");
            else
                Assert.IsTrue(!xrayOn && !pedalOn.activeSelf && pedalOff.activeSelf, "sprites and status are not synchronized");
        }

        /// <summary>
        /// Listener function which is called if the button to insert contrast dye was clicked
        /// </summary>
        public void OnButtonInsertContrastDye()
        {
            contrastDyeInserted = !contrastDyeInserted;

            /**
             * Send data to XRay shader
             */
            ShaderProperties.Instance.sendContrastDyeInserted(contrastDyeInserted);
        }

        /// <summary>
        /// Reset Navigation settings if treatment is finished or canceled.
        /// </summary>
        public void Reset()
        {
            cArmRotationX.rotation = new Quaternion(0,0,0,0);
            cArmRotationZ.rotation = new Quaternion(0,0,0,0);
            xrayOn = false;

            newImage = pedalOffSprite;
            oldImage.sprite = pedalOnSprite;

            contrastDyeInserted = false;
            ShaderProperties.Instance.sendXrayOn(xrayOn);
            ShaderProperties.Instance.sendXrayOn(contrastDyeInserted);
        }
    }
}
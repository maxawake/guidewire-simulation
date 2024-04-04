using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GuidewireSim;
using UnityEngine.UI;
using System.Globalization;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace vascularIntervention
{
    /// <summary>
    /// This class is responsible for accessing the guidewire model. Currently the model of Robin Mader (as a part of his Master thesis) is used. 
    /// To use another guidewire model, the following things have to be modified:
    /// - Change the class of the variable simulationScript.
    /// - Modify the function ActivateGuidewire() and DeactivateGuidewire() (More details are shown in the function description).
    /// - Modify the listener functions insertGuidewire, exitGuidewire, rotateLeftGuidewire, rotateGuideWire, insertExitGuidewireNotPressed and rotateLeftRightGuidewireNotPressed.
    /// - Modify the model parameters by modifying the startoptions menu panel and the update function.
    
    /// Reference to the guidewire model of Robin Mader:
    /// Mader, R. (2023). Simulating guidewires in blood vessels using cosserat rod theory.Master’s thesis, Ruprecht Karl University of Heidelberg, Germany.
    /// </summary>
    public class GuidewireAPI : MonoBehaviour
    {
        [SerializeField] private SimulationLoop simulationScript; // Main script of the guidewire model. Needs to be changed when using another model.
        
        /*
        * Inputs for managing the model parameters. Needs to be changed when using another model.
        */
        public Text inputGuidewireLength;
        public Text inputConstraintSolverSteps;
        public Text inputTimeSteps;
        public Text checkGuidewireLength;
        public Text checkConstraintSolverSteps;
        public Text checkTimeSteps;


        private GuidewireTexture guideWireTexture; // Script for drawing the guidewire in a 3D texture.
        private int numberElements; // Number of elements of the guidewire.
        [SerializeField] private CameraFollow cameraFollow;  // 3D Camera for following the aorta.
        public bool canStartTreatment = true; // Returns true if all parameters in the start options menu were set correctly.
        public bool treatmentActive = false;

        [SerializeField] private Button insertGuideWireButton; 
        [SerializeField] private Button exitGuideWireButton; 
        [SerializeField] private Button rotateLeftGuideWireButton; 
        [SerializeField] private Button rotateRightGuideWireButton;

        // Awake is called when script instance is loaded
        public void Awake()
        {
            guideWireTexture = this.GetComponent<GuidewireTexture>();

            Assert.IsNotNull(simulationScript, "SriptObject of guidewire model not attached.");
            Assert.IsNotNull(cameraFollow, "Scriptobject of CameraFollow not attached.");
            Assert.IsNotNull(guideWireTexture, "GuideWiretexture script not attached.");

            Assert.IsNotNull(insertGuideWireButton, "insert guidewire button not attached.");
            Assert.IsNotNull(exitGuideWireButton, "exit guidewire button not attached.");
            Assert.IsNotNull(rotateLeftGuideWireButton, "rotate guidewire left button not attached.");
            Assert.IsNotNull(rotateRightGuideWireButton, "rotate guidewire right button not attached.");
            
            addListeners(); // Add Listeners for navigating the guidewire. The listener functions may need to be changed if another guidewire model is used.
        }

        /// <summary>
        /// Activate the guidewire. The main scrpt of the guidewire model must contain the following functions and attributes:
        /// void createGuidewire(int n): Creates a guidewire with n elements.
        /// void initialize(): Initialize the model.
        /// bool activatad: Activate/Deactivate the simulation loop. 
        /// </summary>
        public void ActivateGuidewire()
        {
            simulationScript.createGuidewire(numberElements);

            /*
             * Send positions of guidewire elements to the GuidewireTexture script.
             */
            Vector3[] guidewirePositions = new Vector3[numberElements];
            for (int i = 0; i < numberElements; ++i)
            {
                guidewirePositions[i] = simulationScript.spheres[i].transform.position; // Needs to be changed if another model is used.
            }
            guideWireTexture.setGuidewirePositions(guidewirePositions);

            simulationScript.initialize();
            simulationScript.activated = true;

            /*
             * Change the position of the aorta model so that the last element of the guidewire is at the start position of the aorta model.
             * By default, the start position of the aorta model is at (0,0,0);
             */
            this.GetComponent<TreatmentOptions>().changePositionOfAorta(new Vector3(0, 0, numberElements * 10));

            /*
             * Set target of camera by choosing the last element of the guidewire.
             * Needs to be changed if another model is used.
             */
            cameraFollow.setTarget(simulationScript.spheres[simulationScript.SpheresCount - 1]); 
        }

        /// <summary>
        /// Deactivate guidewire. The main scrpt of the guidewire model must contain the following functions and attributes:
        /// void destroyGuideWire: Destroy the current guidewire.
        /// bool activatad: Activate/Deactivate the simulation loop. 
        /// </summary>
        public void DeactivateGuidewire()
        {
            simulationScript.activated = false;
            simulationScript.destroyGuideWire();
            Vector3[] guideWirePositions = new Vector3[0];
            guideWireTexture.setGuidewirePositions(guideWirePositions);
        }


        private void Update()
        {
            canStartTreatment = true;

            int guidewireLength;
            if (int.TryParse(inputGuidewireLength.text, out guidewireLength))
            {
                if (guidewireLength > 2)
                {
                    checkGuidewireLength.color = new Color(0, 1, 0, 1);
                    numberElements = guidewireLength;
                }
                else
                {
                    checkGuidewireLength.color = new Color(1, 0, 0, 1);
                    canStartTreatment = false;
                }
            }
            else
            {
                checkGuidewireLength.color = new Color(1, 0, 0, 1);
                canStartTreatment = false;
            }

            int constraintSolverSteps;
            if (int.TryParse(inputConstraintSolverSteps.text, out constraintSolverSteps))
            {
                if (constraintSolverSteps >= 1 && constraintSolverSteps <= 1000)
                {
                    checkConstraintSolverSteps.color = new Color(0, 1, 0, 1);
                    simulationScript.ConstraintSolverSteps = constraintSolverSteps;
                }
                else
                {
                    checkConstraintSolverSteps.color = new Color(1, 0, 0, 1);
                    canStartTreatment = false;
                }
            }
            else
            {
                checkConstraintSolverSteps.color = new Color(1, 0, 0, 1);
                canStartTreatment = false;
            }

            float timeStep;
            if (float.TryParse(inputTimeSteps.text, out timeStep))
            {
                timeStep = float.Parse(inputTimeSteps.text, CultureInfo.InvariantCulture.NumberFormat);
                if (timeStep >= 0.002f && timeStep <= 0.04f)
                {
                    checkTimeSteps.color = new Color(0, 1, 0, 1);
                    simulationScript.TimeStep = timeStep;
                }
                else
                {
                    checkTimeSteps.color = new Color(1, 0, 0, 1);
                    canStartTreatment = false;
                }
            }
            else
            {
                checkTimeSteps.color = new Color(1, 0, 0, 1);
                canStartTreatment = false;
            }
        }

        /// <summary>
        /// Add Listener functions to the navigation buttons for the guidewire.
        /// </summary>
        private void addListeners()
        {
            /*
             * Event triggers for the buttons to moving and rotating the guidewire.
             */
            EventTrigger insertGuideWireButtonTrigger = insertGuideWireButton.gameObject.AddComponent<EventTrigger>();
            EventTrigger exitGuideWireButtonTrigger = exitGuideWireButton.gameObject.AddComponent<EventTrigger>();
            EventTrigger rotateLeftGuideWireButtonTrigger = rotateLeftGuideWireButton.gameObject.AddComponent<EventTrigger>();
            EventTrigger rotateRightGuideWireButtonTrigger = rotateRightGuideWireButton.gameObject.AddComponent<EventTrigger>();

            /*
             * Create pointDown event for insertGuidewireButton. 
             */
            var pointerDownInsertGuideWire = new EventTrigger.Entry();
            pointerDownInsertGuideWire.eventID = EventTriggerType.PointerDown;
            pointerDownInsertGuideWire.callback.AddListener((e) => insertGuidewire());

            /*
             * Create pointerDown event for exitGuidewireButton. 
             */
            var pointerDownExitGuideWire = new EventTrigger.Entry();
            pointerDownExitGuideWire.eventID = EventTriggerType.PointerDown;
            pointerDownExitGuideWire.callback.AddListener((e) => exitGuidewire());

            /*
             * Create pointerUp event for insertGuidewireButton and exitGuidewireButton.
             */
            var pointerUpInsertExitGuideWire = new EventTrigger.Entry();
            pointerUpInsertExitGuideWire.eventID = EventTriggerType.PointerUp;
            pointerUpInsertExitGuideWire.callback.AddListener((e) => insertExitGuidewireNotPressed());


            /*
            * Create pointDown event for rotateLeftGuidewireButton. 
            */
            var pointerDownRotateLeftGuideWire = new EventTrigger.Entry();
            pointerDownRotateLeftGuideWire.eventID = EventTriggerType.PointerDown;
            pointerDownRotateLeftGuideWire.callback.AddListener((e) => rotateLeftGuidewire());

            /*
             * Create pointerDown event for rotateRightGuidewireButton. 
             */
            var pointerDownRotateRightGuideWire = new EventTrigger.Entry();
            pointerDownRotateRightGuideWire.eventID = EventTriggerType.PointerDown;
            pointerDownRotateRightGuideWire.callback.AddListener((e) => rotateRightGuidewire());

            /*
             * Create pointerUp event for insertGuidewireButton and exitGuidewireButton.
             */
            var pointerUpRotateLeftRightGuideWire = new EventTrigger.Entry();
            pointerUpRotateLeftRightGuideWire.eventID = EventTriggerType.PointerUp;
            pointerUpRotateLeftRightGuideWire.callback.AddListener((e) => rotateLeftRightGuidewireNotPressed());


            /*
             * Add events to the triggers.
             */
            insertGuideWireButtonTrigger.triggers.Add(pointerDownInsertGuideWire);
            insertGuideWireButtonTrigger.triggers.Add(pointerUpInsertExitGuideWire);

            exitGuideWireButtonTrigger.triggers.Add(pointerDownExitGuideWire);
            exitGuideWireButtonTrigger.triggers.Add(pointerUpInsertExitGuideWire);

            rotateLeftGuideWireButtonTrigger.triggers.Add(pointerDownRotateLeftGuideWire);
            rotateLeftGuideWireButtonTrigger.triggers.Add(pointerUpRotateLeftRightGuideWire);

            rotateRightGuideWireButtonTrigger.triggers.Add(pointerDownRotateRightGuideWire);
            rotateRightGuideWireButtonTrigger.triggers.Add(pointerUpRotateLeftRightGuideWire);
        }


        /// <summary>
        /// Listener function to insert the guidewire. Needs to be changed if another model is used.
        /// </summary>
        public void insertGuidewire()
        {
            simulationScript.setVelocity(2.0f);
            simulationScript.sphereExternalForces[simulationScript.SpheresCount - 1] = new Vector3(0, 0, 5);
        }

        /// <summary>
        /// Listener function to exit the guidewire. Needs to be changed if another model is used.
        /// </summary>
        public void exitGuidewire()
        {
            simulationScript.setVelocity(2.0f);
            simulationScript.sphereExternalForces[0] = new Vector3(0, 0, -5);
        }

        /// <summary>
        /// Listener function to rotate the guidewire left. Needs to be changed if another model is used.
        /// </summary>
        public void rotateLeftGuidewire()
        {
            simulationScript.setAngularVelocity(1.0f);
            simulationScript.setVelocity(1.0f);
            simulationScript.cylinderExternalTorques[0] = new Vector3(0f, 0f, 0.3f);
        }

        /// <summary>
        /// Listener function to rotate the guidewire right. Needs to be changed if another model is used.
        /// </summary>
        public void rotateRightGuidewire()
        {
            simulationScript.setAngularVelocity(1.0f);
            simulationScript.setVelocity(1.0f);
            simulationScript.cylinderExternalTorques[0] = new Vector3(0f, 0f, 0.0f);
        }

        /// <summary>
        /// Listener function to stop movement (insert,exit) of the guidewire. Needs to be changed if another model is used.
        /// </summary>
        public void insertExitGuidewireNotPressed()
        {
            simulationScript.setVelocity(0.0f);
            for (int sphereIndex = 0; sphereIndex < simulationScript.SpheresCount; ++sphereIndex)
            {
                simulationScript.sphereExternalForces[sphereIndex] = new Vector3(0, 0, 0);
            }
        }

        /// <summary>
        /// Listener function to stop rotation (left,right) of the guidewire. Needs to be changed if another model is used.
        /// </summary>
        public void rotateLeftRightGuidewireNotPressed()
        {
            simulationScript.setAngularVelocity(0.0f);
            simulationScript.setVelocity(0.0f);
            for (int cylinderIndex = 0; cylinderIndex < (simulationScript.CylinderCount); cylinderIndex++)
            {
                simulationScript.cylinderExternalTorques[cylinderIndex] = Vector3.zero;
            }
        }
    }

}
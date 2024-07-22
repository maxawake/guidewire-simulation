using Codice.Client.BaseCommands;
using GuidewireSim;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.UIElements;
public class CameraController : MonoBehaviour {

    public float moveSpeed = 100f;
    public float rotationSpeed = 100.0f;

    SimulationLoop simulationLoop;

    private void Awake() {
        simulationLoop = FindAnyObjectByType<SimulationLoop>();
        Assert.IsNotNull(simulationLoop);
    }

    void Update () {
        if (!simulationLoop.Logging) {
            Translate();
            Rotate();
        }
    }

    void Translate() {
        Vector3 inputDir = new Vector3(0.0f, 0.0f, 0.0f);

        if (Input.GetKey(KeyCode.W)) {
            inputDir.z += 1.0f;
        }
        if (Input.GetKey(KeyCode.S)) {
            inputDir.z -= 1.0f;
        }
        if (Input.GetKey(KeyCode.A)) {
            inputDir.x -= 1.0f;
        }
        if (Input.GetKey(KeyCode.D)) {
            inputDir.x += 1.0f;
        }

        Vector3 moveDir = transform.forward * inputDir.z + transform.right * inputDir.x;
        transform.position += moveSpeed * moveDir.normalized * Time.deltaTime;
    }

    void Rotate() {
        float rotateDir = 0.0f;
        if (Input.GetKey(KeyCode.Q)) {
            rotateDir -= 1.0f;
        }
        if (Input.GetKey(KeyCode.E)) {
            rotateDir += 1.0f;
        }

        transform.eulerAngles += new Vector3(0.0f, rotateDir * rotationSpeed * Time.deltaTime, 0.0f);
    }
}
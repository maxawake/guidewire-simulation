using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GuidewireSim
{
/**
 * This class represents each orientation by drawing all of its directors as arrows in each frame.
 */
public class DirectorsDrawer : MonoBehaviour
{
    SimulationLoop simulationLoop; //!< The component SimulationLoop.

    [SerializeField] [Range(0.1f, 50f)] float scaleFactor; //!< The scale factor that gets multiplied to the length of the respective director.
    [SerializeField] [Range(1f, 90f)] float arrowHeadAngle; //!< The angle spread of the arrow head.
    [SerializeField] [Range(0.1f, 1f)] float arrowHeadPercentage; //!< The percentage of the length of the arrow that the arrow head covers.
    Color directorOneColor = Color.green; //!< The color that the lines representing the first director are drawn with.
    Color directorTwoColor = Color.blue; //!< The color that the lines representing the second director are drawn with.
    Color directorThreeColor = Color.red; //!< The color that the lines representing the third director are drawn with.
    Color[] directorColors = new Color[3] {Color.red, Color.green, Color.blue}; /**< The color that the lines representing the
                                                                                 *   three directors are drawn with.
                                                                                 *   @note The i-th director is drawn in the i-th Color.
                                                                                 */

    private void Awake()
    {
        simulationLoop = GetComponent<SimulationLoop>();
        Assert.IsNotNull(simulationLoop);
    }

    void Update()
    {
        DrawDirectors(simulationLoop.cylinderPositions, simulationLoop.directors);
    }

    /**
     * Draws the director basis of each orientation element as arrows.
     * @param cylinderPositions The center of mass of each cylinder, i.e. the position of each orientation element.
     * @param directors The orthonormal basis of each orientation element / cylinder, also called directors.
     */
    private void DrawDirectors(Vector3[] cylinderPositions, Vector3[][] directors)
    {
        // iterates over each orientation element
        for (int cylinderIndex = 0; cylinderIndex < cylinderPositions.Length; cylinderIndex++)
        {
            Vector3 startPosition = cylinderPositions[cylinderIndex];

            // iterates over each director
            for (int directorIndex = 0; directorIndex < 3; directorIndex++)
            {
                Vector3 endPosition = startPosition + scaleFactor * directors[directorIndex][cylinderIndex];

                // Draws a line between the cylinder's center of mass and the director's end position.
                Debug.DrawLine(startPosition, endPosition, directorColors[directorIndex]);

                Vector3[] arrowHeadPositions = CalculateArrowHeadPositions(startPosition, endPosition);                    
                DrawArrowHeadLines(directorIndex, endPosition, arrowHeadPositions);
                DrawArrowHeadConnectionLines(directorIndex, arrowHeadPositions);
            }
        }
    }

    /**
     * Calculates the end position of each line of each arrow head. E.g. an arrow head consists of four lines, each of them starting at
     * @p endPosition and spreading in different directions to form the shape of an arrow tip.
     * @param startPosition The start position of the director, i.e. the position of the orientation.
     * @param endPosition The position of the tip of the arrow head.
     * @return The end positions of the four lines that form the arrow head.
     * @req @p arrowHeadPositions has a length of 4.
     */
    private Vector3[] CalculateArrowHeadPositions(Vector3 startPosition, Vector3 endPosition)
    {
        Vector3[] arrowHeadPositions = new Vector3[4];
        Vector3 direction = endPosition - startPosition;

        arrowHeadPositions[0] = endPosition + Quaternion.LookRotation(direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * Vector3.back * arrowHeadPercentage;
        arrowHeadPositions[1] = endPosition + Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back * arrowHeadPercentage;
        arrowHeadPositions[2] = endPosition + Quaternion.LookRotation(direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * Vector3.back * arrowHeadPercentage;
        arrowHeadPositions[3] = endPosition + Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back * arrowHeadPercentage;

        Assert.IsTrue(arrowHeadPositions.Length == 4);

        return arrowHeadPositions;
    }

    /**
     * Draws the four lines that form the arrow head for the director that corresponds to @p directorIndex.
     * @param directorIndex The index of the director under consideration.
     * @param endPosition The position of the tip of the arrow head.
     * @param arrowHeadPositions The end positions of the four lines that form the arrow head.
     * @req @p arrowHeadPositions has a length of 4.
     */
    private void DrawArrowHeadLines(int directorIndex, Vector3 endPosition, Vector3[] arrowHeadPositions)
    {
        Assert.IsTrue(arrowHeadPositions.Length == 4);

        for (int positionIndex = 0; positionIndex < arrowHeadPositions.Length; positionIndex++)
        {
            Debug.DrawLine(endPosition, arrowHeadPositions[positionIndex], directorColors[directorIndex]);
        }
    }

    /**
     * Draws the four lines that connect the arrow head tips with each other. E.g. draws the line from @p arrowHeadPositions 0 and @p arrowHeadPositions 1.
     * @param directorIndex The index of the director under consideration.
     * @param arrowHeadPositions The end positions of the four lines that form the arrow head.
     */
    private void DrawArrowHeadConnectionLines(int directorIndex, Vector3[] arrowHeadPositions)
    {
        Debug.DrawLine(arrowHeadPositions[0], arrowHeadPositions[1], directorColors[directorIndex]);
        Debug.DrawLine(arrowHeadPositions[1], arrowHeadPositions[2], directorColors[directorIndex]);
        Debug.DrawLine(arrowHeadPositions[2], arrowHeadPositions[3], directorColors[directorIndex]);
        Debug.DrawLine(arrowHeadPositions[3], arrowHeadPositions[0], directorColors[directorIndex]);
    }
    }
}
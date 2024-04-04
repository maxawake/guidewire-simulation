using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;

namespace vascularIntervention
{
    /// <summary>
    /// This class is responsible for drawing the guidewire in a 3D texture which will be passed to the XRay Shader for the XRay simulation.
    /// The positions of the elements of the guidewire are set in the GuidewireAPI class. If the distances of the elements are too far away than 
    /// the catmull-rom spline interpolation can be used, otherwise only the positions will be drawn. 
    /// </summary>
    public class GuidewireTexture : MonoBehaviour
    {
        [SerializeField] private Texture3D guidewireTexture; // 3D texture which contains the guidewire.

        private Vector3 aortaPosition; // Transform of the aorta.
        private Vector3[] guidewirePositions; // Guidewire positions.
        private Color[] defaultColors; // default colors to clear the 3D texture.

        public bool useCatmullRomSplineInterpolation = false;

        // Awake is called when script instance is loaded
        private void Awake()
        {
            Assert.IsNotNull(guidewireTexture, "3D Texture for guidewire not assigned");
        }

        // Start is called before the first frame update
        private void Start()
        {
            guidewirePositions = new Vector3[0]; 

            /*
             * Create an array of default colors.
             */
            defaultColors = new Color[guidewireTexture.width * guidewireTexture.height * guidewireTexture.depth];
            for (int i = 0; i < guidewireTexture.width * guidewireTexture.height * guidewireTexture.depth; ++i)
            {
                defaultColors[i] = new Color(0, 0, 0, 0);
            }
        }

        // Physics update
        private void FixedUpdate()
        {
            guidewireTexture.SetPixels(defaultColors); // Clear the texture.

            if (useCatmullRomSplineInterpolation) // Perform catmull-rom spline interpolation.
            {
                interpolateCatmullRomSpline();
            }
            else // Draw only the guidewire positions.
            {
                for (int i = 0; i < guidewirePositions.Length; ++i)
                {
                    int[] textureIndices = computeTextureIndices(guidewirePositions[i]);

                    if (textureIndices[0] > 0 && textureIndices[0] < guidewireTexture.width &&
                        textureIndices[1] > 0 && textureIndices[1] < guidewireTexture.height &&
                        textureIndices[2] > 0 && textureIndices[2] < guidewireTexture.width) // Check if texture indices are in the valid range.
                    {
                        guidewireTexture.SetPixel(textureIndices[0], textureIndices[1], textureIndices[2], new Color(1, 0, 0, 1)); // Assign an attenuation mu=1 to the guidewire position.
                    }
                }
            }
            guidewireTexture.Apply();
        }

        /// <summary>
        /// This function correct the guidewire positons because the aorta was moved and scaled, and returnsthe 3D texture indices.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        int[] computeTextureIndices(Vector3 position)
        {
            /*
             * Correct the guidewire positions.
             */
            Vector3 correctedPosition = new Vector3();
            correctedPosition.x = (position.x - aortaPosition.x) / 2000.0f;
            correctedPosition.y = (position.y - aortaPosition.y) / 2000.0f;
            correctedPosition.z = (position.z - aortaPosition.z) / 2000.0f;

            Vector3 texturePosition = new Vector3();

            /*
             * Transform positions to texture indices.
             */
            int[] textureIndices = new int[3];
            texturePosition[0] = (int)(correctedPosition.x + 0.5f) * (guidewireTexture.width - 1.0f);
            texturePosition[1] = (int)(correctedPosition.y + 0.5f) * (guidewireTexture.height - 1.0f);
            texturePosition[2] = (int)(correctedPosition.z + 0.5f) * (guidewireTexture.depth - 1.0f);

            return textureIndices;
        }

        /// <summary>
        /// Compute the derivatives for the positions of the guide wire elements. The formula is: 
        /// d_i = 0.5 * p_(i+1) - p(i-1) for i = 1,...,N-1, 
        /// where N is the number of positions and d_0 = d_1, d_(N-1) = d(N).
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="slopes"></param>
        /// <returns></returns>
        Vector3[] computeDerivatives(Vector3[] positions)
        {
            Vector3[] d = new Vector3[positions.Length];

            for (int i = 1; i < positions.Length - 1; ++i)
            {
                d[i] = 0.5f * (positions[i + 1] - positions[i - 1]);
            }
            d[0] = d[1];
            d[positions.Length - 1] = d[positions.Length - 2];

            return d;
        }

        /// <summary>
        /// Draw the guidewire by using the catmull-rom spline.
        /// The path between two positions can be computed by the following formula:
        /// p(t) = h_00 + p_0 + h_10 * p_1 + h_01 * d_0 + h_11 * d_1 , 
        /// where t in [0,1], p_0 and p_1 are the positions,
        /// h_00 = 2 * t^3 - 3 * t^2 + 1,
        /// h_10 = -2 * t^3 + 3 * t^2
        /// h_01 = t^3 - 2 * t^2 + t
        /// t^3 - t^2
        /// and d_0, d_1 are the derivatives, which are computed in the function computeDerivatives
        /// </summary>
        public void interpolateCatmullRomSpline()
        {
            for (int index = 0; index < guidewirePositions.Length - 1; ++index) // iterate over all positions
            {
                for (float i = 0; i <= 1.0f; i = i + 0.01f)
                {
                    Vector3[] d = computeDerivatives(guidewirePositions);

                    float iexp2 = i * i;
                    float iexp3 = iexp2 * i;

                    float h00 = 2.0f * iexp3 - 3.0f * iexp2 + 1.0f;
                    float h10 = -2.0f * iexp3 + 3.0f * iexp2;
                    float h01 = iexp3 - 2 * iexp2 + i;
                    float h11 = iexp3 - iexp2;

                    Vector3 interpolatedPosition = h00 * guidewirePositions[index] + h10 * guidewirePositions[index + 1] + h01 * d[index] + h11 * d[index + 1];

                    int[] textureIndices = computeTextureIndices(interpolatedPosition);
                    if (textureIndices[0] > 0 && textureIndices[0] < guidewireTexture.width &&
                        textureIndices[1] > 0 && textureIndices[1] < guidewireTexture.height &&
                        textureIndices[2] > 0 && textureIndices[2] < guidewireTexture.width) // Check if texture indices are in the valid range.
                    {
                        guidewireTexture.SetPixel(textureIndices[0], textureIndices[1], textureIndices[2], new Color(1, 0, 0, 1)); // Assign an attenuation mu=1 to the guidewire position.
                    }
                }
            }
        }

        /// <summary>
        /// Set the guidewire positions.
        /// </summary>
        /// <param name="positions"></param>
        public void setGuidewirePositions(Vector3[] positions)
        {
            guidewirePositions = positions;
        }

        /// <summary>
        /// Set the position of the aorta.
        /// </summary>
        /// <param name="position"></param>
        public void setAortaPosition(Vector3 position)
        {
            aortaPosition = position;
        }
    }
}
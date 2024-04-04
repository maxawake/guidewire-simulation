using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace vascularIntervention {
    /// <summary>
    /// This singleton class is responsible for sending data to the XRay Shader.
    /// </summary>
    public class ShaderProperties: MonoBehaviour
    {
        public static ShaderProperties Instance { get; private set; } // Instance for accessing the data.

        /**
         * XRay shader materials to display two Xray projections which are orthogonal.
         */
        private Material materialBlue;
        private Material materialYellow;

        /**
         * Displays containing the materials.
         */
        public GameObject displayBlue;
        public GameObject displayYellow;


        /**
         * Shader properties access variables.
         */
        public const string textureBody = "_TextureBody";
        public const string textureAorta = "_TextureAorta";
        public const string textureGuidewire = "_TextureGuidewire";
        public const string textureTransferfunction = "_Transferfunction";

        public const string transformationMatrix = "_TransformationMatrix";

        public const string TranslationZ = "_TranslationZ";
        public const string TranslationY = "_TranslationY";

        public const string xrayOn = "_xrayOn";
        public const string contrastDyeInserted = "_contrastDyeInserted";
        public const string guideWireInserted = "_guideWireInserted";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            
            materialBlue = displayBlue.GetComponent<MeshRenderer>().material;
            materialYellow = displayYellow.GetComponent<MeshRenderer>().material;
        }

        /// <summary>
        /// Send rotation data to the XRay shader.
        /// </summary>
        /// <param name="rotationX"></param>
        /// <param name="rotationZ"></param>
        public void sendRotationData(float rotationX, float rotationZ, float translationY, float translationZ)
        {
            float sinX = Mathf.Sin(Mathf.Deg2Rad * rotationX);
            float cosX = Mathf.Cos(Mathf.Deg2Rad * rotationX);

            float sinZ = Mathf.Sin(Mathf.Deg2Rad * rotationZ);
            float cosZ = Mathf.Cos(Mathf.Deg2Rad * rotationZ);

            float sinZOrthogonal = Mathf.Sin(Mathf.Deg2Rad * (rotationZ-90.0f));
            float cosZOrthogonal = Mathf.Cos(Mathf.Deg2Rad * (rotationZ-90.0f));

            Matrix4x4 translationMatrix = new Matrix4x4(new Vector4(1, 0, 0, 0),
                                                      new Vector4(0, 1, 0, translationY),
                                                      new Vector4(0, 0, 1, translationZ),
                                                      new Vector4(0, 0, 0, 1));

            Matrix4x4 rotationMatrixZ = new Matrix4x4(new Vector4(cosZ, sinZ, 0, 0),
                                                      new Vector4(-sinZ, cosZ, 0, 0),
                                                      new Vector4(0, 0, 1, 0),
                                                      new Vector4(0, 0, 0, 1));

            Matrix4x4 rotationMatrixZOrthogonal = new Matrix4x4(new Vector4(cosZOrthogonal, sinZOrthogonal, 0, 0),
                                          new Vector4(-sinZOrthogonal, cosZOrthogonal, 0, 0),
                                          new Vector4(0, 0, 1, 0),
                                          new Vector4(0, 0, 0, 1));

            Matrix4x4 rotationMatrixX = new Matrix4x4(new Vector4(1, 0, 0, 0),
                                                      new Vector4(0, cosX, sinX, 0),
                                                      new Vector4(0, -sinX, cosX, 0),
                                                      new Vector4(0, 0, 0, 1));

            Matrix4x4 transformationMatrix = translationMatrix * rotationMatrixZ * rotationMatrixX;
            Matrix4x4 transformationMatrixOrthogonal = translationMatrix * rotationMatrixZOrthogonal * rotationMatrixX;

            materialBlue.SetMatrix(ShaderProperties.transformationMatrix, transformationMatrix);
            materialYellow.SetMatrix(ShaderProperties.transformationMatrix, transformationMatrixOrthogonal); // Yellow Display show orthogonal XRay Projection.
        }

        /// <summary>
        /// Tells the shader if XRay is activated or not.
        /// </summary>
        /// <param name="xrayOn"></param>
        public void sendXrayOn(bool xrayOn)
        {
            materialYellow.SetInt(ShaderProperties.xrayOn, xrayOn ? 1 : 0);
            materialBlue.SetInt(ShaderProperties.xrayOn, xrayOn ? 1 : 0);
        }

        /// <summary>
        /// Tells the shader if contrast dye is inserted or not.
        /// </summary>
        /// <param name="contrastDyeInserted"></param>
        public void sendContrastDyeInserted(bool contrastDyeInserted)
        {
            materialYellow.SetInt(ShaderProperties.contrastDyeInserted, contrastDyeInserted ? 1 : 0);
            materialBlue.SetInt(ShaderProperties.contrastDyeInserted, contrastDyeInserted ? 1 : 0);
        }

        /// <summary>
        /// Send CT Data(3D Textures for body and aorta).
        /// </summary>
        /// <param name="body"></param>
        /// <param name="aorta"></param>
        public void sendCTTextures(Texture body, Texture aorta)
        {
            materialBlue.SetTexture(textureBody, body);
            materialBlue.SetTexture(textureAorta, aorta);
            materialYellow.SetTexture(textureBody, body);
            materialYellow.SetTexture(textureAorta, aorta);
        }
    }
}

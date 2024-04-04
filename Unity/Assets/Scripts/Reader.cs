using UnityEngine;
using System.IO;
using itk.simple;
using System.Linq;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Assertions;

namespace vascularIntervention
{
    /// <summary>
    /// This class is responsible for reading .nrrd files. 
    /// </summary>
    public class Reader
    {
        /// <summary>
        /// This function reads the cell data from the .nrrd file by using the itk library and creates a three dimensional texture.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Texture3D CreateTextureFromNRRD(string path)
        {
            Assert.IsTrue(File.Exists(path), "Path: + " + path + " to .nrrd file does not exists.");
            ImageFileReader imageFile = new ImageFileReader(); // Class from the itk library to read the .nrrd file.
            imageFile.SetFileName(path);
            Image image = imageFile.Execute(); // Image saves the volume data.

            var dimension = image.GetDimension();
            int depth = (int)image.GetDepth();
            int height = (int)image.GetHeight();
            int width = (int)image.GetWidth();

            Assert.IsTrue(dimension > 0, "dimension of volume is zero.");

            const TextureFormat format = TextureFormat.Alpha8; // Since the voxel values are the intensities, the alpha channel is enough for the 3D texture.

            MinimumMaximumImageFilter minmax = new MinimumMaximumImageFilter(); // Filter class from the itk library to determine the maximum and minimum intensity.
            minmax.Execute(image);
            float min = (float)(minmax.GetMinimum());
            float max = (float)(minmax.GetMaximum());


            Texture3D tex = new Texture3D(width, height, depth, format, false);
            Color[] colors;

            // The if-statement checks if the voxel date of the volume is of type 8-bit unsigned integer or 16-bit unsigned integer.
            if (image.GetPixelID() == PixelIDValueEnum.sitkUInt8)
            {
                /**
                 * Create byte array and convert it to a color array 
                 */
                IntPtr buffer = image.GetBufferAsUInt8();
                int bufferSize = width * depth * height * (int)image.GetNumberOfComponentsPerPixel();
                byte[] imageData = new byte[bufferSize];
                Marshal.Copy(buffer, imageData, 0, imageData.Length);
                colors = imageData.Select(b => new Color(0, 0, 0, b / max)).ToArray();
            }
            else
            {
                /**
                * Create byte array and convert it to a color array. Because the voxel data is of type 16-bit unsigned integer we have to convert
                * the byte array to an ushort array at first.
                */
                IntPtr buffer = image.GetBufferAsUInt16();
                int bufferLength = width * height * depth * sizeof(ushort);
                byte[] imageData = new byte[bufferLength];
                Marshal.Copy(buffer, imageData, 0, imageData.Length);
                ushort[] pixels = new ushort[width * height * depth];
                Buffer.BlockCopy(imageData, 0, pixels, 0, bufferLength);
                colors = pixels.Select(b => new Color(0, 0, 0, b / max)).ToArray();
            }

            /**
             * Update the texture.
             */
            tex.SetPixels(colors);
            tex.Apply();

            return tex;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using BSM = BulletSharp.Math;

namespace GuidewireSim
{
/**
 * This class provides various helper methods for calculation.
 */
public class MathHelper : MonoBehaviour
{
    /**
     * Calculates @p cylinderPositions as the middle points of two adjacent spheres.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param spherePositions The position at the current frame of each sphere.
     * @param cylinderPositions The position/ center of mass of each cylinder.
     * @note @p cylinderPositions is not marked as an out parameter, since @p cylinderPositions is not initialized in this method, but its values
     * are changed.
     */
    public void CalculateCylinderPositions(int cylinderCount, Vector3[] spherePositions, Vector3[] cylinderPositions)
    {
        for (int cylinderIndex = 0; cylinderIndex < cylinderCount; cylinderIndex++)
        {
            cylinderPositions[cylinderIndex] = (spherePositions[cylinderIndex] + spherePositions[cylinderIndex + 1]) / 2;
        }
    }

    /**
     * Calculates the multiplication of \f$ M x \f$, where \f$ M \f$ is the @p matrix, and \f$ x \f$ is the @p vector input.
     * @param matrix The matrix to be multiplied with the vector.
     * @param vector The vector to be multiplied with the matrix.
     * @return The multiplication \f$ M x \f$.
     * @req @p matrix must be a \f$ 3 \times 3 \f$ matrix.
     */
    public Vector3 MatrixVectorMultiplication(float[,] matrix, Vector3 vector)
    {
        // must be 3x3 matrix
        Assert.IsTrue(matrix.GetLength(0) == 3);
        Assert.IsTrue(matrix.GetLength(1) == 3);

        Vector3 result = new Vector3();
        for (int index = 0; index < 3; index++)
        {
            result[index] = matrix[index, 0] * vector[index] + matrix[index, 1] * vector[1] + matrix[index, 2] * vector[2];
        }

        return result;
    }

    /**
     * Calculates the multiplication of \f$ x^{T} M \f$, where \f$ M \f$ is the @p matrix, and \f$ x^{T} \f$ is the @p columnVector input.
     * @param matrix The matrix to be multiplied with the vector.
     * @param vector The column vector \f$ x^{T} \f$ to be multiplied with the matrix.
     * @return The multiplication \f$ x^{T} M \f$.
     * @req @p matrix must be a \f$ 3 \times 3 \f$ matrix.
     * @attention The input vector and the output vector are both column vectors.
     */
    public Vector3 ColumnVectorMatrixMultiplication(Vector3 columnVector, float[,] matrix)
    {
        // input must be 3x3 matrix
        Assert.IsTrue(matrix.GetLength(0) == 3);
        Assert.IsTrue(matrix.GetLength(1) == 3);

        Vector3 result = new Vector3();
        for (int index = 0; index < 3; index++)
        {
            result[index] = columnVector[0] * matrix[0, index] + columnVector[1] * matrix[1, index] + columnVector[2] * matrix[2, index];
        }

        return result;
    }

    /**
     * Calculates the multiplication of \f$ x v^{T} \f$, where \f$ x \f$ is a (row) vector , and \f$ v^{T} \f$ is a column vector.
     * @param vectorOne The (row) vector to be multiplied.
     * @param columnVectorTwo The column vector \f$ x^{T} \f$ to be multiplied.
     * @return The resulting \f$ 3 \times 3 \f$ matrix of the multiplication \f$ x v^{T} \f$.
     */
    public float[,] VectorColumnVectorMultiplication(Vector3 vectorOne, Vector3 columnVectorTwo)
    {
        float[,] result = new float[3,3];
        for (int rowIndex = 0; rowIndex < 3; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < 3; columnIndex++)
            {
                result[rowIndex, columnIndex] = vectorOne[rowIndex] * columnVectorTwo[columnIndex];
            }
        }

        return result;
    }

    /**
     * Calculates the multiplication of \f$ a M \f$, where \f$ a \f$ is a scalar , and \f$ M \f$ is a \f$ 3 \times 3 \f$ matrix.
     * @param scalar The scalar to be multiplied.
     * @param matrix The matrix \f$ M \f$ to be multiplied.
     * @return The resulting \f$ 3 \times 3 \f$ matrix of the multiplication \f$ a M \f$.
     */
    public float[,] ScalarMatrixMultiplication(float scalar, float[,] matrix)
    {
        // input must be 3x3 matrix
        Assert.IsTrue(matrix.GetLength(0) == 3);
        Assert.IsTrue(matrix.GetLength(1) == 3);

        float[,] result = new float[3,3];
        for (int rowIndex = 0; rowIndex < 3; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < 3; columnIndex++)
            {
                result[rowIndex, columnIndex] = scalar * matrix[rowIndex, columnIndex];
            }
        }

        return result;
    }

    /**
     * Calculates the inverse of a \f$ 3 \times 3 \f$ matrix \f$ M \f$.
     * @param matrix The matrix to 
     * @param columnVectorTwo The column vector \f$ x^{T} \f$ to be multiplied.
     * @return The resulting \f$ 3 \times 3 \f$ matrix of the multiplication \f$ x v^{T} \f$.
     */
    public float[,] MatrixInverse(float[,] matrix)
    {
        // input must be 3x3 matrix
        Assert.IsTrue(matrix.GetLength(0) == 3);
        Assert.IsTrue(matrix.GetLength(1) == 3);

        float determinant = matrix[0,0] * (matrix[2,2] * matrix[1,1] - matrix[2,1] * matrix[1,2])
                            - matrix[1,0] * (matrix[2,2] * matrix[0,1] - matrix[2,1] * matrix[0,2])
                            + matrix[2,0] * (matrix[1,2] * matrix[0,1] - matrix[1,1] * matrix[0,2]);
        Debug.Log("determinant" + determinant);
        float inverseDeterminant = 1f / determinant;

        float[,] tempMatrix = new float[3,3];
        tempMatrix[0,0] = matrix[2,2] * matrix[1,1] - matrix[2,1] * matrix[1,2];
        tempMatrix[0,1] = -(matrix[2,2] * matrix[0,1] - matrix[2,1] * matrix[0,2]);
        tempMatrix[0,2] = matrix[1,2] * matrix[0,1] - matrix[1,1] * matrix[0,2];
        tempMatrix[1,0] = -(matrix[2,2] * matrix[1,0] - matrix[2,0] * matrix[1,2]);
        tempMatrix[1,1] = matrix[2,2] * matrix[0,0] - matrix[2,0] * matrix[0,2];
        tempMatrix[1,2] = -(matrix[1,2] * matrix[0,0] - matrix[1,0] * matrix[0,2]);
        tempMatrix[2,0] = matrix[2,1] * matrix[1,0] - matrix[2,0] * matrix[1,1];
        tempMatrix[2,1] = -(matrix[2,1] * matrix[0,0] - matrix[2,0] * matrix[0,1]);
        tempMatrix[2,2] = matrix[1,1] * matrix[0,0] - matrix[1,0] * matrix[0,1];

        float[,] result = new float[3,3];
        result = ScalarMatrixMultiplication(inverseDeterminant, tempMatrix); 

        return result;
    }

    /**
     * Returns a quaternion that is the embedded vector with scalar part zero.
     * @exampletext \f$ (x, y, z) \mapsto (x, y, z, 0) \f$.
     * @param vector The vector to be embedded.
     * @return The quaternion that is the embedded vector with scalar part zero.
     */
    public BSM.Quaternion EmbeddedVector(Vector3 vector)
    {
        return new BSM.Quaternion(vector.x, vector.y, vector.z, 0f);
    }

    /**
     * Returns the imaginary part of a @p quaternion.
     * @exampletext \f$ (x, y, z, w) \mapsto (x, y, z) \f$.
     * @param quaternion The quaternion whose imaginary part to return.
     * @return The imaginary part of a @p quaternion.
     */
    public Vector3 ImaginaryPart(BSM.Quaternion quaternion)
    {
        return new Vector3(quaternion.X, quaternion.Y, quaternion.Z);
    }

    /**
     * Takes as input a BSM.Quaternion and returns a UnityEngine.Quaternion.
     * @param bsmQuaternion The BSM.Quaternion to be converted.
     * @return The converted UnityEngine.Quaternion.
     */
    public Quaternion QuaternionConversionFromBSM(BSM.Quaternion bsmQuaternion)
    {
        return new Quaternion(bsmQuaternion.X, bsmQuaternion.Y, bsmQuaternion.Z, bsmQuaternion.W);
    }

    /**
     * Takes as input a UnityEngine.Quaternion and returns a BSM.Quaternion.
     * @param bsmQuaternion The UnityEngine.Quaternion to be converted.
     * @return The converted BSM.Quaternion.
     */
    public BSM.Quaternion QuaternionConversionToBSM(Quaternion quaternion)
    {
        return new BSM.Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
    }

    /**
     * Calculates the discrete Darboux Vector of two adjacent orientations @p orientationOne, @p orientationTwo.
     * @param orientationOne The orientation with the lower index, e.g. \f$ i \f$.
     * @param orientationTwo The orientation with the higher index, e.g. \f$ i + 1 \f$.
     * @param rodElementLength The distance between two spheres, also the distance between two orientations.
     * @return The discrete Darboux Vector between @p orientationOne and @p orientationTwo.
     * @note There is only cylinderCount - 1 many darboux vectors. The i-th Darboux Vector
     * is between orientation i and orientation i+1.
     * @attention The order in which the orientations are entered matters. The Darboux Vector of \f$ q_1, q_2 \f$ is not the same as the
     * Darboux Vector of \f$ q_2, q_1 \f$.
     */
    public Vector3 DiscreteDarbouxVector(BSM.Quaternion orientationOne, BSM.Quaternion orientationTwo, float rodElementLength)
    {
        BSM.Quaternion tempQuaternion = BSM.Quaternion.Conjugate(orientationOne) * orientationTwo;
        Vector3 tempVector = ImaginaryPart(tempQuaternion);
        float factor = 2f / rodElementLength;

        return factor * tempVector;
    }

    /**
     * Calculates the sign factor of the current discrete Darboux Vector and the rest Darboux Vector of the same orientations.
     * @note Check the Position and Orientation Based Cosserat Rods Paper (2016) for more information on the sign factor.
     * @param currentDarbouxVector The discrete Darboux Vector of two fixed orientations at the current frame.
     * @param restDarbouxVector The rest Darboux Vector of the same two orientations at frame 0.
     * @return The Sign Factor between these two entities.
     */
    public float DarbouxSignFactor(Vector3 currentDarbouxVector, Vector3 restDarbouxVector)
    {
        float difference = SquaredNorm(currentDarbouxVector - restDarbouxVector);
        float summation = SquaredNorm(currentDarbouxVector + restDarbouxVector);
        
        if (difference <= summation) return 1f;
        else return -1f;
    }

    /**
     * Returns the squared norm of a @p vector.
     * @param vector The vector whose squared norm to return.
     * @return The Squared norm of @p vector.
     */
    private float SquaredNorm(Vector3 vector)
    {
        return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
    }

    /**
     * Returns the vector length of @p vector, i.e. \f$ \sqrt{x_1^2 + x_2^2 + x_3^2} \f$ for a three-dimensional vector.
     * @param vector The vector whose length to return.
     * @return The vector length of @p vector.
     */
    public float VectorLength(Vector3 vector)
    {
        return Mathf.Sqrt(SquaredNorm(vector));
    }

    /**
     * Returns the quaternion length of @p quaternion, i.e. \f$ \sqrt{x^2 + y^2 + z^2 + w^2} \f$.
     * @param quaternion The quaternion whose length to return.
     * @return The quaternion length of @p quaternion.
     */
    public float QuaternionLength(BSM.Quaternion quaternion)
    {
        float q0Squared = Mathf.Pow(quaternion.W, 2);
        Vector3 imaginaryPart = ImaginaryPart(quaternion);
        float qTq = SquaredNorm(imaginaryPart);
        float result = Mathf.Sqrt(q0Squared + qTq);

        return result;
    }

    /**
     * Returns the deviation between the actual distance of @p particlePositionOne and @p particlePositionTwo (current Rod Element Length) and
     * the @p defaultRodElementLength.
     * @param particlePositionOne The first particle under consideration for the rod element length.
     * @param particlePositionTwo The first particle under consideration for the rod element length.
     * @param defaultRodElementLength The rod element length at rest state (i.e. frame 0) between these two particles.
     * @return The deviation between the actual rod element length and the default rod element length.
     */
    public float RodElementLengthDeviation(Vector3 particlePositionOne, Vector3 particlePositionTwo, float defaultRodElementLength)
    {
        float currentRodElementLength = RodElementLength(particlePositionOne, particlePositionTwo);
        float deviation = Mathf.Abs(currentRodElementLength - defaultRodElementLength);

        return deviation;
    }

    /**
     * Returns the deviation of the stretch constraint from zero.
     * @param particlePositionOne \f$ p_1 \f$ of the equation (31).
     * @param particlePositionTwo \f$ p_2 \f$ of the equation (31).
     * @param orientation \f$ q \f$ of the equation (31).
     * @param e_3 \f$ e_3 \f$ of the equation (31).
     * @param rodElementLength \f$ l \f$ of the equation (31).
     * @param logIntermediateResults Whether to output several logs that contain intermediate results of the calculation. Default is false.
     * @return The Deviation of the calculated stretch constraint and zero.
     * @note Check the Position and Orientation Based Cosserat Rods Paper (2016), equation (31), for more information on the stretch constraint.
     */
    public float StretchConstraintDeviation(Vector3 particlePositionOne, Vector3 particlePositionTwo, BSM.Quaternion orientation,
                                            BSM.Quaternion e_3, float rodElementLength, bool logIntermediateResults = false)
    {
        Vector3 firstTerm = (1f / rodElementLength) * (particlePositionTwo - particlePositionOne);
        Vector3 secondTerm = ImaginaryPart(orientation * e_3 * BSM.Quaternion.Conjugate(orientation));

        if (logIntermediateResults)
        {
            float normalizedDotProduct = Vector3.Dot(firstTerm, secondTerm) / (VectorLength(firstTerm) * VectorLength(firstTerm));
            float angle = Mathf.Acos(normalizedDotProduct);

            Debug.Log("Length of first term of C_s: " + VectorLength(firstTerm));
            Debug.Log("Length of second term of C_s: " + VectorLength(secondTerm));
            Debug.Log("Angle between first and second term of C_s: " + angle);

        }

        Vector3 constraintResult = firstTerm - secondTerm;

        float deviation = Vector3.Distance(constraintResult, Vector3.zero);

        return deviation;
    }

    /**
     * Returns the deviation of the bend twist constraint from zero.
     * @param orientationOne \f$ q \f$ of the equation (32).
     * @param orientationTwo \f$ u \f$ of the equation (32).
     * @param rodElementLength The Rod Element Length between @p orientationOne and @p orientationTwo.
     * Used to calculate  \f$ \mathbb{\Omega} \f$ of the equation (32).
     * @param discreteRestDarbouxVector \f$ \mathbb{\Omega}^0 \f$ of the equation (32).
     * @param logIntermediateResults Whether to output several logs that contain intermediate results of the calculation. Default is false.
     * @return The Deviation of the calculated bend twist constraint and zero.
     * @note Check the Position and Orientation Based Cosserat Rods Paper (2016), equation (32), for more information on the bend twist constraint.
     */
    public float BendTwistConstraintDeviation(BSM.Quaternion orientationOne, BSM.Quaternion orientationTwo, float rodElementLength,
                                              Vector3 discreteRestDarbouxVector, bool logIntermediateResults = false)
    {
        Vector3 discreteDarbouxVector = DiscreteDarbouxVector(orientationOne, orientationTwo, rodElementLength);
        float darbouxSignFactor = DarbouxSignFactor(discreteDarbouxVector, discreteRestDarbouxVector);

        if (logIntermediateResults)
        {
            Debug.Log("Discrete Darboux Vector: " + discreteDarbouxVector);
            Debug.Log("Discrete Rest Darboux Vector: " + discreteRestDarbouxVector);
            Debug.Log("Darboux Sign Factor: " + darbouxSignFactor);
        }

        Vector3 constraintResult = discreteDarbouxVector - darbouxSignFactor * discreteRestDarbouxVector;

        float deviation = Vector3.Distance(constraintResult, Vector3.zero);

        return deviation;
    }

    /**
     * Calculates the rod element length between @p particlePositionOne and @p particlePositionTwo.
     * @param particlePositionOne The first particle of the rod element under consideration.
     * @param particlePositionTwo The first particle of the rod element under consideration.
     * @return The rod element length.
     */
    public float RodElementLength(Vector3 particlePositionOne, Vector3 particlePositionTwo)
    {
        return Vector3.Distance(particlePositionOne, particlePositionTwo);
    }

    /**
     * Updates all directors of each orientation at the update step of the simulation loop.
     * @exampletext The directors \f$ d_1, d_2, d_3 \f$ are calculated as \f$ d_i = q \cdot e_i \cdot \bar{q} \f$ for each orientation \f$ q\f$,
     * where \f$ e_i \f$ is the i-th world space basis vector. In quaternion calculus, this means rotating \f$ e_i \f$ by \f$ q \f$.
     * @param cylinderCount The count of all cylinders of the guidewire. Equals the length of @p cylinderOrientationPredictions.
     * @param cylinderOrientations The orientation of each cylinder at its center of mass.
     * @param directors The orthonormal basis of each orientation element / cylinder, also called directors.
     * @param worldSpaceBasis The three basis vectors of the world coordinate system.
     * @return All directors of each orientation.
     */
    public Vector3[][] UpdateDirectors(int cylinderCount, BSM.Quaternion[] cylinderOrientations, Vector3[][] directors, BSM.Quaternion[] worldSpaceBasis)
    {
        for (int cylinderIndex = 0; cylinderIndex < cylinderCount; cylinderIndex++)
        {
            for (int axisIndex = 0; axisIndex < 3; axisIndex++)
            {
                directors[axisIndex][cylinderIndex] = ImaginaryPart(cylinderOrientations[cylinderIndex] * worldSpaceBasis[axisIndex] * BSM.Quaternion.Conjugate(cylinderOrientations[cylinderIndex]));
            }
        }

        return directors;
    }

    /**
     * Provides a random unit quaternion drawn from a gaussian distribution.
     * @return A random unit quaternion drawn from a gaussian distribution.
     * @note This works by drawing four random, gaussian distributed, numbers, and filling the components of the quaternion with these numbers.
     * Mathematically, this is equal to drawing a quaternion from a gaussian distribution in \f$ \mathcal{R}^{4} \f$, since the joint distribution
     * of gaussian samples is again gaussian.
     * @req The length of the drawn quaternion is approximately equal to one.
     */
    public BSM.Quaternion RandomUnitQuaternion()
    {
        float x = GetGaussianRandomNumber();
        float y = GetGaussianRandomNumber();
        float z = GetGaussianRandomNumber();
        float w = GetGaussianRandomNumber();

        BSM.Quaternion quaternion = new BSM.Quaternion(x, y, z, w);

        BSM.Quaternion unitQuaternion = BSM.Quaternion.Normalize(quaternion);

        Assert.AreApproximatelyEqual(1f, QuaternionLength(unitQuaternion), tolerance: 0.01f);

        return unitQuaternion;
    }

    /**
     * Provides a sample from \f$ \mathcal{N}(0,1)\f$ by using the Marsaglia polar method to transform a uniform distribution to a normal distribution.
     * @return A sample from \f$ \mathcal{N}(0,1)\f$.
     * @note To understand this method, google the Marsaglia polar method. Note that unity does not provide a function to
     * generate a random number following a gaussian distribution.
     */
    public float GetGaussianRandomNumber()
    {
        float v1, v2, s;

        do
        {
            v1 = 2.0f * Random.Range(0f,1f) - 1.0f;
            v2 = 2.0f * Random.Range(0f,1f) - 1.0f;
            s = v1 * v1 + v2 * v2;
        } while (s >= 1.0f || s == 0f);

        s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);
    
        return v1 * s;
    }
}
}
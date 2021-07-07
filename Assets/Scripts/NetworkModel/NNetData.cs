using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace NetworkModel
{
    public class NNetData
    {
        public List<MatrixData> weights;
        public List<float> biases;

        public NNetData(List<Matrix<float>> weights, List<float> biases)
        {
            this.weights = new List<MatrixData>();
            foreach (var weight in weights)
                this.weights.Add(new MatrixData(weight));

            this.biases = new List<float>(biases);
        }

        public List<Matrix<float>> GetWeights()
        {
            var weights = new List<Matrix<float>>();
            foreach (var weight in this.weights)
                weights.Add(weight.GetMatrix());
            return weights;
        }

        public List<float> GetBiases()
        {
            return new List<float>(biases);
        }
    }

    [System.Serializable]
    public class MatrixData
    {
        public int rows;
        public int cols;
        public float[] flattened;

        public MatrixData(Matrix<float> matrix)
        {
            rows = matrix.RowCount;
            cols = matrix.ColumnCount;
            flattened = matrix.ToRowMajorArray();
        }

        public Matrix<float> GetMatrix()
        {
            return Matrix<float>.Build.DenseOfRowMajor(rows, cols, flattened);
        }
    }
}
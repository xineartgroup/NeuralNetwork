using System.Text.Json.Serialization;

namespace NeuralNetwork
{
    /// <summary>
    /// Simple matrix class for efficient neural network operations
    /// </summary>
    [Serializable]
    public class Matrix
    {
        [JsonInclude]
        public int Rows { get; private set; }

        [JsonInclude]
        public int Cols { get; private set; }

        [JsonInclude]
        private double[,] Data { get; set; }

        public double this[int row, int col]
        {
            get => Data[row, col];
            set => Data[row, col] = value;
        }

        public Matrix()
        {
            Rows = 0;
            Cols = 0;
            Data = new double[0, 0];
        }

        public Matrix(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Data = new double[rows, cols];
        }

        public Matrix(int rows, int cols, double initialValue)
        {
            Rows = rows;
            Cols = cols;
            Data = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Data[i, j] = initialValue;
                }
            }
        }

        public Matrix(Matrix other)
        {
            Rows = other.Rows;
            Cols = other.Cols;
            Data = new double[Rows, Cols];

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    Data[i, j] = other[i, j];
                }
            }
        }

        public static Matrix FromList(List<double> list, bool asRow)
        {
            if (asRow)
            {
                Matrix result = new(1, list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    result[0, i] = list[i];
                }
                return result;
            }
            else
            {
                Matrix result = new(list.Count, 1);
                for (int i = 0; i < list.Count; i++)
                {
                    result[i, 0] = list[i];
                }
                return result;
            }
        }

        public List<double> ToList()
        {
            List<double> result = [];
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    result.Add(Data[i, j]);
                }
            }
            return result;
        }

        public static Matrix Transpose(Matrix m)
        {
            Matrix result = new(m.Cols, m.Rows);
            for (int i = 0; i < m.Rows; i++)
            {
                for (int j = 0; j < m.Cols; j++)
                {
                    result[j, i] = m[i, j];
                }
            }
            return result;
        }

        public static Matrix Multiply(Matrix a, Matrix b)
        {
            if (a.Cols != b.Rows)
            {
                throw new ArgumentException("Number of columns in the first matrix must equal the number of rows in the second matrix for multiplication.");
            }

            Matrix result = new(a.Rows, b.Cols);
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < b.Cols; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < a.Cols; k++)
                    {
                        sum += a[i, k] * b[k, j];
                    }
                    result[i, j] = sum;
                }
            }
            return result;
        }

        public static Matrix Add(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Cols != b.Cols)
            {
                throw new ArgumentException("Matrices must have the same dimensions for addition.");
            }

            Matrix result = new(a.Rows, a.Cols);
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Cols; j++)
                {
                    result[i, j] = a[i, j] + b[i, j];
                }
            }
            return result;
        }

        public static Matrix Subtract(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Cols != b.Cols)
            {
                throw new ArgumentException("Matrices must have the same dimensions for subtraction.");
            }

            Matrix result = new(a.Rows, a.Cols);
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Cols; j++)
                {
                    result[i, j] = a[i, j] - b[i, j];
                }
            }
            return result;
        }

        public static Matrix Multiply(Matrix m, double scalar)
        {
            Matrix result = new(m.Rows, m.Cols);
            for (int i = 0; i < m.Rows; i++)
            {
                for (int j = 0; j < m.Cols; j++)
                {
                    result[i, j] = m[i, j] * scalar;
                }
            }
            return result;
        }

        public void ApplyFunction(Func<double, double> func)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    Data[i, j] = func(Data[i, j]);
                }
            }
        }

        public static Matrix Map(Matrix m, Func<double, double> func)
        {
            Matrix result = new(m.Rows, m.Cols);
            for (int i = 0; i < m.Rows; i++)
            {
                for (int j = 0; j < m.Cols; j++)
                {
                    result[i, j] = func(m[i, j]);
                }
            }
            return result;
        }

        public Matrix Copy()
        {
            return new Matrix(this);
        }

        public void Randomize(double min = 0, double max = 1)
        {
            Random random = new();
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    Data[i, j] = random.NextDouble() * (max - min) + min;
                }
            }
        }

        public double[][] ToJaggedArray()
        {
            var result = new double[Rows][];
            for (int i = 0; i < Rows; i++)
            {
                result[i] = new double[Cols];
                for (int j = 0; j < Cols; j++)
                {
                    result[i][j] = this[i, j];
                }
            }
            return result;
        }

        public static Matrix FromJaggedArray(double[][] data)
        {
            if (data == null || data.Length == 0)
                return new Matrix(0, 0);

            int rows = data.Length;
            int cols = data[0].Length;
            var matrix = new Matrix(rows, cols);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = data[i][j];
                }
            }
            return matrix;
        }

        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    result += Data[i, j] + (j == Cols - 1 ? "" : "\t");
                }
                result += "\n";
            }
            return result;
        }
    }
}

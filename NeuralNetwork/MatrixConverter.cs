using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeuralNetwork
{
    // Custom converter for Matrix to handle double[,] serialization
    public class MatrixConverter : JsonConverter<Matrix>
    {
        public override Matrix Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            int rows = root.GetProperty("Rows").GetInt32();
            int cols = root.GetProperty("Cols").GetInt32();

            var matrix = new Matrix(rows, cols);

            // Read the serialized data as jagged array
            var dataElement = root.GetProperty("Data");
            for (int i = 0; i < rows; i++)
            {
                var rowElement = dataElement[i];
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = rowElement[j].GetDouble();
                }
            }

            return matrix;
        }

        public override void Write(Utf8JsonWriter writer, Matrix value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteNumber("Rows", value.Rows);
            writer.WriteNumber("Cols", value.Cols);

            // Convert 2D array to jagged array for serialization
            writer.WritePropertyName("Data");
            writer.WriteStartArray();

            for (int i = 0; i < value.Rows; i++)
            {
                writer.WriteStartArray();
                for (int j = 0; j < value.Cols; j++)
                {
                    writer.WriteNumberValue(value[i, j]);
                }
                writer.WriteEndArray();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
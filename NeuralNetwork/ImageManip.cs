namespace NeuralNetwork
{
    public class ImageManip
    {
        public const int ImageSize = 28;
        public const int PixelCount = ImageSize * ImageSize; // 784

        public static (List<double>, bool) LoadAndProcessImage(string imagePath)
        {
            Bitmap? processedBitmap;
            var pixelValues = new List<double>(PixelCount);
            (pixelValues, processedBitmap, _) = LoadSingleImage(imagePath, isBlackOnWhite: false);
            processedBitmap?.Dispose(); // Explicitly dispose since we can't use 'using' with the conditional assignment

            return (pixelValues, true);
        }

        public static (List<double>, Bitmap?, bool) LoadSingleImage(string imagePath, bool isBlackOnWhite = false)
        {
            try
            {
                using var bitmap = new Bitmap(imagePath);

                // Resize if necessary
                Bitmap processedBitmap;
                if (bitmap.Width != ImageSize || bitmap.Height != ImageSize)
                {
                    processedBitmap = new Bitmap(ImageSize, ImageSize);
                    using var graphics = Graphics.FromImage(processedBitmap);
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(bitmap, 0, 0, ImageSize, ImageSize);
                }
                else
                {
                    processedBitmap = new Bitmap(bitmap);
                }

                var pixelValues = new List<double>(PixelCount);

                var rect = new Rectangle(0, 0, ImageSize, ImageSize);
                var data = processedBitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                int lightCount = 0;
                int darkCount = 0;

                unsafe
                {
                    byte* ptr = (byte*)data.Scan0;

                    for (int y = 0; y < ImageSize; y++)
                    {
                        byte* row = ptr + (y * data.Stride);

                        for (int x = 0; x < ImageSize; x++)
                        {
                            int pos = x * 4;

                            byte blue = row[pos];
                            byte green = row[pos + 1];
                            byte red = row[pos + 2];

                            double grayscale = (0.299 * red + 0.587 * green + 0.114 * blue) / 255.0;
                            double normalized = grayscale;

                            if (normalized > 0.5)
                            {
                                lightCount++;
                            }
                            else
                            {
                                darkCount++;
                            }

                            if (isBlackOnWhite)
                            {
                                pixelValues.Add(1.0 - normalized); // Invert for black-on-white
                            }
                            else
                            {
                                pixelValues.Add(normalized);
                            }
                        }
                    }
                }

                processedBitmap.UnlockBits(data);
                //processedBitmap.Dispose();

                return (pixelValues, processedBitmap, lightCount > darkCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image {imagePath}: {ex.Message}");
                return ([], null, false);
            }
        }
    }
}

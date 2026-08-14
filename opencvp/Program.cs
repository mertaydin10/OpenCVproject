using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;

namespace opencvp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string baseDir = AppContext.BaseDirectory;

            string imagePath = Path.Combine(baseDir, "image.png");
            using Mat image = Cv2.ImRead(imagePath);

            if (image.Empty())
            {
                Console.WriteLine($"image.png bulunamadı: {imagePath}");
                return;
            }

            using Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

            using var clahe = Cv2.CreateCLAHE(clipLimit: 2.0, tileGridSize: new Size(8, 8));
            using Mat enhanced = new Mat();
            clahe.Apply(gray, enhanced);

            string frontalPath = Path.Combine(baseDir, "haarcascade_frontalface_default.xml");
            string profilePath = Path.Combine(baseDir, "haarcascade_profileface.xml");

            using var frontalCascade = new CascadeClassifier(frontalPath);
            using var profileCascade = new CascadeClassifier(profilePath);

            Rect[] frontalFaces = frontalCascade.DetectMultiScale(
                enhanced,
                scaleFactor: 1.12,
                minNeighbors: 8,
                flags: HaarDetectionTypes.ScaleImage,
                minSize: new Size(75, 75)
            );

            Rect[] profileFaces = DetectProfiles(profileCascade, enhanced);
            Rect[] mirroredProfiles = DetectProfiles(profileCascade, enhanced, flip: true);

            int minArea = (image.Width * image.Height) / 3200;
            List<Rect> faces = FilterFaces(
                frontalFaces.Concat(profileFaces).Concat(mirroredProfiles),
                minArea,
                image.Height
            );

            Console.WriteLine($"Tespit edilen yüz sayısı: {faces.Count}");

            foreach (Rect face in faces)
            {
                Cv2.Rectangle(image, face, Scalar.Red, 3);
            }

            string resultPath = Path.Combine(baseDir, "result.jpg");
            Cv2.ImWrite(resultPath, image);

            Console.WriteLine("İşlem tamamlandı.");
            Console.WriteLine($"Sonuç: {resultPath}");
        }

        static Rect[] DetectProfiles(CascadeClassifier cascade, Mat gray, bool flip = false)
        {
            using Mat input = new Mat();
            if (flip)
                Cv2.Flip(gray, input, FlipMode.Y);
            else
                gray.CopyTo(input);

            Rect[] faces = cascade.DetectMultiScale(
                input,
                scaleFactor: 1.1,
                minNeighbors: 7,
                flags: HaarDetectionTypes.ScaleImage,
                minSize: new Size(70, 70)
            );

            if (!flip)
                return faces;

            return faces
                .Select(r => new Rect(input.Width - r.X - r.Width, r.Y, r.Width, r.Height))
                .ToArray();
        }

        static List<Rect> FilterFaces(IEnumerable<Rect> candidates, int minArea, int imageHeight)
        {
            var filtered = candidates
                .Where(r => r.Width * r.Height >= minArea)
                .Where(r => r.Height >= 78)
                .Where(r => !(r.Y > imageHeight * 0.72 && r.Height < 95))
                .Where(IsFaceLikeShape)
                .OrderByDescending(r => r.Width * r.Height)
                .ToList();

            var result = new List<Rect>();
            foreach (Rect candidate in filtered)
            {
                if (result.All(existing => OverlapRatio(existing, candidate) < 0.35))
                    result.Add(candidate);
            }

            return result;
        }

        static bool IsFaceLikeShape(Rect rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
                return false;

            double ratio = (double)rect.Width / rect.Height;
            return ratio is >= 0.75 and <= 1.35;
        }

        static double OverlapRatio(Rect a, Rect b)
        {
            int x1 = Math.Max(a.X, b.X);
            int y1 = Math.Max(a.Y, b.Y);
            int x2 = Math.Min(a.X + a.Width, b.X + b.Width);
            int y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);

            int overlapWidth = Math.Max(0, x2 - x1);
            int overlapHeight = Math.Max(0, y2 - y1);
            double overlapArea = overlapWidth * overlapHeight;

            double minArea = Math.Min(a.Width * a.Height, b.Width * b.Height);
            return minArea > 0 ? overlapArea / minArea : 0;
        }
    }
}

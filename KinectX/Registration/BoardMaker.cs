using OpenCvSharp;
using OpenCvSharp.Aruco;
using System.Collections.Generic;
using System.IO;

namespace KinectX.Registration
{
    /// <summary>
    /// class which builds out images to build a 3D registration cube of ARUCO markers
    /// </summary>
    public class BoardMaker
    {
        public static void BuildRegistrationCube(string path, double cmWidth = 10.4)
        {
            var dir = Path.GetDirectoryName(path);
            var sides = new Dictionary<string, int>() //side, starting index id
            {
                { "Side1", 0 },
                { "Side2", 4},
                { "Side3", 8},
                { "Side4", 12},
                { "Side5", 16},
                { "Side6", 20},
            };

            var pixelsPerCM = 300 / 2.54;

            //2 SQUARES WIDE, 2 SQUARES HIGH, 150 margin on width,
            //100 margin on height
            var paperPxWidth = pixelsPerCM * cmWidth; //11in - 1in margin *300px/in
            var paperPxHeight = pixelsPerCM * cmWidth; //17in - 1in margin *300px/in
            var markerPxWidth = (int)(pixelsPerCM * 4.5 / 10.4 * cmWidth);
            var marginWidth = (int)((paperPxWidth - 2.0 * markerPxWidth) / 4);
            var verify = 2 * markerPxWidth + 4 * marginWidth;
            var marginHeight = marginWidth;
            var rows = 2;
            var columns = 2;
            var buffer = 0;

            var id = 0;

            foreach (var side in sides)
            {
                Rect roi = new Rect(0, 0, markerPxWidth, markerPxWidth);
                using (var outputImage = new Mat(new Size(paperPxWidth, paperPxHeight), MatType.CV_8UC1, Scalar.White))
                {
                    for (var y = 0; y < rows; y++)
                    {
                        roi.Top = y * markerPxWidth + marginHeight * (y * 2 + 1);

                        for (var x = 0; x < columns; x++)
                        {
                            roi.Left = x * markerPxWidth + marginWidth * (x * 2 + 1);

                            using (var roiMat = new Mat(outputImage, roi))
                            using (var markerImage = new Mat())
                            using (var dict2 = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict6X6_250))
                            {
                                CvAruco.DrawMarker(dict2, id++, markerPxWidth, markerImage, 1);
                                markerImage.CopyTo(roiMat);
                            }
                        }
                    }

                    var crossHairWidth = 1 * pixelsPerCM; // 1 cm
                    var crossHairColor = new Scalar(25, 25, 25);
                    Cv2.Line(outputImage, new Point((float)paperPxWidth / 2, (float)paperPxHeight / 2), new Point((float)paperPxWidth / 2, (float)(paperPxHeight / 2 - crossHairWidth)), crossHairColor);
                    Cv2.Line(outputImage, new Point((float)paperPxWidth / 2, (float)paperPxHeight / 2), new Point((float)paperPxWidth / 2, (float)(paperPxHeight / 2 + crossHairWidth)), crossHairColor);
                    Cv2.Line(outputImage, new Point((float)paperPxWidth / 2, (float)paperPxHeight / 2), new Point((float)(paperPxWidth / 2 - crossHairWidth), (float)(paperPxHeight / 2)), crossHairColor);
                    Cv2.Line(outputImage, new Point((float)paperPxWidth / 2, (float)paperPxHeight / 2), new Point((float)(paperPxWidth / 2 + crossHairWidth), (float)(paperPxHeight / 2)), crossHairColor);

                    Cv2.PutText(outputImage, side.Key, new Point((float)paperPxWidth / 2 - marginWidth/1.3, (float)paperPxHeight), HersheyFonts.HersheyPlain, 1.5/10.4*cmWidth, new Scalar(25, 25, 25), 1);
                    path = Path.Combine(dir, side.Key + ".png");
                    Cv2.ImWrite(path, outputImage);
                }

            }
        }
    }
}
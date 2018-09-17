using OpenCvSharp;
using OpenCvSharp.Aruco;
using System;
using System.Collections.Generic;
using System.IO;

namespace KinectX.Registration
{
    /// <summary>
    /// class which builds out images to build a 3D registration cube of ARUCO markers
    /// </summary>
    public class BoardMaker
    {
        public static void BuildRegistrationCube(string path, int mmWidth = 104)
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

            //3 SQUARES WIDE, 3 SQUARES HIGH, 150 margin on width,
            //100 margin on height
            var paperPxWidth = pixelsPerCM* 10.4; //11in - 1in margin *300px/in
            var paperPxHeight = pixelsPerCM * 10.4; //17in - 1in margin *300px/in
            var markerPxWidth = (int)(pixelsPerCM * 4.5);
            var marginWidth = (int)(pixelsPerCM * 0.5);
            var marginHeight = (int)(pixelsPerCM * 0.5);
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
                        roi.Top = y * markerPxWidth + marginHeight * (y + 1) + buffer;

                        for (var x = 0; x < columns; x++)
                        {
                            roi.Left = x * markerPxWidth + marginWidth * (x + 1) + buffer;

                            using (var roiMat = new Mat(outputImage, roi))
                            using (var markerImage = new Mat())
                            using (var dict2 = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict6X6_250))
                            {
                                CvAruco.DrawMarker(dict2, id++, markerPxWidth, markerImage, 1);
                                markerImage.CopyTo(roiMat);
                            }
                        }
                    }

                    path = Path.Combine(dir, side.Key + ".png");
                    Cv2.ImWrite(path, outputImage);
                }

            }
        }
    }
}
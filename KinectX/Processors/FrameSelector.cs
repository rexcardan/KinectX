using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinect.Voxels.Processors
{
    public class FrameSelector
    {
        public FrameSelector(int initialFrame=1, int numOfFramesToProcess = 1)
        {
            InitialFrame = initialFrame;
            NumFramesToProcess = numOfFramesToProcess;
        }

        public int InitialFrame { get; set; } = 1;
        public int NumFramesToProcess { get; set; } = 1;

        public bool IsDesiredFrame
        {
            get
            {
                return _frameNum >= InitialFrame && _frameNum < InitialFrame + NumFramesToProcess;
            }
        }

        public bool AreAllFramesCaptured
        {
            get
            {
                return CurrentFrameNum >= InitialFrame + NumFramesToProcess - 1;
            }
        }

        public int CurrentFrameNum { get { return _frameNum; } }

        private int _frameNum = 0;
        public void IncrementFrame()
        {
            _frameNum++;
        }
    }
}

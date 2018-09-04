using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Network
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName = "IKxServer")]
    public interface IKxServer
    {

        [System.ServiceModel.OperationContractAttribute(Action = "http://cardankx.org/IKxServer/LatestDepthImage", ReplyAction = "http://cardankx.org/IKxServer/LatestDepthImageResponse")]
        byte[] LatestDepthImage();

        [System.ServiceModel.OperationContractAttribute(Action = "http://cardankx.org/IKxServer/LatestDepthImage", ReplyAction = "http://cardankx.org/IKxServer/LatestDepthImageResponse")]
        System.Threading.Tasks.Task<byte[]> LatestDepthImageAsync();

        [System.ServiceModel.OperationContractAttribute(Action = "http://cardankx.org/IKxServer/LatestYUVImage", ReplyAction = "http://cardankx.org/IKxServer/LatestYUVImageResponse")]
        byte[] LatestYUVImage();

        [System.ServiceModel.OperationContractAttribute(Action = "http://cardankx.org/IKxServer/LatestYUVImage", ReplyAction = "http://cardankx.org/IKxServer/LatestYUVImageResponse")]
        System.Threading.Tasks.Task<byte[]> LatestYUVImageAsync();

        [System.ServiceModel.OperationContractAttribute(Action = "http://cardankx.org/IKxServer/LatestRGBImage", ReplyAction = "http://cardankx.org/IKxServer/LatestRGBImageResponse")]
        byte[] LatestRGBImage();

        [System.ServiceModel.OperationContractAttribute(Action = "http://cardankx.org/IKxServer/LatestRGBImage", ReplyAction = "http://cardankx.org/IKxServer/LatestRGBImageResponse")]
        System.Threading.Tasks.Task<byte[]> LatestRGBImageAsync();

        [System.ServiceModel.OperationContractAttribute(Action = "http://cardankx.org/IKxServer/LatestJPEGImage", ReplyAction = "http://cardankx.org/IKxServer/LatestJPEGImageResponse")]
        byte[] LatestJPEGImage();

        [System.ServiceModel.OperationContractAttribute(Action = "http://cardankx.org/IKxServer/LatestJPEGImage", ReplyAction = "http://cardankx.org/IKxServer/LatestJPEGImageResponse")]
        System.Threading.Tasks.Task<byte[]> LatestJPEGImageAsync();

        [System.ServiceModel.OperationContractAttribute(Action = "http://cardankx.org/IKxServer/LastColorGain", ReplyAction = "http://cardankx.org/IKxServer/LastColorGainResponse")]
        float LastColorGain();

        [System.ServiceModel.OperationContractAttribute(Action = "http://cardankx.org/IKxServer/LastColorGain", ReplyAction = "http://cardankx.org/IKxServer/LastColorGainResponse")]
        System.Threading.Tasks.Task<float> LastColorGainAsync();

        [System.ServiceModel.OperationContractAttribute(Action = "http://cardankx.org/IKxServer/LastColorExposureTimeTicks", ReplyAction = "http://cardankx.org/IKxServer/LastColorExposureTimeTicksResponse")]
        long LastColorExposureTimeTicks();

        [System.ServiceModel.OperationContractAttribute(Action = "http://cardankx.org/IKxServer/LastColorExposureTimeTicks", ReplyAction = "http://cardankx.org/IKxServer/LastColorExposureTimeTicksResponse")]
        System.Threading.Tasks.Task<long> LastColorExposureTimeTicksAsync();

        //[System.ServiceModel.OperationContractAttribute(Action = "http://cardankx.org/IKxServer/GetCalibration", ReplyAction = "http://cardankx.org/IKxServer/GetCalibrationResponse")]
        //RoomAliveToolkit.Kinect2Calibration GetCalibration();

        //[System.ServiceModel.OperationContractAttribute(Action = "http://cardankx.org/IKxServer/GetCalibration", ReplyAction = "http://cardankx.org/IKxServer/GetCalibrationResponse")]
        //System.Threading.Tasks.Task<RoomAliveToolkit.Kinect2Calibration> GetCalibrationAsync();
    }
}

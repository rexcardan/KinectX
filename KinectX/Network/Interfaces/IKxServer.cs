﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;

namespace KinectX.Network.Interfaces
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName = "KxServer")]
    public interface KxServer
    {

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/KxServer/LatestDepthImage", ReplyAction = "http://tempuri.org/KxServer/LatestDepthImageResponse")]
        ushort[] LatestDepthImage();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/KxServer/LatestYUVImage", ReplyAction = "http://tempuri.org/KxServer/LatestYUVImageResponse")]
        byte[] LatestYUVImage();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/KxServer/LatestRGBImage", ReplyAction = "http://tempuri.org/KxServer/LatestRGBImageResponse")]
        byte[] LatestRGBImage();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/KxServer/FusionCameraPose", ReplyAction = "http://tempuri.org/KxServer/FusionCameraPoseResponse")]
        byte[] FusionCameraPose();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/KxServer/CameraPose", ReplyAction = "http://tempuri.org/KxServer/CameraPoseResponse")]
        byte[] CameraPose();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/KxServer/RecordXef", ReplyAction = "http://tempuri.org/KxServer/RecordXefResponse")]
        bool RecordXef(TimeSpan duration);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/KxServer/LastRecording", ReplyAction = "http://tempuri.org/KxServer/LastRecordingResponse")]
        byte[] LastRecording();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/KxServer/LatestJPEGImage", ReplyAction = "http://tempuri.org/KxServer/LatestJPEGImageResponse")]
        byte[] LatestJPEGImage();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/KxServer/LastColorGain", ReplyAction = "http://tempuri.org/KxServer/LastColorGainResponse")]
        float LastColorGain();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/KxServer/LastColorExposureTimeTicks", ReplyAction = "http://tempuri.org/KxServer/LastColorExposureTimeTicksResponse")]
        long LastColorExposureTimeTicks();
    }
}
﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KinectX.Network.Interfaces
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName = "KxServer")]
    public interface KxServer
    {

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/KxServer/LatestDepthImage", ReplyAction = "http://tempuri.org/KxServer/LatestDepthImageResponse")]
        byte[] LatestDepthImage();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/KxServer/LatestYUVImage", ReplyAction = "http://tempuri.org/KxServer/LatestYUVImageResponse")]
        byte[] LatestYUVImage();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/KxServer/LatestRGBImage", ReplyAction = "http://tempuri.org/KxServer/LatestRGBImageResponse")]
        byte[] LatestRGBImage();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/KxServer/LatestJPEGImage", ReplyAction = "http://tempuri.org/KxServer/LatestJPEGImageResponse")]
        byte[] LatestJPEGImage();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/KxServer/LastColorGain", ReplyAction = "http://tempuri.org/KxServer/LastColorGainResponse")]
        float LastColorGain();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/KxServer/LastColorExposureTimeTicks", ReplyAction = "http://tempuri.org/KxServer/LastColorExposureTimeTicksResponse")]
        long LastColorExposureTimeTicks();
    }
}
using Google.Protobuf.WellKnownTypes;
using HalconDotNet;
using Microsoft.ML.Tokenizers;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using work.cv;
using work.math;

namespace AIDatasetsPro.core
{
    internal interface IIc
    {
        string data_dir_path { get; }
        Size size { get; }

        List<double[]> FindModel(Mat Src, double MinScore, int NumMatches, out Mat dis, out Mat mask, int NumLevels = 5, double MaxOverlap = 0.0, string SubPixel = "true", double Greediness = 0.7);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.EnumsAndStats
{
    public static class Consts
    {
        public const double GraphXScaleFactorStandard  = 1000D;
        public const double GraphXScaleFactorMaruniec = 10D;
        public const double TimeScaleFactorStandard = 100D;

        public const int PursuitEndTimeStandard = 9231000;
        public const int SaccadeStartTimeStandard = 10676000; //10149000;
        public const int AntiSaccadeStartTimeStandard = 15426000;


        public const int PursuitEndTimeMaruniec = 98360;
        public const int SaccadeStartTimeMaruniec = 111000;
        public const int AntiSaccadeStartTimeMaruniec = 155000;

        public const string FileExtentionMaruniec = ".mrc";
        public const string FileExtentionET = ".et";

        public const string FileExtentionTxt = ".txt";
        public const string FileExtentionCsv = ".csv";
        public const string FileExtentionXml= ".xml";

    }
}

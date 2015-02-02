using System;
using System.Collections.Generic;
using System.IO;
using Encog.Util.File;

namespace MaterialPositioner
{
    public static class Config
    {
     

        public static int NumberOfInputs = 3;

        public static FileInfo BasePath = new FileInfo(@"F:\GIT\ENCOG\Football\LHCbCorners\encog-dotnet-core-3.1.0\Data\Football\Football\Prem\PremTest\1HL_2NNeurons\");

        #region "Step 1"

        
        /*
        public static FileInfo BaseFile = FileUtil.CombinePath(BasePath, "E01.csv");
        public static FileInfo RealRunnerFile = FileUtil.CombinePath(BasePath, "RealRunnerFile_070215.csv");
        */

        
        public static FileInfo BaseFile = FileUtil.CombinePath(BasePath, "E01_ExcludeLastWeek.csv");
        public static FileInfo RealRunnerFile = FileUtil.CombinePath(BasePath, "RealRunnerFile_100115.csv");
        

        public static FileInfo ShuffledBaseFile = FileUtil.CombinePath(BasePath, "ShuffledMaterials.csv");

        #endregion

        #region "Step 2"

        public static FileInfo TrainingFile = FileUtil.CombinePath(BasePath, "TrainingMaterials.csv");
        public static FileInfo EvaluationFile = FileUtil.CombinePath(BasePath, "EvaluationMaterials.csv");

        #endregion

        #region "Step 3"

        public static FileInfo NormalizedTrainingFile = FileUtil.CombinePath(BasePath, "NormTrainingMaterials.csv");
        public static FileInfo NormalizedEvaluationFile = FileUtil.CombinePath(BasePath, "NormEvaluationMaterials.csv");
        public static FileInfo AnalysisFile = FileUtil.CombinePath(BasePath, "AnalysisMaterials.ega");

        #endregion

        #region "Step 4"

        public static FileInfo TrainedNetworkFile = FileUtil.CombinePath(BasePath, "Density_Train.eg");

        #endregion

        #region "Step 5"

        #endregion

        #region "Step 6"

        public static FileInfo ValidationResult = FileUtil.CombinePath(BasePath, "Density_ValidationResult.csv");

        #endregion


        #region "Step 7"

        public static FileInfo NormalizedRealRunnerFile = FileUtil.CombinePath(BasePath, "NormalizedRealRunnerFile.csv");

        #endregion

        #region "Step 8"

        public static FileInfo OutputResult = FileUtil.CombinePath(BasePath, "OutputResult.csv");
        public static FileInfo ShuffledFile = FileUtil.CombinePath(BasePath, "ShuffledReal.csv");
        public static FileInfo RealEvaluationFile = FileUtil.CombinePath(BasePath, "RealEvaluationFile.csv");
        public static FileInfo DummyEvaluationFile = FileUtil.CombinePath(BasePath, "DummyEvaluationFile.csv");
        public static int NumberOfTeams { get; set; }
        public static int NumberOfOutputs { get; set; }
        public static int IdealOutputs { get; set; }

        #endregion

        public static FileInfo OutputTests = FileUtil.CombinePath(BasePath, "OutputTests.csv");
        
        
    }
}

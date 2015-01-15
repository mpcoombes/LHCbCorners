using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encog.Util.File;

namespace AttributePredictor
{
    class Config
    {        
        public static FileInfo BasePath = new FileInfo(@"C:\MlData\");

        #region "Step 1"

        public static FileInfo BaseFile = FileUtil.CombinePath(BasePath, "Materials.csv");
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

        #endregion

        #region "Step 5"

        #endregion

        #region "Step 6"

        #endregion
    }
    }
}

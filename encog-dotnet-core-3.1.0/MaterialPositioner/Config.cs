using System;
using System.Collections.Generic;
using System.IO;
using Encog.Util.File;

namespace MaterialPositioner
{
    public static class Config
    {
        public static List<string> Teams 
        {
            get
            {
                var teams = new List<string>();
                teams.Add("Arsenal");
                teams.Add("Leicester");
                teams.Add("Man United");
                teams.Add("QPR");
                teams.Add("Stoke");
                teams.Add("West Brom");
                teams.Add("West Ham");
                teams.Add("Crystal Palace");
                teams.Add("Everton");
                teams.Add("Swansea");
                teams.Add("Hull");
                teams.Add("Aston Villa");
                teams.Add("Sunderland");
                teams.Add("Man City");
                teams.Add("Tottenham");
                teams.Add("Southampton");
                teams.Add("Liverpool");
                teams.Add("Newcastle");
                teams.Add("Burnley");
                teams.Add("Chelsea");
                teams.Add("Norwich");
                teams.Add("Cardiff");
                teams.Add("Fulham");

                teams.Sort();
                return teams;
            }
        }

        public static Dictionary<string, double> TeamMap
        {
            get
            {
                double i = 0.0;
                var teamMap = new Dictionary<string, double>();
                foreach (var ateam in Teams)
                {
                    teamMap[ateam] = i;
                    i = i + 1.0;
                }
                return teamMap;
            }
        }


        public static string GetTeam(double id)
        {
            foreach (var team in TeamMap)
            {
                if (Math.Abs(team.Value - id) < 0.01)
                {
                    return team.Key;
                }
            }
            return "";
        }

        public static double GetTeamNumber(string id)
        {
            foreach (var team in TeamMap)
            {
                if (team.Key == id)
                {
                    return team.Value;
                }
            }
            return -99;
        }
        

        public static FileInfo BasePath = new FileInfo(@"C:\MlData\Test\Football\Football\L1\");

        #region "Step 1"

        public static FileInfo BaseFile = FileUtil.CombinePath(BasePath, "L1.csv");
//        public static FileInfo BaseFile = FileUtil.CombinePath(BasePath, "YieldStrengthELModulusBase.csv");

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

        public static FileInfo RealRunnerFile = FileUtil.CombinePath(BasePath, "RealRunnerFile.csv");
        public static FileInfo NormalizedRealRunnerFile = FileUtil.CombinePath(BasePath, "NormalizedRealRunnerFile.csv");

        #endregion

        #region "Step 8"

        public static FileInfo OutputResult = FileUtil.CombinePath(BasePath, "OutputResult.csv");
        public static FileInfo ShuffledFile = FileUtil.CombinePath(BasePath, "ShuffledReal.csv");
        public static FileInfo RealEvaluationFile = FileUtil.CombinePath(BasePath, "RealEvaluationFile.csv");
        public static FileInfo DummyEvaluationFile = FileUtil.CombinePath(BasePath, "DummyEvaluationFile.csv");
        public static int NumberOfTeams { get; set; }
        #endregion
    }
}

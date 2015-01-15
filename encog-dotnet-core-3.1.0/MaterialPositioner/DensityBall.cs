using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encog.App.Analyst;
using Encog.App.Analyst.CSV.Normalize;
using Encog.App.Analyst.Script.Normalize;
using Encog.App.Analyst.Wizard;
using Encog.Engine.Network.Activation;
using Encog.ML.Data.Basic;
using Encog.Neural.Networks;
using Encog.Persist;
using Encog.Util.Arrayutil;
using Encog.Util.CSV;
using Encog.Util.Simple;

namespace MaterialPositioner
{
    public class DensityBall
    {
        private BasicNetwork network;
        private EncogAnalyst analyst;
        private AnalystWizard wizard;

        public DensityBall()
        {
            network = (BasicNetwork)EncogDirectoryPersistence.LoadObject(Config.TrainedNetworkFile);
            analyst = new EncogAnalyst();
            analyst.Load(Config.AnalysisFile.ToString());
            wizard = new AnalystWizard(analyst);
            var evaluationSet = EncogUtility.LoadCSV2Memory(Config.NormalizedEvaluationFile.ToString(),
                network.InputCount, network.OutputCount, true, CSVFormat.English, false);
        }

        public List<double> NormalizeArray(List<double> input, double[] max, double[] min)
        {
            //var norm = new AnalystNormalize(wizard._script);
            var results = new List<double>();
            int i = 0;
            foreach (var val in input)
            {
                var norm = new NormalizedField(NormalizationAction.Normalize,
                                              null, max[i], min[i], 1, -1);
                results[i] = norm.Normalize(val);

                i++;
            }

            return results;
        }

        public double PredicteDensity(List<double> input)
        {
            double density = 0.0;
            //double[] maxs = {   485754.6733, 680038.8153, 582896.7443,
            //                    1050, 1210, 1130,
            //                    4200,4900,4550,
            //                    3776.999902,3909.999902,3828.499902,
            //                    400000000000000000,5000000000000000000, 2700000000000000000};

            //double[] mins = {   0.006071, 0.006071, 0.006071,
            //                    0.00000732,0.0000113,0.00000932,
            //                    0.00012,0.00025,0.000185,
            //                    -39.0000061,-39.0000061,-39.0000061,
            //                    1.580000043,1.620000005,1.600000024 };
            double[] maxs =
            {
                100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 
                100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 
                100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 
                100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0
            };
            double[] mins =
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0
            };
            var normInput = NormalizeArray(input, maxs, mins);
            var normOutput = new List<double>();
            network.Compute(normInput.ToArray(), normOutput.ToArray());

            density = analyst.Script.Normalize.NormalizedFields[15].DeNormalize(normOutput[0]);

            return density;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encog.App.Analyst;
using Encog.App.Analyst.CSV.Normalize;
using Encog.App.Analyst.CSV.Segregate;
using Encog.App.Analyst.CSV.Shuffle;
using Encog.App.Analyst.Script;
using Encog.App.Analyst.Wizard;
using Encog.Engine.Network.Activation;
using Encog.ML;
using Encog.ML.Bayesian;
using Encog.ML.Data.Basic;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Persist;
using Encog.Util.Arrayutil;
using Encog.Util.CSV;
using Encog.Util.Simple;

namespace MaterialPositioner
{
    class Program
    {
        private const bool Classification = false;

        private static void Main(string[] args)
        {
            FootballParameters = new Dictionary<string, int>();
            // Step 1
            Shuffle(Config.BaseFile, Config.ShuffledBaseFile);
            // Shuffle(Config.RealRunnerFile, Config.ShuffledFile);
            // Step 2
            Segregate(Config.ShuffledBaseFile, Config.TrainingFile, Config.EvaluationFile, 99, 1);
            Segregate(Config.RealRunnerFile, Config.RealEvaluationFile, Config.DummyEvaluationFile, 100, 0);

            // Step 3
            Normalize();
            // Step 4

            //  int hiddenLayers = 6;
            for (int hiddenLayers = 4; hiddenLayers < 7; ++hiddenLayers)
            {
                CreateNetwork(Config.TrainedNetworkFile, hiddenLayers);
                // Step 5
                TrainNetwork();
                // Step 6
                double error = Evaluate();
                Console.WriteLine("Error {0}, Hidden Layers {1}", error.ToString(), hiddenLayers.ToString());
            }

            Console.WriteLine("press any key to exit...");
            Console.ReadLine();
        }

        static void Shuffle(FileInfo source, FileInfo output)
        {
            var shuffle = new ShuffleCSV();
            shuffle.Analyze(source, true, CSVFormat.English);
            shuffle.ProduceOutputHeaders = true;
            shuffle.Process(output);
        }

        static void Segregate(FileInfo source, FileInfo file1, FileInfo file2, int percentage1, int percentage2)
        {
            var segrate = new SegregateCSV();
            segrate.Targets.Add(new SegregateTargetPercent(file1, percentage1));
            segrate.Targets.Add(new SegregateTargetPercent(file2, percentage2));
            segrate.ProduceOutputHeaders = true;
            segrate.Analyze(source, true, CSVFormat.English);
            segrate.Process();
        }



        static void Normalize()
        {
            var analyst = new EncogAnalyst();

            var wizard = new AnalystWizard(analyst);
            wizard.Goal = AnalystGoal.Regression;

            wizard.Wizard(Config.BaseFile, true, AnalystFileFormat.DecpntComma);

            int num = 0;
            foreach (var field in analyst.Script.Normalize.NormalizedFields)
            {
                FootballParameters[field.Name] = num;
                if (num < 1)
                {
                    Config.NumberOfTeams = field.Classes.Count - 1;
                }
                if (num > 1)
                {
                    field.Action = Encog.Util.Arrayutil.NormalizationAction.Normalize;
                    Console.WriteLine("Name {0} and number {1}", field.Name, num);
                }
                num++;
            }


            var norm = new AnalystNormalizeCSV();
            norm.ProduceOutputHeaders = true;
            norm.Analyze(Config.TrainingFile, true, CSVFormat.English, analyst);
            norm.Normalize(Config.NormalizedTrainingFile);
            norm.Analyze(Config.EvaluationFile, true, CSVFormat.English, analyst);
            norm.Normalize(Config.NormalizedEvaluationFile);
            norm.Analyze(Config.RealEvaluationFile, true, CSVFormat.English, analyst);
            norm.Normalize(Config.NormalizedRealRunnerFile);
            analyst.Save(Config.AnalysisFile);
        }

        static void CreateNetwork(FileInfo networkFile, int HiddenLayers)
        {
            var network = new BasicNetwork();
            //network.AddLayer(new BasicLayer(null, false, 2));
            int numberOfInput = Config.NumberOfTeams * 2;
            network.AddLayer(new BasicLayer(null, false, numberOfInput));
            network.AddLayer(new BasicLayer(new ActivationTANH(), true, 6));
            network.AddLayer(new BasicLayer(new ActivationTANH(), false, 16));



            network.Structure.FinalizeStructure();
            network.Reset();
            EncogDirectoryPersistence.SaveObject(networkFile, (BasicNetwork)network);
        }

        static void TrainNetwork()
        {
            var network = (BasicNetwork)EncogDirectoryPersistence.LoadObject((Config.TrainedNetworkFile));

            var trainingSet = EncogUtility.LoadCSV2Memory(Config.NormalizedTrainingFile.ToString(),
                network.InputCount, network.OutputCount, true, CSVFormat.English, false);
            var train = new ResilientPropagation(network, trainingSet);

            int epoch = 1;
            var previousError = 100.00;
            var change = 100.0;
            do
            {
                train.Iteration();
                Console.WriteLine("Epoch: {0} Error : {1}", epoch, train.Error);
                epoch++;
                change = (previousError - train.Error) / previousError;
                previousError = train.Error;

            } while (change > 0.001);    // Tensile Strength Elastic Limit

            EncogDirectoryPersistence.SaveObject(Config.TrainedNetworkFile, network);
        }

        static double Evaluate()
        {
            var network = (BasicNetwork)EncogDirectoryPersistence.LoadObject(Config.TrainedNetworkFile);
            var analyst = new EncogAnalyst();
            analyst.Load(Config.AnalysisFile.ToString());
            var evaluationSet = EncogUtility.LoadCSV2Memory(Config.NormalizedEvaluationFile.ToString(),
                network.InputCount, network.OutputCount, true, CSVFormat.English, false);

            double sigmaSum = 0;
            int n = 0;
            using (var file = new System.IO.StreamWriter(Config.ValidationResult.ToString()))
            {
                double correctResults = 0;

                double totalBets = 0;
                double totalWinningBets = 0;
                foreach (var item in evaluationSet)
                {
                    var actualLine = "Actual,";
                    var resultLine = "Predicted,";
                    int p = 0;

       


                    for (int inputIndex = 0; inputIndex < 2; ++inputIndex)
                    {

                        int nTeams = Config.NumberOfTeams;
                        double normMax = analyst.Script.Normalize.NormalizedFields[p].NormalizedHigh;
                        double normMin = analyst.Script.Normalize.NormalizedFields[p].NormalizedLow;
                        var eq = new Encog.MathUtil.Equilateral(nTeams + 1, normMax, normMin);

                        var array = item.InputArray.Take(nTeams).ToArray();
                        if (inputIndex > 0)
                        {
                            array = item.InputArray.Skip(nTeams).ToArray();
                        }
                        var classInt = eq.Decode(array);
                        var className = analyst.Script.Fields[p].ClassMembers[classInt].Name;

                        resultLine = resultLine + className.ToString() + ",";
                        actualLine = actualLine + className.ToString() + ",";

                        p++;
                    }

                    p = 2;
                    bool correctResult = true;

                    double estimatedTotalCorners = 0;
                    double actualTotalCorners = 0;

                    for (var i = 0; i < item.Ideal.Count; ++i)
                    {
                        var NormalizedActualoutput = (BasicMLData)network.Compute(item.Input);

                        var NetworkOutput =
                            analyst.Script.Normalize.NormalizedFields[p].DeNormalize(NormalizedActualoutput.Data[i]);
                        var Actualoutput = analyst.Script.Normalize.NormalizedFields[p].DeNormalize(item.Ideal[i]);

                        resultLine = resultLine + NetworkOutput.ToString() + ",";
                        actualLine = actualLine + Actualoutput.ToString() + ",";

                        if (i < 2)
                        {
                            if (Math.Abs(Math.Round(NetworkOutput) - Math.Round(Actualoutput)) > 0.5)
                            {
                                correctResult = false;
                            }
                            sigmaSum = sigmaSum + (NetworkOutput - Actualoutput) * (NetworkOutput - Actualoutput);
                            n++;
                        }

                        if (p == FootballParameters["HC"] || p == FootballParameters["AC"])
                        {
                            estimatedTotalCorners = estimatedTotalCorners + NetworkOutput;
                            actualTotalCorners = actualTotalCorners + Actualoutput;
                        }

                        p++;
                    }

                    if (estimatedTotalCorners > 12 )
                    {
                        totalBets++;
                        if (actualTotalCorners > 8 && estimatedTotalCorners > 8)
                        {
                            totalWinningBets++;
                        }
                        /*
                        if (actualTotalCorners < 8 && estimatedTotalCorners < 8)
                        {
                            totalWinningBets++;
                        }
                         * */
                    }

                    if (correctResult)
                    {
                        correctResults = correctResults + 1;
                    }
                    file.WriteLine(resultLine);
                    file.WriteLine(actualLine);

                }

                Console.WriteLine("How many corners. Winning Bets {0}, Out Of {1}, gives {2}", totalWinningBets.ToString(), totalBets.ToString(), (totalWinningBets/totalBets).ToString());


                Console.WriteLine("How many correct {0}",correctResults/evaluationSet.Count);

            }



            var newSet = EncogUtility.LoadCSV2Memory(Config.NormalizedRealRunnerFile.ToString(),
              network.InputCount, network.OutputCount, true, CSVFormat.English, false);
            using (var file = new System.IO.StreamWriter(Config.OutputResult.ToString()))
            {

                foreach (var item in newSet)
                {
                    var resultLine = "";
                    int p = 0;

                    string homeTeam = "";
                    string awayTeam = "";
                    for (int inputIndex = 0; inputIndex < 2; ++inputIndex)
                    {
                        int nTeams = Config.NumberOfTeams;
                        double normMax = analyst.Script.Normalize.NormalizedFields[p].NormalizedHigh;
                        double normMin = analyst.Script.Normalize.NormalizedFields[p].NormalizedLow;
                        var eq = new Encog.MathUtil.Equilateral(nTeams + 1, normMax, normMin);

                        var array = item.InputArray.Take(nTeams).ToArray();
                        if (inputIndex > 0)
                        {
                            array = item.InputArray.Skip(nTeams).ToArray();
                        }
                       
                        var classInt = eq.Decode(array);
                        var className = analyst.Script.Fields[p].ClassMembers[classInt].Name;
                        if (inputIndex == FootballParameters["HomeTeam"])
                        {
                            homeTeam = className;
                        }
                        if (inputIndex == FootballParameters["AwayTeam"])
                        {
                            awayTeam = className;
                        }
                        resultLine = resultLine + className.ToString() + ",";
                    }

                    p = 2;
                    var NormalizedActualoutput = (BasicMLData)network.Compute(item.Input);
                    double estimatedTotalCorners = 0;

                    for (var i = 0; i < NormalizedActualoutput.Data.Length; ++i)
                    {

                        var NetworkOutput =
                            analyst.Script.Normalize.NormalizedFields[p].DeNormalize(NormalizedActualoutput.Data[i]);

                        if (p == FootballParameters["HC"] || p == FootballParameters["AC"])
                        {
                            estimatedTotalCorners = estimatedTotalCorners + NetworkOutput;
                            
                        }

                        p++;
                        resultLine = resultLine + NetworkOutput.ToString() + ",";
                    }

                    Console.WriteLine("Estimated total corners {0}", estimatedTotalCorners.ToString());
                    if (estimatedTotalCorners > 12)
                    {
                        Console.WriteLine("Bet On total corners {0} for {1} v {2}", estimatedTotalCorners.ToString(), homeTeam, awayTeam);
                    }

                    file.WriteLine(resultLine);

                }
            }

            return sigmaSum / n;
        }

        public static Dictionary<string, int> FootballParameters { get; set; }
    }
}

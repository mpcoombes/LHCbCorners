using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

            FootballParameters = new Dictionary<int, FootballParameter>();

            FootballMatches = new Dictionary<string, FootballMatch>();

            int hiddenLayers = 0;

            //       System.Threading.Thread.Sleep(100);
            // Step 1
            Shuffle(Config.BaseFile, Config.ShuffledBaseFile);

            // Step 2
            Segregate(Config.ShuffledBaseFile, Config.TrainingFile, Config.EvaluationFile, 100, 0);
            Segregate(Config.RealRunnerFile, Config.RealEvaluationFile, Config.DummyEvaluationFile, 100, 0);

            // Step 3
            Normalize();
            // Step 4

         //   CreateNetwork(Config.TrainedNetworkFile, hiddenLayers);
         //   TrainNetwork();
         //   Evaluate(Config.NormalizedTrainingFile);


            
            for (int i = 0; i < 20; ++i)
            {
             //   System.Threading.Thread.Sleep(10);
                CreateNetwork(Config.TrainedNetworkFile, hiddenLayers);
                // Step 5
                TrainNetwork();
                // Step 6
             //   EvaluateNextMatches(Config.NormalizedTrainingFile);
                EvaluateNextMatches(Config.NormalizedRealRunnerFile);


            }

            AnalyseResults();

           //Evaluate(Config.NormalizedTrainingFile);

            Evaluate(Config.NormalizedRealRunnerFile);

             
            Console.WriteLine("press any key to exit...");
            Console.ReadLine();
        }

        private static void AnalyseResults()
        {
            using (var file = new System.IO.StreamWriter(Config.OutputResult.ToString()))
            {
                foreach (var match in FootballMatches)
                {
                    var line = match.Value.HomeTeam + "," + match.Value.AwayTeam + ",";

                    foreach (var param in match.Value.FootballParameters)
                    {
                        var stdDev = StandardDeviation(param.Value.Result);
                        var avg = param.Value.Result.Average();
                        param.Value.Result.Sort();

                        var most = param.Value.Result.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
                        double count = param.Value.Result.Count(i => i == most);
                        Int32 index = Int32.Parse(Math.Round(param.Value.Result.Count / 2.0).ToString());

                        var median = param.Value.Result[index];

                        param.Value.Median = median;

                        param.Value.Mean = avg;

                        param.Value.Percentage = count/param.Value.Result.Count;

                        param.Value.Mode = most;

                        line = line + param.Value.Name + "," + most + "," + count/param.Value.Result.Count + "," + avg.ToString() + "," + param.Value.ActualResult + ",";

                        if (param.Value.Percentage > 0.8)
                        {
                            line = line + "Bet ,";
                        }
                        else
                        {
                            line = line + "No Bet ,";
                        }

                    }
               
                    file.WriteLine(line);

                }
            }
        }

        private static double StandardDeviation(List<double> results)
        {
            double mean = results.Average(); ;
            double sum = 0;
            foreach (var result in results)
            {
                sum = sum + (result - mean) * (result - mean);
            }
            var stdDev = Math.Sqrt(sum / results.Count);
            return stdDev;
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
            wizard.Goal = AnalystGoal.Classification;

            wizard.Wizard(Config.BaseFile, true, AnalystFileFormat.DecpntComma);

            int num = 0;
            foreach (var field in analyst.Script.Normalize.NormalizedFields)
            {
                FootballParameters[num] = new FootballParameter();
                FootballParameters[num].Name = field.Name;
                FootballParameters[num].ID = num;
                if (FootballParameters[num].Name == "HomeTeam")
                {
                    Config.NumberOfTeams = field.Classes.Count - 1;
                }
                if (FootballParameters[num].Name == "Date")
                {
                    field.Action = Encog.Util.Arrayutil.NormalizationAction.Normalize;
                    //   Console.WriteLine("Name {0} and number {1}", field.Name, num);
                }
             
                num++;
            }

            Config.IdealOutputs = num - Config.NumberOfInputs;


            var norm = new AnalystNormalizeCSV();
            norm.ProduceOutputHeaders = true;
            norm.Analyze(Config.TrainingFile, true, CSVFormat.English, analyst);
            norm.Normalize(Config.NormalizedTrainingFile);
            norm.Analyze(Config.EvaluationFile, true, CSVFormat.English, analyst);
            norm.Normalize(Config.NormalizedEvaluationFile);
            norm.Analyze(Config.RealEvaluationFile, true, CSVFormat.English, analyst);
            norm.Normalize(Config.NormalizedRealRunnerFile);
            analyst.Save(Config.AnalysisFile);
            Config.NumberOfOutputs = 0;
            num = 0;
            foreach (var field in norm._series.Analyst.Script.Fields)
            {
                Console.WriteLine(field.Name);
                FootballParameters[num].Classes = field.ClassMembers.Count;
                num++;
                if (num > 2)
                {
                    Config.NumberOfOutputs = Config.NumberOfOutputs + field.ClassMembers.Count;
                }
            }
        }

        static void CreateNetwork(FileInfo networkFile, int HiddenLayers)
        {
            Network = new BasicNetwork();
            //network.AddLayer(new BasicLayer(null, false, 2));
            int numberOfInput = Config.NumberOfTeams * 2 + 1;
            Network.AddLayer(new BasicLayer(null, false, numberOfInput));
            Network.AddLayer(new BasicLayer(new ActivationTANH(), true, 6));
            Network.AddLayer(new BasicLayer(new ActivationTANH(), true, Config.NumberOfOutputs - Config.IdealOutputs));


            Network.Structure.FinalizeStructure();
            Network.Reset();
            // EncogDirectoryPersistence.SaveObject(networkFile, (BasicNetwork)Network);
        }

        static void TrainNetwork()
        {
            var trainingSet = EncogUtility.LoadCSV2Memory(Config.NormalizedTrainingFile.ToString(),
                Network.InputCount, Network.OutputCount, true, CSVFormat.English, false);
            var train = new ResilientPropagation(Network, trainingSet);

            int epoch = 1;
            var previousError = 100.00;
            var change = 100.0;
            do
            {
                train.Iteration();
                // Console.WriteLine("Epoch: {0} Error : {1}", epoch, train.Error);
                epoch++;
                change = (previousError - train.Error) / previousError;
                previousError = train.Error;

            } while (epoch < 10000);

            //  EncogDirectoryPersistence.SaveObject(Config.TrainedNetworkFile, network);
        }


        private static double Evaluate(FileInfo normalizedEvaluationFile)
        {
            // var network = (BasicNetwork)EncogDirectoryPersistence.LoadObject(Config.TrainedNetworkFile);
            var analyst = new EncogAnalyst();
            analyst.Load(Config.AnalysisFile.ToString());
            var evaluationSet = EncogUtility.LoadCSV2Memory(normalizedEvaluationFile.ToString(),
                Network.InputCount, Network.OutputCount, true, CSVFormat.English, false);


            int itemNumber = 0;
            foreach (var item in evaluationSet)
            {
                var actualLine = "Actual,";
                var resultLine = "Predicted,";
                var homeTeam = "";
                var awayTeam = "";
                int p = 0;

                for (int inputIndex = 0; inputIndex < 2; ++inputIndex)
                {

                    int nTeams = Config.NumberOfTeams;
                    double normMax = analyst.Script.Normalize.NormalizedFields[p].NormalizedHigh;
                    double normMin = analyst.Script.Normalize.NormalizedFields[p].NormalizedLow;
                    var eq = new Encog.MathUtil.Equilateral(nTeams + 1, normMax, normMin);
                    var tempArray = item.InputArray.Take(nTeams * 2).ToArray();
                    var array = tempArray.Take(nTeams).ToArray();
                    if (inputIndex > 0)
                    {
                        array = tempArray.Skip(nTeams).ToArray();
                    }
                    var classInt = eq.Decode(array);
                    var className = analyst.Script.Fields[p].ClassMembers[classInt].Name;

                    resultLine = resultLine + className.ToString() + ",";
                    actualLine = actualLine + className.ToString() + ",";

                    if (FootballParameters[inputIndex].Name == "HomeTeam")
                    {
                        homeTeam = className;
                    }
                    if (FootballParameters[inputIndex].Name == "AwayTeam")
                    {
                        awayTeam = className;
                    }

                    p++;
                }

                p = 3;

                int count = 0;
                for (var i = 0; i < Config.IdealOutputs; ++i)
                {

                    double normMax = analyst.Script.Normalize.NormalizedFields[p].NormalizedHigh;
                    double normMin = analyst.Script.Normalize.NormalizedFields[p].NormalizedLow;
                    int nClasses = FootballParameters[p].Classes;
                    var eq = new Encog.MathUtil.Equilateral(nClasses, normMax, normMin);

                    var NormalizedActualoutput = (BasicMLData)Network.Compute(item.Input);

                    var tempArray = NormalizedActualoutput.Data;
                    tempArray = tempArray.Skip(count).ToArray();
                    tempArray = tempArray.Take(nClasses - 1).ToArray();

                    var idealArray = item.Ideal.Data;
                    idealArray = idealArray.Skip(count).ToArray();
                    idealArray = idealArray.Take(nClasses - 1).ToArray();

                    count = count + nClasses - 1;

                    var classInt = eq.Decode(tempArray);
                    var className = analyst.Script.Fields[p].ClassMembers[classInt].Name;

                    var idealClassInt = eq.Decode(idealArray);

                    var Actualoutput = analyst.Script.Fields[p].ClassMembers[idealClassInt].Name;
                  //  var Actualoutput = analyst.Script.Normalize.NormalizedFields[p].DeNormalize(item.Ideal[i]);

                    FootballParameters[p].ActualResult = Actualoutput;

                    FootballMatches[homeTeam + "," + awayTeam + "," + itemNumber.ToString()].FootballParameters[p].ActualResult = Actualoutput;

                    actualLine = actualLine + Actualoutput.ToString() + ",";


                    if (FootballMatches[homeTeam + "," + awayTeam + "," + itemNumber.ToString()].FootballParameters[p].Percentage > 0.8)
                    {
                        FootballParameters[p].TotalBets++;

                        if (FootballMatches[homeTeam + "," + awayTeam + "," + itemNumber.ToString()].FootballParameters[p].Mode.ToString() == Actualoutput.ToString())
                        {
                            FootballParameters[p].CorrectBets++;
                        }
                    }

                    if (FootballMatches[homeTeam + "," + awayTeam + "," + itemNumber.ToString()].FootballParameters[p].Mode.ToString() == Actualoutput.ToString())
                    {
                        FootballParameters[p].CorrectResults++;
                    }

                  

                    //FootballParameters[p].DiffSum += (NetworkOutput - Actualoutput) * (NetworkOutput - Actualoutput);
                    FootballParameters[p].Entries++;
                    p++;
                }
                itemNumber++;
            }


            using (var file = new System.IO.StreamWriter(Config.OutputTests.ToString()))
            {
                var header = "Name, Percentage Correct No Selection, Percentage Correct w. Selection";
                file.WriteLine(header);
                foreach (var param in FootballParameters)
                {
                    var result = "";
                    if (param.Value.Entries > 0)
                    {
                        var sigma = Math.Sqrt(param.Value.DiffSum/param.Value.Entries);
                        var percentageCorrect = param.Value.CorrectResults/param.Value.Entries;
                        var percentageCorrect2 = param.Value.CorrectBets.ToString() + "/" + param.Value.TotalBets +
                                                 " = " + (param.Value.CorrectBets/param.Value.TotalBets).ToString();

                        result = param.Value.Name + "," + percentageCorrect + "," + percentageCorrect2 + "," + param.Value.Mode.ToString() + "," + param.Value.ActualResult.ToString();
                        file.WriteLine(result);
                        Console.WriteLine("Parameter {0}, RMS {1}, Percentage Correct {2} , {3} , Key {4}",
                            param.Value.Name,
                            sigma.ToString(), percentageCorrect.ToString(), percentageCorrect2.ToString(),
                            param.Key.ToString());

                    }
                }
            }

            return 0;
        }



        private static void EvaluateNextMatches(FileInfo _inputFile)
        {

            var analyst = new EncogAnalyst();
            analyst.Load(Config.AnalysisFile.ToString());
            var newSet = EncogUtility.LoadCSV2Memory(_inputFile.ToString(),
                  Network.InputCount, Network.OutputCount, true, CSVFormat.English, false);

            int itemNumber = 0;
            foreach (var item in newSet)
            {
                var resultLine = "";
                int p = 0;

                string homeTeam = "";
                string awayTeam = "";

                for (int inputIndex = 0; inputIndex < 2; ++inputIndex)
                {
                    int nTeams = Config.NumberOfTeams;
                    var tempArray = item.InputArray.Take(nTeams * 2).ToArray();
                    double normMax = analyst.Script.Normalize.NormalizedFields[p].NormalizedHigh;
                    double normMin = analyst.Script.Normalize.NormalizedFields[p].NormalizedLow;
                    var eq = new Encog.MathUtil.Equilateral(nTeams + 1, normMax, normMin);

                    var array = tempArray.Take(nTeams).ToArray();
                    if (inputIndex > 0)
                    {
                        array = tempArray.Skip(nTeams).ToArray();
                    }

                    var classInt = eq.Decode(array);
                    var className = analyst.Script.Fields[p].ClassMembers[classInt].Name;
                    if (FootballParameters[inputIndex].Name == "HomeTeam")
                    {
                        homeTeam = className;
                    }
                    if (FootballParameters[inputIndex].Name == "AwayTeam")
                    {
                        awayTeam = className;
                    }
                    resultLine = resultLine + className.ToString() + ",";
                }







                if (!FootballMatches.ContainsKey(homeTeam + "," + awayTeam + "," + itemNumber.ToString()))
                {
                    FootballMatches[homeTeam + "," + awayTeam + "," + itemNumber.ToString()] = new FootballMatch();
                }

                FootballMatches[homeTeam + "," + awayTeam + "," + itemNumber.ToString()].HomeTeam = homeTeam;
                FootballMatches[homeTeam + "," + awayTeam + "," + itemNumber.ToString()].AwayTeam = awayTeam;


                FootballMatches[homeTeam + "," + awayTeam + "," + itemNumber.ToString()].ID = itemNumber;

                p = 3;

                int count = 0;
                for (var i = 0; i < Config.IdealOutputs; ++i)
                {
                    if (!FootballMatches[homeTeam + "," + awayTeam + "," + itemNumber.ToString()].FootballParameters.ContainsKey(p))
                    {
                        FootballMatches[homeTeam + "," + awayTeam + "," + itemNumber.ToString()].FootballParameters[p] = new FootballParameter();
                    }

                    FootballMatches[homeTeam + "," + awayTeam + "," + itemNumber.ToString()].FootballParameters[p].Name =
                        FootballParameters[p].Name;

                    double normMax = analyst.Script.Normalize.NormalizedFields[p].NormalizedHigh;
                    double normMin = analyst.Script.Normalize.NormalizedFields[p].NormalizedLow;
                    int nClasses = FootballParameters[p].Classes;
                    var eq = new Encog.MathUtil.Equilateral(nClasses, normMax, normMin);
                    var NormalizedActualoutput = (BasicMLData)Network.Compute(item.Input);



                    var tempArray = NormalizedActualoutput.Data;
                    tempArray = tempArray.Skip(count).ToArray();
                    tempArray = tempArray.Take(nClasses - 1).ToArray();

                    var classInt = eq.Decode(tempArray);
                    var className = analyst.Script.Fields[p].ClassMembers[classInt].Name;
                    int networkOutput = Int32.Parse(className);
                    FootballMatches[homeTeam + "," + awayTeam + "," + itemNumber.ToString()].FootballParameters[p].Result.Add(networkOutput);

                    var idealArray = item.Ideal.Data;
                    idealArray = idealArray.Skip(count).ToArray();
                    idealArray = idealArray.Take(nClasses - 1).ToArray();

                    var idealClassInt = eq.Decode(idealArray);

                    var Actualoutput = analyst.Script.Fields[p].ClassMembers[idealClassInt].Name;
                    FootballMatches[homeTeam + "," + awayTeam + "," + itemNumber.ToString()].FootballParameters[p]
                        .ActualResult = Actualoutput;

                    count = count + nClasses - 1;


                    p++;
                    resultLine = resultLine + className + ",";
                
                }
                itemNumber++;
            }

        }

        public static Dictionary<int, FootballParameter> FootballParameters { get; set; }
        public static Dictionary<string, FootballMatch> FootballMatches { get; set; }

        public static BasicNetwork Network { get; set; }

    }


    internal class FootballMatch
    {

        public FootballMatch()
        {
            HomeTeam = "";
            AwayTeam = "";
            FootballParameters = new Dictionary<int, FootballParameter>();
        }
        public Dictionary<int, FootballParameter> FootballParameters { get; set; }

        public string HomeTeam { get; set; }

        public int ID { get; set; }

        public string AwayTeam { get; set; }

    }
    internal class FootballParameter
    {

        public FootballParameter()
        {
            Entries = 0;
            DiffSum = 0.0;
            CorrectResults = 0;
            CorrectBets = 0;
            TotalBets = 0;
            Result = new List<double>();
        }
        public int ID { get; set; }
        public double Entries { get; set; }
        public string Name { get; set; }
        public double DiffSum { get; set; }
        public double CorrectResults { get; set; }
        public double TotalBets { get; set; }

        public List<double> Result { get; set; }
        public double Median { get; set; }
        public double Mean { get; set; }
        public int Classes { get; set; }
        public double Percentage { get; set; }
        public double Mode { get; set; }
        public double CorrectBets { get; set; }
        public string ActualResult { get; set; }
    }
}

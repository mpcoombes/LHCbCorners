using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encog.App.Analyst.CSV.Segregate;
using Encog.App.Analyst.CSV.Shuffle;
using Encog.Util.CSV;

namespace AttributePredictor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Step 1
            Shuffle(Config.BaseFile);
            // Step 2
            Segregate(Config.ShuffledBaseFile);
            // Step 3
            // Step 4
            // Step 5
            // Step 6
        }

        private static void Shuffle(FileInfo source)
        {
            var shuffle = new ShuffleCSV();
            shuffle.Analyze(source, true, CSVFormat.English);
            shuffle.ProduceOutputHeaders = true;
            shuffle.Process((Config.ShuffledBaseFile));
        }

        private static void Segregate(FileInfo source)
        {
            var segrate = new SegregateCSV();
            segrate.Targets.Add(new SegregateTargetPercent(Config.TrainingFile, 75));
            segrate.Targets.Add(new SegregateTargetPercent(Config.EvaluationFile, 25));
            segrate.ProduceOutputHeaders = true;
            segrate.Analyze(source, true, CSVFormat.English);
            segrate.Process();
        }
    }
}

//
// Encog(tm) Core v3.1 - .Net Version
// http://www.heatonresearch.com/encog/
//
// Copyright 2008-2012 Heaton Research, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//   
// For more information on Heaton Research copyrights, licenses 
// and trademarks visit:
// http://www.heatonresearch.com/copyright
//
using System;
using System.Collections.Generic;
using Encog.MathUtil.RBF;
using Encog.ML.Factory.Parse;
using Encog.Neural;
using Encog.Neural.RBF;
using Encog.Util;

namespace Encog.ML.Factory.Method
{
    /// <summary>
    /// A factory to create RBF networks.
    /// </summary>
    ///
    public class RBFNetworkFactory
    {
        /// <summary>
        /// The max layer count.
        /// </summary>
        ///
        public const int MaxLayers = 3;

        /// <summary>
        /// Create a RBF network.
        /// </summary>
        ///
        /// <param name="architecture">THe architecture string to use.</param>
        /// <param name="input">The input count.</param>
        /// <param name="output">The output count.</param>
        /// <returns>The RBF network.</returns>
        public IMLMethod Create(String architecture, int input,
                               int output)
        {
            IList<String> layers = ArchitectureParse.ParseLayers(architecture);
            if (layers.Count != MaxLayers)
            {
                throw new EncogError(
                    "RBF Networks must have exactly three elements, "
                    + "separated by ->.");
            }

            ArchitectureLayer inputLayer = ArchitectureParse.ParseLayer(
                layers[0], input);
            ArchitectureLayer rbfLayer = ArchitectureParse.ParseLayer(
                layers[1], -1);
            ArchitectureLayer outputLayer = ArchitectureParse.ParseLayer(
                layers[2], output);

            int inputCount = inputLayer.Count;
            int outputCount = outputLayer.Count;

            RBFEnum t;

            if (rbfLayer.Name.Equals("Gaussian", StringComparison.InvariantCultureIgnoreCase))
            {
                t = RBFEnum.Gaussian;
            }
            else if (rbfLayer.Name.Equals("Multiquadric", StringComparison.InvariantCultureIgnoreCase))
            {
                t = RBFEnum.Multiquadric;
            }
            else if (rbfLayer.Name.Equals("InverseMultiquadric", StringComparison.InvariantCultureIgnoreCase))
            {
                t = RBFEnum.InverseMultiquadric;
            }
            else if (rbfLayer.Name.Equals("MexicanHat", StringComparison.InvariantCultureIgnoreCase))
            {
                t = RBFEnum.MexicanHat;
            }
            else
            {
                throw new NeuralNetworkError("Unknown RBF: " + rbfLayer.Name);
            }

            var holder = new ParamsHolder(rbfLayer.Params);

            int rbfCount = holder.GetInt("C", true, 0);

            var result = new RBFNetwork(inputCount, rbfCount,
                                        outputCount, t);

            return result;
        }
    }
}

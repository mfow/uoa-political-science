using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS380
{
    public static class PrincipalComponentAnalysis
    {
        public static double DotProduct(double[] A, double[] B, int startA, int startB, int length)
        {
            double result = 0.0;

            for (int i = 0; i < length; i++)
            {
                result += A[i + startA] * B[i + startB];
            }

            return result;
        }

        public static void Normalize(double[][] channels, int length, out double[] channelMeans, out double[] channelSDs)
        {
            channelMeans = new double[channels.Length];
            channelSDs = new double[channels.Length];

            for (int channelIndex = 0; channelIndex < channels.Length; channelIndex++)
            {
                var channel = channels[channelIndex];

                var mean = (from sample in channel select sample).Take(length).Average();
                channelMeans[channelIndex] = mean;

                var SD = Math.Pow((from sample in channel select Math.Pow(sample - mean, 2.0)).Take(length).Average(), 0.5);
                channelSDs[channelIndex] = SD;

                for (int i = 0; i < length; i++)
                {
                    channel[i] = (channel[i] - mean) / SD;
                }
            }
        }

        public class PCAResults
        {
            public double[][] Matrix { get; set; }
            public double[] VarianceExplainedByComponent { get; set; }
            public List<double[]> MeansByStage { get; set; }
            public List<double[]> StandardDeviationByStage { get; set; }

            public PCAResults()
            {
                MeansByStage = new List<double[]>();
                StandardDeviationByStage = new List<double[]>();
            }

            public double[] TransformSample(double[] sample)
            {
                var sample2 = sample.ToArray();
                var result = new double[MeansByStage.Count];

                for (int stage = 0; stage < result.Length; stage++)
                {
                    var zScores = new double[MeansByStage.Count];

                    for (int i = 0; i < sample2.Length; i++)
                    {
                        zScores[i] = (sample2[i] - MeansByStage[stage][i]) / StandardDeviationByStage[stage][i];
                    }

                    double dotProduct = 0.0;

                    for (int i = 0; i < sample2.Length; i++)
                    {
                        dotProduct += zScores[i] * Matrix[stage][i];
                    }

                    result[stage] = dotProduct;
                }

                return result;
            }

            public double[] BackTransformSample(double[] sample)
            {
                var result = new double[sample.Length];

                for (int i = 0; i < sample.Length; i++)
                {
                    result[i] = MeansByStage[0][i];
                }

                for (int stage = 0; stage < result.Length; stage++)
                {
                    double zScoreInitial = 0.0; // Should be = 0 in theory.

                    for (int i = 0; i < result.Length; i++)
                    {
                        zScoreInitial += ((result[i] - MeansByStage[stage][i]) / StandardDeviationByStage[stage][i]) * Matrix[stage][i];
                    }

                    var zScoreTarget = sample[stage];
                    var zScoreAdd = zScoreTarget - zScoreInitial;

                    for (int i = 0; i < sample.Length; i++)
                    {
                        double dotProduct = zScoreAdd * Matrix[stage][i] * StandardDeviationByStage[stage][i];

                        result[i] += dotProduct;
                    }


                    double zScoreFinal = 0.0;

                    for (int i = 0; i < result.Length; i++)
                    {
                        zScoreFinal += ((result[i] - MeansByStage[stage][i]) / StandardDeviationByStage[stage][i]) * Matrix[stage][i];
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Calculates a matrix which transforms a set of signals to a the principal components of those signals.
        /// </summary>
        /// <param name="channels"></param>
        /// <param name="length"></param>
        /// <returns>Returns a matrix which transforms the channels (after being normalized) into the principal components</returns>
        public static PCAResults CalculateMatrix(double[][] channels, int length)
        {
            var result = new PCAResults();

            // Initilize Matrix and copy channels.
            double[][] matrix = new double[channels.Length][];
            double[][] channels2 = new double[channels.Length][];

            for (int i = 0; i < channels.Length; i++)
            {
                matrix[i] = new double[channels.Length];
                channels2[i] = channels[i].ToArray();
            }

            double[] sdSumByComponent = new double[channels.Length + 1]; // We add an extra "null component" to make our following code easier.

            // Iterate to calculate it.
            for (int componentIndex = 0; componentIndex < channels.Length; componentIndex++)
            {
                var p = InternalCalculateP(channels2, length);
                matrix[componentIndex] = p.ToArray();

                // Subtract the current component from the data.

                double[] channelMeans = (from currentChannel in channels2 select currentChannel.Average()).ToArray();
                double[] channelSDs = new double[channels.Length];

                for (int channelIndex = 0; channelIndex < channels.Length; channelIndex++)
                {
                    channelSDs[channelIndex] = Math.Pow((from sample in channels2[channelIndex] select Math.Pow((sample - channelMeans[channelIndex]), 2.0)).Average(), 0.5);
                }

                sdSumByComponent[componentIndex] = (from x in channelSDs select x * x).Sum();

                result.MeansByStage.Add(channelMeans.ToArray());
                result.StandardDeviationByStage.Add(channelSDs.ToArray());

                for (int sampleIndex = 0; sampleIndex < length; sampleIndex++)
                {
                    double dotProduct = 0.0;

                    for (int channelIndex = 0; channelIndex < channels.Length; channelIndex++)
                    {
                        var channel = channels2[channelIndex];

                        double sample = channel[sampleIndex];

                        // Normalize sample.
                        var sample_n = (sample - channelMeans[channelIndex]) / channelSDs[channelIndex];

                        dotProduct += sample_n * p[channelIndex];

                        channel[sampleIndex] = sample;
                    }

                    for (int channelIndex = 0; channelIndex < channels.Length; channelIndex++)
                    {
                        var channel = channels2[channelIndex];

                        double sample = channel[sampleIndex];
                        double sample_n = (sample - channelMeans[channelIndex]) / channelSDs[channelIndex];

                        // Subtract x*p.
                        sample_n -= p[channelIndex] * dotProduct;

                        double sample_p = (sample_n * channelSDs[channelIndex]) + channelMeans[channelIndex];

                        channel[sampleIndex] = sample_p;
                    }
                }
            }

            double[] varExplainedByComponent = new double[channels.Length];

            //Calculate the portion of variance explained by component.
            for (int componentIndex = 0; componentIndex < channels.Length; componentIndex++)
            {
                varExplainedByComponent[componentIndex] = (sdSumByComponent[componentIndex] - sdSumByComponent[componentIndex + 1]) / sdSumByComponent[0];
            }

            result.Matrix = matrix;
            result.VarianceExplainedByComponent = varExplainedByComponent;

            return result;
        }

        private static double[] InternalCalculateP(double[][] channels, int length)
        {
            // Calculate mean and SD.
            double[] channelMean = (from channel in channels select channel.Average()).ToArray();
            double[] channelSD = new double[channels.Length];

            for (int i = 0; i < channels.Length; i++)
            {
                double mean = channelMean[i];
                channelSD[i] = Math.Sqrt((from sample in channels[i] select Math.Pow((sample - mean), 2.0)).Average());
            }

            // Create a random vector to represent the transformation.
            Random r = new Random();

            double[] p = new double[channels.Length];
            for (int i = 0; i < channels.Length; i++)
            {
                p[i] = (r.NextDouble() * 2.0) - 1.0;
            }

            // Align p.
            for (int c = 0; c < 100; c++)
            {
                double[] t = new double[channels.Length];

                for (int sample = 0; sample < length; sample++)
                {
                    double dotProduct = 0.0;

                    for (int i = 0; i < channels.Length; i++)
                    {
                        dotProduct += ((channels[i][sample] - channelMean[i]) / channelSD[i]) * p[i];
                    }

                    for (int i = 0; i < channels.Length; i++)
                    {
                        t[i] += dotProduct * ((channels[i][sample] - channelMean[i]) / channelSD[i]);
                    }
                }

                double tAbsolute = Math.Pow((from element in t select element * element).Sum(), 0.5);
                p = (from element in t select element / tAbsolute).ToArray();
            }

            //// Correct for standard deviation equal to zero.
            //// (We assume mean is already zero as it is corrected above)
            //double pSD = Math.Sqrt((from element in p select element * element).Average());
            //p = (from element in p select element / pSD).ToArray();

            return p;
        }
    }
}

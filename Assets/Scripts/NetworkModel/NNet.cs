using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using Random = UnityEngine.Random;

namespace NetworkModel
{
    public class NNet
    {
        public List<Matrix<float>> layers;
        public List<Matrix<float>> weights;
        public List<float> biases;

        public float fitness;

        public NNet(params int[] neuronCounts)
        {
            var layerCount = neuronCounts.Length;

            layers = new List<Matrix<float>>();
            weights = new List<Matrix<float>>();
            biases = new List<float>();

            for (int i = 0; i < layerCount; i++)
            {
                var layer = Matrix<float>.Build.Dense(1, neuronCounts[i]);
                layers.Add(layer);

                if (i > 0)
                {
                    var weight = Matrix<float>.Build.Dense(neuronCounts[i - 1], neuronCounts[i]);
                    weights.Add(weight);
                }
            }
        }

        public void Randomize()
        {
            foreach (var weight in weights)
            {
                biases.Add(Random.Range(-1f, 1f));

                for (int x = 0; x < weight.RowCount; x++)
                    for (int y = 0; y < weight.ColumnCount; y++)
                        weight[x, y] = Random.Range(-1f, 1f);
            }
        }

        public float[] RunNetwork(params float[] inputs)
        {
            for (int i = 0; i < inputs.Length; i++)
                layers[0][0, i] = inputs[i];

            layers[0] = layers[0].PointwiseTanh();

            for (int i = 1; i < layers.Count; i++)
                layers[i] = ((layers[i - 1] * weights[i - 1]) + biases[i - 1]).PointwiseTanh();

            return layers[layers.Count - 1].ToRowMajorArray();
        }

        public void Crossover(NNet a, NNet b)
        {
            for (int i = 0; i < a.weights.Count; i++)
            {
                if (Random.value < 0.5f)
                    weights[i] = a.weights[i].Clone();
                else
                    weights[i] = b.weights[i].Clone();
            }

            for (int i = 0; i < a.biases.Count; i++)
            {
                if (Random.value < 0.5f)
                    biases.Add(a.biases[i]);
                else
                    biases.Add(b.biases[i]);
            }
        }

        public void Mutate(float mutationRate)
        {
            for (int i = 0; i < weights.Count; i++)
            {
                if (Random.value <= mutationRate)
                {
                    var mututed = weights[i];
                    int mutatedPointCount = Random.Range(1, (mututed.RowCount * mututed.ColumnCount) / 7);

                    for (int j = 0; j < mutatedPointCount; j++)
                    {
                        int randomColumn = Random.Range(0, mututed.ColumnCount);
                        int randomRow = Random.Range(0, mututed.RowCount);

                        mututed[randomRow, randomColumn] = Mathf.Clamp(mututed[randomRow, randomColumn] + Random.Range(-1f, 1f), -1f, 1f);
                    }

                    weights[i] = mututed;
                }
            }
        }

        public void Save()
        {
            var data = new NNetData(weights, biases);
            var json = JsonUtility.ToJson(data);
            var path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\";
            File.WriteAllText(path + "/" + DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss") + ".json", json);
        }

        public void Load(TextAsset networkFile)
        {
            var data = JsonUtility.FromJson<NNetData>(networkFile.text);

            weights = data.GetWeights();
            biases = data.GetBiases();

            layers = new List<Matrix<float>>();
            for (int i = 0; i < weights.Count; i++)
            {
                var layer = Matrix<float>.Build.Dense(1, weights[i].RowCount);
                layers.Add(layer);
            }
            var outputLayer = Matrix<float>.Build.Dense(1, weights[weights.Count - 1].ColumnCount);
            layers.Add(outputLayer);
        }
    }
}
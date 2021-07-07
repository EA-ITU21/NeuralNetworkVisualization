using UnityEngine;
using System.Collections.Generic;
using NetworkModel;

namespace Visualisation
{
    public class VisualisationManager : MonoBehaviour
    {
        public Transform layersParent;

        public Transform networkLabelParent;
        public VisualisationCamera visualisationCamera;

        public Neuron neuronPrefab;
        public LineRenderer positiveWeightPrefab;
        public LineRenderer negativeWeightPrefab;
        public NeuronLabel neuronLabelPrefab;

        private NNet network;
        private List<List<Neuron>> layers;
        private List<NeuronLabel> neuronLabels;

        private VisualState currentVisualState = VisualState.AllVisible;
        private VisualState previousVisualState = VisualState.AllVisible;
        private HashSet<Vector2Int> toggledNeurons = new HashSet<Vector2Int>();

        public const float neuronInterval = 25f;
        private const float layerInterval = 50f;

        public void ResetWithNetwork(NNet network, string[] inputLabels, string[] outputLabels)
        {
            this.network = network;

            Reset();
            CreateLayers();
            CreateWeights();
            CreateNeuronLabels(inputLabels, outputLabels);
            ToggleHiddenNeurons();
        }

        void Reset()
        {
            var childCount = layersParent.childCount;
            for (int i = 0; i < childCount; i++)
                Destroy(layersParent.GetChild(i).gameObject);

            if (layers != null)
            {
                foreach (var layer in layers)
                    layer.Clear();
                layers.Clear();
            }

            if (neuronLabels != null)
            {
                foreach (var label in neuronLabels)
                    Destroy(label.gameObject);
                neuronLabels.Clear();
            }
        }

        void CreateLayers()
        {
            layers = new List<List<Neuron>>();

            for (int i = 0; i < network.layers.Count; i++)
            {
                var layer = new List<Neuron>();

                var layerGO = new GameObject("Layer" + i.ToString()).transform;
                layerGO.SetParent(layersParent);
                layerGO.position = layersParent.position + layerInterval * i * Vector3.right;

                var layerHeight = (network.layers[i].ColumnCount - 1) * neuronInterval;
                var top = layerGO.position + Vector3.up * layerHeight / 2; 
                for (int j = 0; j < network.layers[i].ColumnCount; j++)
                {
                    var neuronPosition = top + neuronInterval * j * Vector3.down;
                    var neuron = Instantiate(neuronPrefab, neuronPosition, Quaternion.identity, layerGO);
                    neuron.Initialize(i, j, this, visualisationCamera);
                    layer.Add(neuron);
                }

                layers.Add(layer);
            }
        }

        void CreateWeights()
        {
            for (int i = 0; i < layers.Count - 1; i++)
            {
                for (int j = 0; j < layers[i].Count; j++)
                {
                    for (int k = 0; k < layers[i + 1].Count; k++)
                    {
                        var weight = network.weights[i][j, k];
                        layers[i][j].Connect(layers[i + 1][k], layers[i].Count, weight > 0 ? positiveWeightPrefab : negativeWeightPrefab, weight);
                    }
                }
            }
        }

        void CreateNeuronLabels(string[] inputLabels, string[] outputLabels)
        {
            neuronLabels = new List<NeuronLabel>();

            var inputLayer = layers[0];
            for (int i = 0; i < inputLayer.Count; i++)
            {
                var label = Instantiate(neuronLabelPrefab, networkLabelParent);
                label.Initialize(inputLayer[i], inputLabels[i], true, visualisationCamera);
                neuronLabels.Add(label);
            }

            var outputLayer = layers[layers.Count - 1];
            for (int i = 0; i < outputLayer.Count; i++)
            {
                var label = Instantiate(neuronLabelPrefab, networkLabelParent);
                label.Initialize(outputLayer[i], outputLabels[i], false, visualisationCamera);
                neuronLabels.Add(label);
            }
        }

        public void UpdateNetwork()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                for (int j = 0; j < layers[i].Count; j++)
                {
                    layers[i][j].UpdateNeuron(network.layers[i][0, j]);
                }
            }
        }

        void ToggleHiddenNeurons()
        {
            if (currentVisualState == VisualState.AllHidden)
            {
                for (int i = 1; i < layers.Count; i++)
                {
                    for (int j = 0; j < layers[i].Count; j++)
                    {
                        ToggleNeuron(i, j, false);
                    }
                }
            }
            else if (currentVisualState == VisualState.Mixed)
            {
                if (previousVisualState == VisualState.AllVisible)
                {
                    foreach (var neuron in toggledNeurons)
                    {
                        ToggleNeuron(neuron.x, neuron.y, false);
                    }
                }
                else if (previousVisualState == VisualState.AllHidden)
                {
                    for (int i = 1; i < layers.Count; i++)
                    {
                        for (int j = 0; j < layers[i].Count; j++)
                        {
                            if (!toggledNeurons.Contains(new Vector2Int(i, j)))
                            {
                                ToggleNeuron(i, j, false);
                            }
                        }
                    }
                }
            }
        }

        public void OnShowAllButtonClicked()
        {
            currentVisualState = VisualState.AllVisible;

            for (int i = 1; i < layers.Count; i++)
            {
                for (int j = 0; j < layers[i].Count; j++)
                {
                    ToggleNeuron(i, j, true);
                }
            }
            toggledNeurons.Clear();
        }

        public void OnHideAllButtonClicked()
        {
            currentVisualState = VisualState.AllHidden;

            for (int i = 1; i < layers.Count; i++)
            {
                for (int j = 0; j < layers[i].Count; j++)
                {
                    ToggleNeuron(i, j, false);
                }
            }
            toggledNeurons.Clear();
        }

        public void OnNeuronClicked(int layerIndex, int neuronIndex)
        {
            if (currentVisualState == VisualState.AllVisible)
            {
                ToggleNeuron(layerIndex, neuronIndex, false);
                toggledNeurons.Add(new Vector2Int(layerIndex, neuronIndex));
                previousVisualState = VisualState.AllVisible;
                currentVisualState = VisualState.Mixed;
            }
            else if (currentVisualState == VisualState.AllHidden)
            {
                ToggleNeuron(layerIndex, neuronIndex, true);
                toggledNeurons.Add(new Vector2Int(layerIndex, neuronIndex));
                previousVisualState = VisualState.AllHidden;
                currentVisualState = VisualState.Mixed;
            }
            else if (currentVisualState == VisualState.Mixed)
            {
                if (!toggledNeurons.Contains(new Vector2Int(layerIndex, neuronIndex)))
                {
                    if (previousVisualState == VisualState.AllVisible)
                    {
                        ToggleNeuron(layerIndex, neuronIndex, false);
                    }
                    else if (previousVisualState == VisualState.AllHidden)
                    {
                        ToggleNeuron(layerIndex, neuronIndex, true);
                    }
                    toggledNeurons.Add(new Vector2Int(layerIndex, neuronIndex));
                }
                else
                {
                    if (previousVisualState == VisualState.AllVisible)
                    {
                        ToggleNeuron(layerIndex, neuronIndex, true);
                    }
                    else if (previousVisualState == VisualState.AllHidden)
                    {
                        ToggleNeuron(layerIndex, neuronIndex, false);
                    }
                    toggledNeurons.Remove(new Vector2Int(layerIndex, neuronIndex));
                }
            }
        }

        void ToggleNeuron(int layerIndex, int neuronIndex, bool toggle)
        {
            if (layerIndex > 0)
            {
                foreach (var neuron in layers[layerIndex - 1])
                {
                    neuron.weightLines[neuronIndex].gameObject.SetActive(toggle);
                }
            }
        }
    }
}
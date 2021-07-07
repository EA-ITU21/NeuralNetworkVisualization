using UnityEngine;
using System;
using System.Collections.Generic;

namespace Visualisation
{
    public class Neuron : MonoBehaviour
    {
        public static Action OnNeuronRightClicked;

        public Transform output;
        public Renderer outputRenderer;
        [HideInInspector]
        public List<LineRenderer> weightLines = new List<LineRenderer>();

        private int layerIndex;
        private int neuronIndex;
        private VisualisationManager visualisationManager;
        private VisualisationCamera visualisationCamera;

        private bool leftMouseClickedDown;
        private bool rightMouseClickedDown;

        private const float neuronWidth = 25f;
        private const float outputOffset = 2f;
        private const float inputOffset = 2f;
        private const float inputExcess = 1f;

        public void Initialize(int layerIndex, int neuronIndex, VisualisationManager visualisationManager, VisualisationCamera visualisationCamera)
        {
            this.layerIndex = layerIndex;
            this.neuronIndex = neuronIndex;
            this.visualisationManager = visualisationManager;
            this.visualisationCamera = visualisationCamera;
        }

        public void Connect(Neuron target, int inputCount, LineRenderer weightPrefab, float weight)
        {
            var weightLine = Instantiate(weightPrefab, output.position, Quaternion.identity, output);
            weightLine.positionCount = 4;
            weightLine.SetPosition(1, Vector3.right * outputOffset);

            var back = target.transform.position + neuronWidth / 2 * Vector3.forward;
            var inputPosition = back + (neuronIndex + 0.5f) * neuronWidth / inputCount * Vector3.back;
            weightLine.SetPosition(2, (inputPosition + Vector3.left * inputOffset) - output.position);
            weightLine.SetPosition(3, (inputPosition + Vector3.right * inputExcess) - output.position);

            weightLine.startWidth = Mathf.Abs(weight);

            weightLines.Add(weightLine);
        }

        public void UpdateNeuron(float value)
        {
            var outputColor = GetHeatMapColor(value);
            outputRenderer.material.color = outputColor;

            foreach (var weightLine in weightLines)
            {
                weightLine.startColor = outputColor;
                weightLine.endColor = outputColor;
            }
        }

        private Color GetHeatMapColor(float value)
        {
            if (value >= 0.25f)
            {
                return Color.Lerp(Color.yellow, Color.red, Mathf.InverseLerp(0.25f, 1f, value));
            }
            else if (value >= 0)
            {
                return Color.Lerp(Color.green, Color.yellow, Mathf.InverseLerp(0, 0.25f, value));
            }
            else if (value >= -0.25f)
            {
                return Color.Lerp(Color.cyan, Color.green, Mathf.InverseLerp(-0.25f, 0, value));
            }
            else
            {
                return Color.Lerp(Color.blue, Color.cyan, Mathf.InverseLerp(-1, -0.25f, value));
            }
        }

        private void OnMouseOver()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                leftMouseClickedDown = true;
            }
            else if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                rightMouseClickedDown = true;
            }

            if (Input.GetKeyUp(KeyCode.Mouse0) && leftMouseClickedDown)
            {
                visualisationManager.OnNeuronClicked(layerIndex, neuronIndex);
            }
            else if (Input.GetKeyUp(KeyCode.Mouse1) && rightMouseClickedDown)
            {
                visualisationCamera.FocusOnNeuron(transform.position);
            }
        }

        private void OnMouseExit()
        {
            leftMouseClickedDown = false;
            rightMouseClickedDown = false;
        }
    }
}
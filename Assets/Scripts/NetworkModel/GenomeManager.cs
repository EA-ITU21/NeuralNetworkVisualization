using System.Collections.Generic;
using UnityEngine;
using UI;
using UnityEngine.UI;

namespace NetworkModel
{
    public class GenomeManager : MonoBehaviour
    {
        public CarController carController;
        public RunMode runMode;

        public int[] neuronCounts = { 3, 10, 10, 2 };

        public int populationCount = 85;
        public int bestAgentSelection = 8;
        public int worstAgentSelection = 3;
        public int crossoverCount = 39;
        [Range(0.0f, 1.0f)]
        public float mutationRate = 0.055f;

        public TextAsset[] testNetworks;

        public GameObject episodeInfoPanel;
        public Text generationNumberText;
        public Text episodeNumberText;
        public GameObject saveNetworkButton;

        public Transform testNetworkSelectionPanel;
        public TestNetworkSelectionButton testNetworkSelectionButtonPrefab;

        private NNet[] population;

        private List<TestNetworkSelectionButton> testNetworkSelectionButtons;

        private int currentGeneration;
        private int currentGenome;

        #region Singleton
        public static GenomeManager instance;

        void Awake()
        {
            instance = this;
        }
        #endregion

        public void Start()
        {
            if (runMode == RunMode.Test)
            {
                CreateTestNetworkSelectionButtons();
                ApplyTestNetwork(currentGenome);
            }
            else if (runMode == RunMode.Training)
            {
                CreatePopulation();
                ApplyCurrentGenome();

                episodeInfoPanel.SetActive(true);
                saveNetworkButton.SetActive(true);
            }
        }

        void ApplyTestNetwork(int index)
        {
            var network = new NNet();
            network.Load(testNetworks[index]);
            currentGenome = index;
            testNetworkSelectionButtons[index].SetSelected(true);

            carController.ResetWithNetwork(network, true);
        }

        void ApplyCurrentGenome()
        {
            carController.ResetWithNetwork(population[currentGenome], false);
            UpdateGenomeInfo();
        }

        public void EvaluateCurrentGenome(float fitness)
        {
            if (runMode == RunMode.Test)
            {
                ApplyTestNetwork(currentGenome);
                return;
            }

            population[currentGenome].fitness = fitness;

            currentGenome++;
            if (currentGenome >= population.Length)
            {
                Repopulate();
                currentGenome = 0;
            }

            ApplyCurrentGenome();
        }

        #region Test
        void CreateTestNetworkSelectionButtons()
        {
            testNetworkSelectionPanel.gameObject.SetActive(true);
            testNetworkSelectionButtons = new List<TestNetworkSelectionButton>();

            for (int i = 0; i < testNetworks.Length; i++)
            {
                var testNetworkSelectionButton = Instantiate(testNetworkSelectionButtonPrefab, testNetworkSelectionPanel);
                testNetworkSelectionButton.Initialize(i, this);
                testNetworkSelectionButtons.Add(testNetworkSelectionButton);
            }
        }

        public void OnTestNetworkButtonClicked(int index)
        {
            testNetworkSelectionButtons[currentGenome].SetSelected(false);
            ApplyTestNetwork(index);
        }
        #endregion

        #region Training
        void CreatePopulation()
        {
            population = new NNet[populationCount];
            RandomizePopulation(population, 0, population.Length);
        }

        void RandomizePopulation(NNet[] population, int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                population[i] = new NNet(neuronCounts);
                population[i].Randomize();
            }
        }

        void Repopulate()
        {
            SortPopulation();

            var newPopulation = new NNet[populationCount];
            var genePool = new List<int>();

            SelectGenomesNaturally(newPopulation, genePool, out int endIndex);
            Crossover(newPopulation, endIndex, genePool, out endIndex);
            Mutate(newPopulation, endIndex);
            RandomizePopulation(newPopulation, endIndex, population.Length);

            genePool.Clear();
            currentGeneration++;
            population = newPopulation;
        }

        void SelectGenomesNaturally(NNet[] newPopulation, List<int> geneIndexPool, out int endIndex)
        {
            endIndex = 0;

            for (int i = 0; i < bestAgentSelection; i++)
            {
                if (i == 0)
                {
                    newPopulation[i] = population[i];
                    endIndex++;
                }
                
                geneIndexPool.Add(i);
            }

            for (int i = 0; i < worstAgentSelection; i++)
            {
                geneIndexPool.Add(population.Length - 1 - i);
            }
        }

        void Crossover(NNet[] newPopulation, int startIndex, List<int> geneIndexPool, out int endIndex)
        {
            int index = startIndex;

            for (int i = 0; i < crossoverCount; i++)
            {
                int a = 0;
                int b = 0;

                for (int l = 0; l < 100; l++)
                {
                    a = geneIndexPool[Random.Range(0, geneIndexPool.Count)];
                    b = geneIndexPool[Random.Range(0, geneIndexPool.Count)];

                    if (a != b)
                        break;
                }

                if (a == b)
                    continue;

                newPopulation[index] = new NNet(neuronCounts);
                newPopulation[index].Crossover(population[a], population[b]);
                index++;
            }
            endIndex = index;
        }

        void Mutate(NNet[] newPopulation, int endIndex)
        {
            for (int i = bestAgentSelection; i < endIndex; i++)
            {
                newPopulation[i].Mutate(mutationRate);
            }
        }

        void SortPopulation()
        {
            for (int i = 0; i < population.Length; i++)
            {
                for (int j = i; j < population.Length; j++)
                {
                    if (population[i].fitness < population[j].fitness)
                    {
                        var temp = population[i];
                        population[i] = population[j];
                        population[j] = temp;
                    }
                }
            }
        }

        void UpdateGenomeInfo()
        {
            generationNumberText.text = string.Format("Generation: {0}", currentGeneration + 1);
            episodeNumberText.text = string.Format("Episode: {0}/{1}", currentGenome + 1, populationCount);
        }

        public void OnSaveNetworkButtonClicked()
        {
            population[currentGenome].Save();
        }
        #endregion
    }
}
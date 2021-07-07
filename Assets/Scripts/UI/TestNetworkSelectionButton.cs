using UnityEngine;
using UnityEngine.UI;
using NetworkModel;

namespace UI
{
    public class TestNetworkSelectionButton : MonoBehaviour
    {
        public Text text;
        public Image image;

        public Color selectedColor;
        private  Color startColor;

        private int index;
        private GenomeManager genomeManager;

        public void Initialize(int index, GenomeManager genomeManager)
        {
            this.index = index;
            this.genomeManager = genomeManager;

            text.text = string.Format("Network {0}", index + 1);
            startColor = image.color;
        }

        public void OnClicked()
        {
            genomeManager.OnTestNetworkButtonClicked(index);
        }

        public void SetSelected(bool selected)
        {
            image.color = selected ? selectedColor : startColor;
        }
    }
}
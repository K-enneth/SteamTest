using System.Collections.Generic;
using UnityEngine;

namespace SteamLobbyTutorial
{
    public class PanelSwap : MonoBehaviour
    {
        public List<Panel> panels = new List<Panel>();

        public void SwapPanels(string panelName)
        {
            for (int i = 0; i < panels.Count; i++)
            {
                if (panels[i].panelName == panelName)
                {
                    panels[i].gameObject.SetActive(true);
                }
                else
                {
                    panels[i].gameObject.SetActive(false);
                }
            }
        }
    }
    
}

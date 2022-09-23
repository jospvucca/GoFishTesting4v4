using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GoFish
{
    public class ButtonPlayerConfiguration : MonoBehaviour
    {
        public Button[] playerButtons;

        public void AsignPlayersToButtons(string player1Name, string player2Name, string player3Name)
        {
            playerButtons[0].GetComponentInChildren<Text>().text = player1Name;
            playerButtons[1].GetComponentInChildren<Text>().text = player2Name;
            playerButtons[2].GetComponentInChildren<Text>().text = player3Name;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SWNetwork;

namespace GoFish
{
    public class Lobby : MonoBehaviour
    {
        public enum LobbyState
        {
            Default,
            JoinedRoom,
        }
        public LobbyState State = LobbyState.Default;
        public bool Debugging = false;

        public GameObject PopoverBackground;
        public GameObject EnterNicknamePopover;
        public GameObject WaitForOpponentPopover;
        public GameObject StartRoomButton;
        public InputField NicknameInputField;

        public GameObject[] PlayerPortraits;
        //public GameObject Player2Portrait;

        string nickname;

        private void Start()
        {
            // disable all online UI elements
            HideAllPopover();
            NetworkClient.Lobby.OnLobbyConnectedEvent += OnLobbyConnected;
            NetworkClient.Lobby.OnNewPlayerJoinRoomEvent += OnNewPlayerJoinRoomEvent;
            NetworkClient.Lobby.OnRoomReadyEvent += OnRoomReadyEvent;
        }

        private void OnDestroy()
        {
            //izbacuje error ali nije game breaking ja mislin, to sa ifon rjesit(pogledat rjs)
            NetworkClient.Lobby.OnLobbyConnectedEvent -= OnLobbyConnected;
            NetworkClient.Lobby.OnNewPlayerJoinRoomEvent -= OnNewPlayerJoinRoomEvent;
            NetworkClient.Lobby.OnRoomReadyEvent -= OnRoomReadyEvent;
        }

        void ShowEnterNicknamePopover()
        {
            PopoverBackground.SetActive(true);
            EnterNicknamePopover.SetActive(true);
        }

        void ShowJoinedRoomPopover()
        {
            EnterNicknamePopover.SetActive(false);
            WaitForOpponentPopover.SetActive(true);
            StartRoomButton.SetActive(false);
            foreach (GameObject portrait in PlayerPortraits)
            {
                portrait.SetActive(false);
            }
        }

        void ShowReadyToStartUI()
        {
            StartRoomButton.SetActive(true);
            for (int i = 0; i < GetNumberOfPlayersInTheRoom(); i++)
            {
                PlayerPortraits[i].SetActive(true);
            }
        }

        void HideAllPopover()
        {
            PopoverBackground.SetActive(false);
            EnterNicknamePopover.SetActive(false);
            WaitForOpponentPopover.SetActive(false);
            StartRoomButton.SetActive(false);
            foreach (GameObject portrait in PlayerPortraits)
            {
                portrait.SetActive(false);
            }
        }

        void SetActivePlayerPortraits(int SWreplyNumberOfPlayers)
        {
            if (SWreplyNumberOfPlayers == 1)
            {
                PlayerPortraits[0].SetActive(true);
            }

            else if (SWreplyNumberOfPlayers == 2)
            {
                PlayerPortraits[0].SetActive(true);
                PlayerPortraits[1].SetActive(true);
            }

            else if (SWreplyNumberOfPlayers == 3)
            {
                PlayerPortraits[0].SetActive(true);
                PlayerPortraits[1].SetActive(true);
                PlayerPortraits[2].SetActive(true);
            }

            else if (SWreplyNumberOfPlayers == 4)
            {
                PlayerPortraits[0].SetActive(true);
                PlayerPortraits[1].SetActive(true);
                PlayerPortraits[2].SetActive(true);
                PlayerPortraits[3].SetActive(true);
            }

            else if (SWreplyNumberOfPlayers == 0)
            {
                Debug.Log("No players found.");
            }

            else
            {
                Debug.Log("Too many players. Max 4!");

            }
        }

        //****************** Matchmaking ***************************//
        void CheckIn()
        {
            //string customPlayerId = "a1b2c3d4";
            NetworkClient.Instance.CheckIn(nickname, (bool successful, string error) =>   //customPlayerId umisto nickname, takoder napravit id na random nacinu
            {
                if (!successful)
                {
                    Debug.LogError(error);
                }
            });
        }

        void RegisterToTheLobbyServer()
        {
            //PlayerData playerData = new PlayerData();
            //playerData.name = "John";
            //playerData.externalId = "1234567890";

            NetworkClient.Lobby.Register(nickname, (successful, reply, error) => {      //playerData umisto nickname, za to je potreban seriazible klasa playerData, pogledat dokumentaciju
                if (successful)
                {
                    Debug.Log("Lobby registered " + reply);
                    if (string.IsNullOrEmpty(reply.roomId))
                    {
                        JoinOrCreateRoom();
                    }

                    else if (reply.started)
                    {
                        State = LobbyState.JoinedRoom;
                        ConnectToRoom();
                    }

                    else
                    {
                        State = LobbyState.JoinedRoom;
                        ShowJoinedRoomPopover();
                        GetPlayersInTheRoom();
                    }
                }
                else
                {
                    Debug.Log("Lobby failed to register " + reply);
                }
            });
        }

        void JoinOrCreateRoom()
        {
            NetworkClient.Lobby.JoinOrCreateRoom(false, 4, 300, (successful, reply, error) => {     //300-> 0
                if (successful)
                {
                    Debug.Log("Joined or created room " + reply);
                    State = LobbyState.JoinedRoom;
                    ShowJoinedRoomPopover();
                    GetPlayersInTheRoom();
                }
                else
                {
                    Debug.Log("Failed to join or create room " + error);
                }
            });
        }

        //mozda nece tribat
        int GetNumberOfPlayersInTheRoom()
        {
            int i = 1;
            NetworkClient.Lobby.GetPlayersInRoom((succesful, reply, error) =>
            {
                if (succesful)
                {
                    Debug.Log("Got players " + reply);
                    foreach (SWPlayer player in reply.players)
                    {
                        i++;
                    }
                }
                else
                {
                    Debug.Log("Failed to get players " + error);
                    i = -1;
                }
            });
            Debug.Log("Number of players in room: " + i);
            return i;
        }

        void GetPlayersInTheRoom()
        {
            NetworkClient.Lobby.GetPlayersInRoom((successful, reply, error) => {
                if (successful)
                {
                    Debug.Log("Got players " + reply);
                    SetActivePlayerPortraits(reply.players.Count);

                    if (NetworkClient.Lobby.IsOwner && reply.players.Count == 4)     //mozda i provjerit sve igrace   //+1 jer pretpostavljan da pocne od 0
                    {
                        ShowReadyToStartUI();
                    }
                }
                else
                {
                    Debug.Log("Failed to get players " + error);
                }
            });
        }

        void ConnectToRoom()
        {
            //TO DO: connect to the room server
            NetworkClient.Instance.ConnectToRoom((connected) =>
            {
                if (connected)
                {
                    SceneManager.LoadScene("MultiplayerGameScene");
                }
                else
                {
                    Debug.Log("Failed to connect to server.");
                }
            });
        }

        void LeaveRoom()
        {
            NetworkClient.Lobby.LeaveRoom((successful, error) => {
                if (successful)
                {
                    Debug.Log("Left room");
                    State = LobbyState.Default;
                }
                else
                {
                    Debug.Log("Failed to leave room " + error);
                }
            });
        }

        void StartRoom()
        {
            NetworkClient.Lobby.StartRoom((sucessful, error) =>
            {
                if (sucessful)
                {
                    Debug.Log("Started room.");
                }
                else
                {
                    Debug.Log("Failed to start room " + error);
                }
            });
        }

        //****************** Lobby Events **************************//
        void OnLobbyConnected()
        {
            RegisterToTheLobbyServer();
        }

        void OnNewPlayerJoinRoomEvent(SWJoinRoomEventData eventData)
        {
            GetPlayersInTheRoom();
            /*if (NetworkClient.Lobby.IsOwner && GetNumberOfPlayersInTheRoom() + 1 == 1)
            {
                ShowReadyToStartUI();
            }*/
        }

        void OnRoomReadyEvent(SWRoomReadyEventData eventData)
        {
            ConnectToRoom();
        }

        //****************** UI event handlers *********************//
        /// <summary>
        /// Practice button was clicked.
        /// </summary>
        public void OnPracticeClicked()
        {
            Debug.Log("OnPracticeClicked");
            SceneManager.LoadScene("GameScene");
        }

        /// <summary>
        /// Online button was clicked.
        /// </summary>
        public void OnOnlineClicked()
        {
            Debug.Log("OnOnlineClicked");
            ShowEnterNicknamePopover();
        }

        /// <summary>
        /// Cancel button in the popover was clicked.
        /// </summary>
        public void OnCancelClicked()
        {
            Debug.Log("OnCancelClicked");

            if (State == LobbyState.JoinedRoom)
            {
                // TODO: leave room.
                LeaveRoom();
            }

            HideAllPopover();
        }

        /// <summary>
        /// Start button in the WaitForOpponentPopover was clicked.
        /// </summary>
        public void OnStartRoomClicked()
        {
            Debug.Log("OnStartRoomClicked");
            // players are ready to player now.
            if (Debugging)
            {
                SceneManager.LoadScene("GameScene");
            }
            else
            {
                //Start room
                StartRoom();
            }
        }

        /// <summary>
        /// Ok button in the EnterNicknamePopover was clicked.
        /// </summary>
        public void OnConfirmNicknameClicked()
        {
            nickname = NicknameInputField.text;
            Debug.Log($"OnConfirmNicknameClicked: {nickname}");

            if (Debugging)
            {
                ShowJoinedRoomPopover();
                ShowReadyToStartUI();
            }
            else
            {
                //Use nickname as player custom id to check into SocketWeaver.
                CheckIn();
            }
        }
    }
}

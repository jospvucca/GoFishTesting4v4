using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SWNetwork;
using System;
using UnityEngine.Events;

namespace GoFish
{
    [Serializable]
    public class GameDataEvent : UnityEvent<EncryptedData>
    {

    }

    [Serializable]
    public class RankSelectedEvent : UnityEvent<Ranks>
    {

    }

   [Serializable]
    public class TargetPlayerSelectedEvent : UnityEvent<Player> //testing
    {

    }

    public class NetCode : MonoBehaviour
    {
        public MultiplayerGame mpGame;
        public GameDataEvent OnGameDataReadyEvent = new GameDataEvent();
        public GameDataEvent OnGameDataChangedEvent = new GameDataEvent();

        public UnityEvent OnGameStateChangedEvent = new UnityEvent();

        public RankSelectedEvent OnRankSelectedEvent = new RankSelectedEvent();

        public TargetPlayerSelectedEvent OnTargetPlayerSelectedEvent = new TargetPlayerSelectedEvent();     //testing

        public UnityEvent OnOpponenConfirmed = new UnityEvent();

        RoomPropertyAgent roomPropertyAgent;
        RoomRemoteEventAgent roomRemoteEventAgent;

        const string ENCRYPTED_DATA = "EncryptedData";
        const string GAME_STATE_CHANGED = "GameStateChanged";
        const string RANK_SELECTED = "RankSelected";
        const string TARGET_PLAYER_SELECTED = "TargetPlayerSelected";   //testing
        const string OPPONENT_CONFIRMED = "OpponentConfirmed";

        private void Awake()
        {
            roomPropertyAgent = FindObjectOfType<RoomPropertyAgent>();
            roomRemoteEventAgent = FindObjectOfType<RoomRemoteEventAgent>();
        }

        public void ModifyGameData(EncryptedData encryptedData)
        {
            roomPropertyAgent.Modify(ENCRYPTED_DATA, encryptedData);
        }

        public void NotifyOtherPlayersGameStateChanged()
        {
            roomRemoteEventAgent.Invoke(GAME_STATE_CHANGED);
        }

        public void NotifyHostPlayerRankSelected(int selectedRank)
        {
            SWNetworkMessage message = new SWNetworkMessage();
            message.Push(selectedRank);
            roomRemoteEventAgent.Invoke(RANK_SELECTED, message);
        }

        public void NotifyHostPlayerTargetPlayerSelected(string targetPlayer)   //testing
        {
            SWNetworkMessage message = new SWNetworkMessage();
            message.PushUTF8ShortString(targetPlayer);
            roomRemoteEventAgent.Invoke(TARGET_PLAYER_SELECTED, message); ;
        }

        public void NotifyHostPlayerOpponentConfirmed()
        {
            roomRemoteEventAgent.Invoke(OPPONENT_CONFIRMED);
        }

        public void EnableRoomPropertyAgent()
        {
            roomPropertyAgent.Initialize();
        }

        //*********************** Room Property Events *********************//
        public void OnEncryptedDataReady()
        {
            EncryptedData encryptedData = roomPropertyAgent.GetPropertyWithName(ENCRYPTED_DATA).GetValue<EncryptedData>();
            OnGameDataReadyEvent.Invoke(encryptedData);
        }

        public void OnEncryptedDataChanged()
        {
            Debug.Log("NetCode :: OnEncryptedDataChanged().");
            EncryptedData encryptedData = roomPropertyAgent.GetPropertyWithName(ENCRYPTED_DATA).GetValue<EncryptedData>();
            OnGameDataChangedEvent.Invoke(encryptedData);
        }

        //*********************** Room Remote Events ***********************//
        public void OnGameStateChangedRemoteEvent()
        {
            OnGameStateChangedEvent.Invoke();
        }

        public void OnRankSelectedRemoteEvent(SWNetworkMessage message)
        {
            int intRank = message.PopInt32();
            OnRankSelectedEvent.Invoke((Ranks)intRank);
        }

        public void OnTargetPlayerSelectedRemoteEvent(SWNetworkMessage message) //testing
        {
            string strTargetPlayer = message.PopUTF8ShortString();
            List<Player> allPlayers = mpGame.gameDataManager.GetAllPlayers();
            foreach (Player player in allPlayers)
            {
                if (player.PlayerId.Equals(strTargetPlayer))
                {
                    OnTargetPlayerSelectedEvent.Invoke(player);
                }
            }

        }

        public void OnOpponentConfirmedRemoteEvent()
        {
            OnOpponenConfirmed.Invoke();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GoFish
{
    [Serializable]
    public class EncryptedData
    {
        public byte[] data;
    }

    [Serializable]
    public class GameDataManager
    {
        Player localPlayer;
        Player[] remotePlayer;

        [SerializeField]
        ProtectedData protectedData;

        public GameDataManager(Player local, Player remote, Player remote2, Player remote3, string roomId = "1234567890123456")
        {
            localPlayer = local;

            remotePlayer = new Player[3];

            remotePlayer[0] = remote;
            remotePlayer[1] = remote2;
            remotePlayer[2] = remote3;

            //SetCurrentTurnPlayer(null);
            //Debug.Log("On Awake in GameDataManager :: SetCurrentTurnPlayer(null).");
            //SetCurrentTurnTargetPlayer(null);
            //Debug.Log("On Awake in GameDataManager :: SetCurrentTurnTargerPlayer(null).");

            protectedData = new ProtectedData(localPlayer.PlayerId, remotePlayer[0].PlayerId, remotePlayer[1].PlayerId, remotePlayer[2].PlayerId, roomId);
        }

        public void Shuffle()
        {
            List<byte> cardValues = new List<byte>();

            for (byte value = 0; value < 52; value++)
            {
                cardValues.Add(value);
            }

            List<byte> poolOfCards = new List<byte>();

            for (int index = 0; index < 52; index++)
            {
                int valueIndexToAdd = UnityEngine.Random.Range(0, cardValues.Count);

                byte valueToAdd = cardValues[valueIndexToAdd];
                poolOfCards.Add(valueToAdd);
                cardValues.Remove(valueToAdd);
            }

            protectedData.SetPoolOfCards(poolOfCards);
        }

        public void DealCardValuesToPlayer(Player player, int numberOfCards)
        {
            List<byte> poolOfCards = protectedData.GetPoolOfCards();

            int numberOfCardsInThePool = poolOfCards.Count;
            int start = numberOfCardsInThePool - 1 - numberOfCards;

            List<byte> cardValues = poolOfCards.GetRange(start, numberOfCards);
            poolOfCards.RemoveRange(start, numberOfCards);

            protectedData.AddCardValuesToPlayer(player, cardValues);
            //protectedData.SetPoolOfCards(poolOfCards);                  //zbog enkripcije, provjerit i ostale metode ako je nesto sjebano
        }

        public byte DrawCardValue()
        {
            List<byte> poolOfCards = protectedData.GetPoolOfCards();

            int numberOfCardsInThePool = poolOfCards.Count;

            if (numberOfCardsInThePool > 0)
            {
                byte cardValue = poolOfCards[numberOfCardsInThePool - 1];
                poolOfCards.Remove(cardValue);
                protectedData.SetPoolOfCards(poolOfCards);    
                return cardValue;
            }

            return Constants.POOL_IS_EMPTY;
        }

        public List<byte> PlayerCards(Player player)
        {
            return protectedData.PlayerCards(player);
        }

        public void AddCardValuesToPlayer(Player player, List<byte> cardValues)
        {
            protectedData.AddCardValuesToPlayer(player, cardValues);
        }

        public void AddCardValueToPlayer(Player player, byte cardValue)
        {
            protectedData.AddCardValueToPlayer(player, cardValue);
        }

        public void RemoveCardValuesFromPlayer(Player player, List<byte> cardValuesToRemove)
        {
            protectedData.RemoveCardValuesFromPlayer(player, cardValuesToRemove);
        }

        public void AddBooksForPlayer(Player player, int numberOfNewBooks)
        {
            protectedData.AddBooksForPlayer(player, numberOfNewBooks);
        }

        public Player Winner()
        {
            string winnerPlayerId = protectedData.WinnerPlayerId();
            if (winnerPlayerId.Equals(localPlayer.PlayerId))
            {
                return localPlayer;
            }

            else if (winnerPlayerId.Equals(remotePlayer[0].PlayerId)) 
            {
                return remotePlayer[0];
            }

            else if (winnerPlayerId.Equals(remotePlayer[1].PlayerId))
            {
                return remotePlayer[1];
            }

            else if (winnerPlayerId.Equals(remotePlayer[2].PlayerId))
            {
                return remotePlayer[2];
            }

            else
            {
                Debug.LogWarning("Logic error in GameDataManager :: Winner()!");
                return null;
            }
        }

        public bool GameFinished()
        {
            return protectedData.GameFinished();
        }

        public List<byte> TakeCardValuesWithRankFromPlayer(Player player, Ranks ranks)
        {
            List<byte> playerCards = protectedData.PlayerCards(player);

            List<byte> result = new List<byte>();

            foreach (byte cv in playerCards)
            {
                if (Card.GetRank(cv) == ranks)
                {
                    result.Add(cv);
                }
            }

            protectedData.RemoveCardValuesFromPlayer(player, result);

            return result;
        }

        public Dictionary<Ranks, List<byte>> GetBooks(Player player)
        {
            List<byte> playerCards = protectedData.PlayerCards(player);

            var groups = playerCards.GroupBy(Card.GetRank).Where(g => g.Count() == 4);

            if (groups.Count() > 0)
            {
                Dictionary<Ranks, List<byte>> setOfFourDictionary = new Dictionary<Ranks, List<byte>>();

                foreach (var group in groups)
                {
                    List<byte> cardValues = new List<byte>();

                    foreach (var value in group)
                    {
                        cardValues.Add(value);
                    }

                    setOfFourDictionary[group.Key] = cardValues;
                }

                return setOfFourDictionary;
            }

            return null;
        }

        public Ranks SelectRandomRanksFromPlayersCardValues(Player player)
        {
            List<byte> playerCards = protectedData.PlayerCards(player);
            int index = UnityEngine.Random.Range(0, playerCards.Count);

            return Card.GetRank(playerCards[index]);
        }

        public void SetGameState(Game.GameState gameState)
        {
            protectedData.SetGameState((int)gameState);
        }

        public Game.GameState GetGameState()
        {
            return (Game.GameState)protectedData.GetGameState();
        }

        public void SetCurrentTurnPlayer(Player player)
        {
            if (player == null)
            {
                protectedData.SetCurrentTurnPlayer("null");
            }
            else
            {
                protectedData.SetCurrentTurnPlayer(player.PlayerId);        //dodana provjera za null, tribalo bi i za ostale
            }
        }


        public Player GetCurrentTurnPlayer()
        {
            string playerId = protectedData.GetCurrentTurnPlayer();
            if (localPlayer.PlayerId.Equals(playerId))
            {
                return localPlayer;
            }
            else
            {
                for (int i = 0; i < remotePlayer.Length; i++)   //mozda +/- 1 kod lengtha?
                {
                    if (remotePlayer[i].PlayerId.Equals(playerId))
                    {
                        return remotePlayer[i];
                    }
                }
                Debug.Log("Logic error in GameDataManager :: GetCurrentTurnPlayer(). Returns null, probably should not.");  //ako ovako ne bude radilo samo rasclanit na else ifove za svaki remote
                return null;
            }
        }

        public void SetCurrentTurnTargetPlayer(Player player)
        {
            protectedData.SetCurrentTurnTargetPlayer(player.PlayerId);
        }

        public Player GetCurrentTurnTargetPlayer()
        {
            string playerId = protectedData.GetCurrentTurnTargetPlayer();
            if (localPlayer.PlayerId.Equals(playerId))
            {
                return localPlayer;
            }
            else
            {
                for (int i = 0; i < remotePlayer.Length; i++)   //mozda +/- 1 kod lengtha?
                {
                    if (remotePlayer[i].PlayerId.Equals(playerId))
                    {
                        return remotePlayer[i];
                    }
                }
                Debug.Log("Logic error in GameDataManager :: GetCurrentTurnPlayer(). Returns null, probably should not.");  //ako ovako ne bude radilo samo rasclanit na else ifove za svaki remote
                return null;
            }
        }

        public Player GetNextTurnPlayer()
        {
            //this function is not expandable and is hardcoded, needs redoing 
            string playerId = protectedData.GetCurrentTurnPlayer();
            if (localPlayer.PlayerId.Equals(playerId))
            {
                return remotePlayer[0];
            }

            else if (remotePlayer[0].PlayerId.Equals(playerId))
            {
                return remotePlayer[1];
            }

            else if (remotePlayer[1].PlayerId.Equals(playerId))
            {
                return remotePlayer[2];
            }

            else if (remotePlayer[2].PlayerId.Equals(playerId))
            {
                return localPlayer;
            }

            //else if (remotePlayer[3].PlayerId.Equals(playerId))
            //{
            //    return localPlayer;
            //}

            else
            {
                Debug.LogError("Might be Logic error in GameDataManager :: GetNextTurnPlayer()!\nUnless its first turn, then host local player should start first.");
                return localPlayer;
            }
        }

        public void SetSelectedRank(Ranks rank)
        {
            protectedData.SetSelectedRank((int)rank);
        }

        public Ranks GetSelectedRank()
        {
            return (Ranks)protectedData.GetSelectedRank();
        }

        public List<Player> GetAllPlayers()
        {
            string[] allPlayerIds = protectedData.GetAllPlayers();
            List<Player> allPlayers = new List<Player>();
            for (int i = 0; i < allPlayerIds.Length; i++)       //mozda +/- 1 nisan sig
            {
                if (localPlayer.PlayerId.Equals(allPlayerIds[i]))   //djelomicno hardkodirano al nije mi se dalo mislit
                {
                    allPlayers.Add(localPlayer);
                }

                else if (remotePlayer[0].PlayerId.Equals(allPlayerIds[i]))
                {
                    allPlayers.Add(remotePlayer[0]);
                }

                else if (remotePlayer[1].PlayerId.Equals(allPlayerIds[i]))
                {
                    allPlayers.Add(remotePlayer[1]);
                }

                else if (remotePlayer[2].PlayerId.Equals(allPlayerIds[i]))
                {
                    allPlayers.Add(remotePlayer[2]);
                }

                //else if (remotePlayer[3].PlayerId.Equals(allPlayerIds[i]))
                //{
                //    allPlayers.Add(remotePlayer[3]);
                //}

                else
                {
                    Debug.LogError("Crucial Net error in GameDataManager :: List<Player> GetAllPlayers!");
                    return null;
                }
            }

            return allPlayers;
        }

        public EncryptedData EncryptedData()
        {
            Byte[] data = protectedData.ToArray();

            EncryptedData encryptedData = new GoFish.EncryptedData();
            encryptedData.data = data;

            return encryptedData;
        }

        public void ApplyEncryptedData(EncryptedData encryptedData)
        {
            if (encryptedData == null)
            {
                return;
            }

            protectedData.ApplyByteArray(encryptedData.data);
        }
    }
}

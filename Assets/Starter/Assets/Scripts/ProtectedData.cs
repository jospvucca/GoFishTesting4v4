using System;
using System.Collections.Generic;
using UnityEngine;
using SWNetwork;
using System.Linq;
using System.Text;

namespace GoFish
{
    /// <summary>
    /// Stores the important data of the game
    /// We will encypt the fields in a multiplayer game.
    /// </summary>
    [Serializable]
    public class ProtectedData
    {
        [SerializeField]
        List<byte> poolOfCards = new List<byte>();
        [SerializeField]
        List<byte> player1Cards = new List<byte>();
        [SerializeField]
        List<byte> player2Cards = new List<byte>();
        [SerializeField]
        List<byte> player3Cards = new List<byte>();
        [SerializeField]
        List<byte> player4Cards = new List<byte>();

        [SerializeField]
        int numberOfBooksForPlayer1;
        [SerializeField]
        int numberOfBooksForPlayer2;
        [SerializeField]
        int numberOfBooksForPlayer3;
        [SerializeField]
        int numberOfBooksForPlayer4;

        [SerializeField]
        string player1Id;
        [SerializeField]
        string player2Id;
        [SerializeField]
        string player3Id;
        [SerializeField]
        string player4Id;

        [SerializeField]
        string currentTurnPlayerId;
        [SerializeField]
        int currentGameState;   //dodat metode koje fale i nisu u tutorialu: C:\Users\Josip\Desktop\SKULA\UnityServerOptions\GoFish-master\Final\Assets\Scripts     //vjv triba pogledat i protectedData i GameDataManager
        [SerializeField]
        int selectedRank;
        [SerializeField]
        string currentTurnTargetPlayerId;

        byte[] encryptionKey;
        byte[] safeData;

        public ProtectedData(string p1Id, string p2Id, string p3Id, string p4Id, string roomId)
        {
            player1Id = p1Id;
            player2Id = p2Id;
            player3Id = p3Id;
            player4Id = p4Id;
            currentTurnPlayerId = "null";
            currentTurnTargetPlayerId = "null";

            //SetCurrentTurnPlayer(currentTurnPlayerId);
            //SetCurrentTurnTargetPlayer(currentTurnTargetPlayerId);

            selectedRank = (int)Ranks.NoRanks;
            //CalculateKey(roomId);
            Debug.Log("ProtectedData stored players: \n" + player1Id + "\n" + player2Id + "\n" + player3Id + "\n" + player4Id);
            Encrypt();
        }

        public void SetPoolOfCards(List<byte> cardValues)
        {
            Decrypt();
            poolOfCards = cardValues;
            Encrypt();
        }

        public List<byte> GetPoolOfCards()
        {
            List<byte> result;
            Decrypt();
            result = poolOfCards;
            Encrypt();
            return result;
        }

        public List<byte> PlayerCards(Player player)
        {
            List<byte> result;
            Decrypt();
            if (player.PlayerId.Equals(player1Id))
            {
                result = player1Cards;
                Encrypt();
                return result;
            }

            else if (player.PlayerId.Equals(player2Id)) 
            {
                result = player2Cards;
                Encrypt();
                return result;
            }

            else if (player.PlayerId.Equals(player3Id))
            {
                result = player3Cards;
                Encrypt();
                return result;
            }

            else if (player.PlayerId.Equals(player4Id))
            {
                result = player4Cards;
                Encrypt();
                return result;
            }

            else
            {
                Debug.LogWarning("Logic error in ProtectedData :: PlayerCards(Player)!");
                Encrypt();
                return null;
            }
        }

        public void AddCardValuesToPlayer(Player player, List<byte> cardValues)
        {
            Decrypt();
            if (player.PlayerId.Equals(player1Id))
            {
                player1Cards.AddRange(cardValues);
                player1Cards.Sort();
            }

            else if (player.PlayerId.Equals(player2Id)) 
            {
                player2Cards.AddRange(cardValues);
                player2Cards.Sort();
            }

            else if (player.PlayerId.Equals(player3Id))
            {
                player3Cards.AddRange(cardValues);
                player3Cards.Sort();
            }

            else if (player.PlayerId.Equals(player4Id))
            {
                player4Cards.AddRange(cardValues);
                player4Cards.Sort();
            }

            else
            {
                Debug.LogWarning("Logic error in ProtectedData :: AddCardValuesToPlayer(Player, List<byte>)!");
            }
            Encrypt();
        }

        public void AddCardValueToPlayer(Player player, byte cardValue)
        {
            Decrypt();
            if (player.PlayerId.Equals(player1Id))
            {
                player1Cards.Add(cardValue);
                player1Cards.Sort();
            }

            else if (player.PlayerId.Equals(player2Id))
            {
                player2Cards.Add(cardValue);
                player2Cards.Sort();
            }

            else if (player.PlayerId.Equals(player3Id))
            {
                player3Cards.Add(cardValue);
                player3Cards.Sort();
            }

            else if (player.PlayerId.Equals(player4Id))
            {
                player4Cards.Add(cardValue);
                player4Cards.Sort();
            }

            else
            {
                Debug.LogWarning("Logic error in ProtectedData :: AddCardValueToPlayer(Player, byte)!");
            }
            Encrypt();
        }

        public void RemoveCardValuesFromPlayer(Player player, List<byte> cardValuesToRemove)
        {   //mislin da cu ovde morat razvit malo bolju logiku, ostavit cu u komentaru staru verziju za svaki slucaj
            /*if (player.PlayerId.Equals(player1Id))
            {
                player1Cards.RemoveAll(cv => cardValuesToRemove.Contains(cv));
            }
            else
            {
                player2Cards.RemoveAll(cv => cardValuesToRemove.Contains(cv));
            }*/
            Decrypt();
            if (player.PlayerId.Equals(player1Id))
            {
                player1Cards.RemoveAll(cv => cardValuesToRemove.Contains(cv));
            }

            else if (player.PlayerId.Equals(player2Id))
            {
                player2Cards.RemoveAll(cv => cardValuesToRemove.Contains(cv));
            }

            else if (player.PlayerId.Equals(player3Id))
            {
                player3Cards.RemoveAll(cv => cardValuesToRemove.Contains(cv));
            }

            else if (player.PlayerId.Equals(player4Id))
            {
                player4Cards.RemoveAll(cv => cardValuesToRemove.Contains(cv));
            }

            else
            {
                Debug.LogWarning("Logic error in ProtectedData :: RemoveCardValuesFromPlayer(Player,List<byte>)!");
            }
            Encrypt();
        }

        public void AddBooksForPlayer(Player player, int numberOfNewBooks)
        {
            Decrypt();
            if (player.PlayerId.Equals(player1Id))
            {
                numberOfBooksForPlayer1 += numberOfNewBooks;
            }

            else if (player.PlayerId.Equals(player2Id)) 
            {
                numberOfBooksForPlayer2 += numberOfNewBooks;
            }

            else if (player.PlayerId.Equals(player3Id))
            {
                numberOfBooksForPlayer3 += numberOfNewBooks;
            }

            else if (player.PlayerId.Equals(player4Id))
            {
                numberOfBooksForPlayer4 += numberOfNewBooks;
            }

            else
            {
                Debug.LogWarning("Logic error in ProtectedData :: AddBooksForPlayer(Player,int)!");
            }

            Encrypt();
        }

        public bool GameFinished()
        {
            bool result;
            Decrypt();
            if (poolOfCards.Count == 0)
            {
                result = true;
                Encrypt();
                return result;
            }

            else if (player1Cards.Count == 0)
            {
                result = true;
                Encrypt();
                return result;
            }

            else if (player2Cards.Count == 0)
            {
                result = true;
                Encrypt();
                return result;
            }

            else if (player3Cards.Count == 0)
            {
                result = true;
                Encrypt();
                return result;
            }

            else if (player4Cards.Count == 0)
            {
                result = true;
                Encrypt();
                return result;
            }

            else
            {
                Debug.Log("Might be error in logic, might be game ended.\n ProtectedData :: GameFinished()!");
                result = false;
                Encrypt();
                return result;
            }

            //return false;
        }

        public string WinnerPlayerId()
        {
            string result;
            Decrypt();
            if (numberOfBooksForPlayer1 > numberOfBooksForPlayer2 && numberOfBooksForPlayer1 > numberOfBooksForPlayer3 && numberOfBooksForPlayer1 > numberOfBooksForPlayer4)
            {
                result = player1Id;
                Encrypt();
                return result;
            }

            else if (numberOfBooksForPlayer2 > numberOfBooksForPlayer1 && numberOfBooksForPlayer2 > numberOfBooksForPlayer3 && numberOfBooksForPlayer2 > numberOfBooksForPlayer4)
            {
                result = player2Id;
                Encrypt();
                return result;
            }

            else if (numberOfBooksForPlayer3 > numberOfBooksForPlayer1 && numberOfBooksForPlayer3 > numberOfBooksForPlayer2 && numberOfBooksForPlayer3 > numberOfBooksForPlayer4)
            {
                result = player3Id;
                Encrypt();
                return result;
            }

            else if (numberOfBooksForPlayer4 > numberOfBooksForPlayer1 && numberOfBooksForPlayer4 > numberOfBooksForPlayer2 && numberOfBooksForPlayer4 > numberOfBooksForPlayer3)
            {
                result = player4Id;
                Encrypt();
                return result;
            }

            else
            {
                Debug.LogWarning("Logic error in ProtectedData :: WinnerPlayerId()! Probably a tie.");
                Encrypt();
                return "null";
            }
        }

        public void SetGameState(int gameState)
        {
            Decrypt();
            currentGameState = gameState;
            Encrypt();
        }

        public int GetGameState()
        {
            int result;
            Decrypt();
            result = currentGameState;
            Encrypt();
            return result;
        }

        public void SetCurrentTurnPlayer(string playerId)
        {
            Decrypt();
            currentTurnPlayerId = playerId;
            Encrypt();
        }

        public string GetCurrentTurnPlayer()
        {
            string result;
            Decrypt();
            result = currentTurnPlayerId;
            Encrypt();
            return result;
        }

        public void SetSelectedRank(int rank)
        {
            Decrypt();
            selectedRank = rank;
            Encrypt();
        }

        public int GetSelectedRank()
        {
            int result;
            Decrypt();
            result = selectedRank;
            Encrypt();
            return result;
        }

        public string GetCurrentTurnTargetPlayer()
        {
            string result;
            Decrypt();
            result = currentTurnTargetPlayerId;
            Encrypt();
            return result;
        }

        public void SetCurrentTurnTargetPlayer(string playerId)
        {
            Decrypt();
            currentTurnTargetPlayerId = playerId;
            Encrypt();
        }

        public string[] GetAllPlayers()
        {
            string[] allPlayerIds = new string[4];  //amo rec hardcodirano, vjv mogu stavit i = getnumberofplayers itd, al nema smisla imo
            allPlayerIds[0] = player1Id;
            allPlayerIds[1] = player2Id;
            allPlayerIds[2] = player3Id;
            allPlayerIds[3] = player4Id;

            return allPlayerIds;

        }
        ////////dodat ovde

        public Byte[] ToArray()
        {
            /*SWNetworkMessage message = new SWNetworkMessage();

            message.Push((Byte)poolOfCards.Count);
            message.PushByteArray(poolOfCards.ToArray());   //nije rekurzija koliko san svatija nego inbuilt funkcija

            message.Push((Byte)player1Cards.Count);
            message.PushByteArray(player1Cards.ToArray());

            message.Push((Byte)player2Cards.Count);
            message.PushByteArray(player2Cards.ToArray());

            message.Push((Byte)player3Cards.Count);
            message.PushByteArray(player3Cards.ToArray());

            message.Push((Byte)player4Cards.Count);
            message.PushByteArray(player4Cards.ToArray());

            message.Push(numberOfBooksForPlayer1);
            message.Push(numberOfBooksForPlayer2);
            message.Push(numberOfBooksForPlayer3);
            message.Push(numberOfBooksForPlayer4);

            message.PushUTF8ShortString(player1Id);
            message.PushUTF8ShortString(player2Id);
            message.PushUTF8ShortString(player3Id);
            message.PushUTF8ShortString(player4Id);

            message.PushUTF8ShortString(currentTurnPlayerId);
            message.Push(currentGameState);

            message.Push(selectedRank);
            message.PushUTF8ShortString(currentTurnTargetPlayerId);

            Debug.Log(message); //za testiranje
            return message.ToArray();*/

            return safeData;
        }

        public void ApplyByteArray(Byte[] byteArray)
        {
            /*SWNetworkMessage message = new SWNetworkMessage(byteArray);

            byte poolOfCardsCount = message.PopByte();
            poolOfCards = message.PopByteArray(poolOfCardsCount).ToList();

            byte poolOfPlayer1CardsCount = message.PopByte();
            player1Cards = message.PopByteArray(poolOfPlayer1CardsCount).ToList();

            byte poolOfPlayer2CardsCount = message.PopByte();
            player2Cards = message.PopByteArray(poolOfPlayer2CardsCount).ToList();

            byte poolOfPlayer3CardsCount = message.PopByte();
            player3Cards = message.PopByteArray(poolOfPlayer3CardsCount).ToList();

            byte poolOfPlayer4CardsCount = message.PopByte();
            player4Cards = message.PopByteArray(poolOfPlayer4CardsCount).ToList();

            numberOfBooksForPlayer1 = message.PopInt32();
            numberOfBooksForPlayer2 = message.PopInt32();
            numberOfBooksForPlayer3 = message.PopInt32();
            numberOfBooksForPlayer4 = message.PopInt32();

            player1Id = message.PopUTF8ShortString();
            player2Id = message.PopUTF8ShortString();
            player3Id = message.PopUTF8ShortString();
            player4Id = message.PopUTF8ShortString();

            currentTurnPlayerId = message.PopUTF8ShortString();
            currentGameState = message.PopInt32();

            selectedRank = message.PopInt32();
            currentTurnTargetPlayerId = message.PopUTF8ShortString();*/

            safeData = byteArray;
        }

        void CalculateKey(string roomId)
        {
            string RoomIdSubstring = roomId.Substring(0, 16);
            encryptionKey = Encoding.UTF8.GetBytes(RoomIdSubstring);
        }

        void Encrypt()
        {
            //SWNetworkMessage message = new SWNetworkMessage();

            //message.Push((Byte)poolOfCards.Count);
            //message.PushByteArray(poolOfCards.ToArray());

            //message.Push((Byte)player1Cards.Count);
            //message.PushByteArray(player1Cards.ToArray());

            //message.Push((Byte)player2Cards.Count);
            //message.PushByteArray(player2Cards.ToArray());

            //message.Push((Byte)player3Cards.Count);
            //message.PushByteArray(player3Cards.ToArray());

            //message.Push((Byte)player4Cards.Count);
            //message.PushByteArray(player4Cards.ToArray());

            //message.Push(numberOfBooksForPlayer1);
            //message.Push(numberOfBooksForPlayer2);
            //message.Push(numberOfBooksForPlayer3);
            //message.Push(numberOfBooksForPlayer4);

            //message.PushUTF8ShortString(player1Id);
            //message.PushUTF8ShortString(player2Id);
            //message.PushUTF8ShortString(player3Id);
            //message.PushUTF8ShortString(player4Id);

            //message.PushUTF8ShortString(currentTurnPlayerId);
            //message.Push(currentGameState);

            //message.Push(selectedRank);
            //message.PushUTF8ShortString(currentTurnTargetPlayerId);

            ////Debug.Log(message); //za testiranje
            //if (message == null)
            //    return; //testinranje

            ////
            //safeData = AES.EncryptAES128(message.ToArray(), encryptionKey);
            //poolOfCards = new List<byte>();
            //player1Cards = new List<byte>();
            //player2Cards = new List<byte>();
            //player3Cards = new List<byte>();
            //player4Cards = new List<byte>();

            //numberOfBooksForPlayer1 = 0;
            //numberOfBooksForPlayer2 = 0;
            //numberOfBooksForPlayer3 = 0;
            //numberOfBooksForPlayer4 = 0;

            //player1Id = null;
            //player2Id = null;
            //player3Id = null;
            //player4Id = null;

            //currentTurnPlayerId = null;
            //currentGameState = 0;
            //selectedRank = 0;
            //currentTurnTargetPlayerId = null;

        }

        void Decrypt()
        {
            //if (safeData == null)   //testiranje
            //    return;

            //byte[] byteArray = AES.DecryptAES128(safeData, encryptionKey);

            //SWNetworkMessage message = new SWNetworkMessage(byteArray);
            //byte poolOfCardsCount = message.PopByte();
            //poolOfCards = message.PopByteArray(poolOfCardsCount).ToList();

            //byte poolOfPlayer1CardsCount = message.PopByte();
            //player1Cards = message.PopByteArray(poolOfPlayer1CardsCount).ToList();

            //byte poolOfPlayer2CardsCount = message.PopByte();
            //player2Cards = message.PopByteArray(poolOfPlayer2CardsCount).ToList();

            //byte poolOfPlayer3CardsCount = message.PopByte();
            //player3Cards = message.PopByteArray(poolOfPlayer3CardsCount).ToList();

            //byte poolOfPlayer4CardsCount = message.PopByte();
            //player4Cards = message.PopByteArray(poolOfPlayer4CardsCount).ToList();

            //numberOfBooksForPlayer1 = message.PopInt32();
            //numberOfBooksForPlayer2 = message.PopInt32();
            //numberOfBooksForPlayer3 = message.PopInt32();
            //numberOfBooksForPlayer4 = message.PopInt32();

            //player1Id = message.PopUTF8ShortString();
            //player2Id = message.PopUTF8ShortString();
            //player3Id = message.PopUTF8ShortString();
            //player4Id = message.PopUTF8ShortString();

            //currentTurnPlayerId = message.PopUTF8ShortString();
            //currentGameState = message.PopInt32();

            //selectedRank = message.PopInt32();
            //currentTurnTargetPlayerId = message.PopUTF8ShortString();
        }
    }
}
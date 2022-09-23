using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SWNetwork;

namespace GoFish
{
    public class MultiplayerGame : Game
    {
        NetCode netCode;
        protected new void Awake()
        {
            base.Awake();

            netCode = FindObjectOfType<NetCode>();

            NetworkClient.Lobby.GetPlayersInRoom((sucessful, reply, error) =>
            {
                if (sucessful)
                {
                    int i = 0;
                    if (reply.players != null)
                    {
                        foreach (SWPlayer player in reply.players)
                        {
                            string playerName = player.GetCustomDataString();
                            string playerId = player.id;

                            if (playerId.Equals(NetworkClient.Instance.PlayerId))
                            {
                                localPlayer.PlayerName = playerName;
                                localPlayer.PlayerId = playerId;
                                localPlayer.IsAI = false;
                            }
                            else
                            {
                                remotePlayer[i].PlayerName = playerName;
                                remotePlayer[i].PlayerId = playerId;
                                remotePlayer[i].IsAI = false;
                                i++;
                            }
                        }
                        ChoseThePlayerCardButtonPanel.GetComponent<ButtonPlayerConfiguration>().AsignPlayersToButtons(remotePlayer[0].PlayerName, remotePlayer[1].PlayerName, remotePlayer[2].PlayerName);
                    }
                    else
                    {
                        //vjv javit error
                    }

                    gameDataManager = new GameDataManager(localPlayer, remotePlayer[0], remotePlayer[1], remotePlayer[2], NetworkClient.Lobby.RoomId);

                    netCode.EnableRoomPropertyAgent();
                }
                else
                {
                    Debug.Log("Failed to get players in the room.");
                }
            });
        }

        protected new void Start()
        {
            Debug.Log("Multiplayer Game Start.");

        }

        protected override void OnGameStarted()
        {
            if (NetworkClient.Instance.IsHost)
            {
                gameDataManager.Shuffle();
                gameDataManager.DealCardValuesToPlayer(localPlayer, Constants.PLAYER_INITIAL_CARDS);
                gameDataManager.DealCardValuesToPlayer(remotePlayer[0], Constants.PLAYER_INITIAL_CARDS);
                gameDataManager.DealCardValuesToPlayer(remotePlayer[1], Constants.PLAYER_INITIAL_CARDS);
                gameDataManager.DealCardValuesToPlayer(remotePlayer[2], Constants.PLAYER_INITIAL_CARDS);

                gameState = GameState.TurnStarted;

                gameDataManager.SetGameState(gameState);

                //testing 1.86
                gameDataManager.SetCurrentTurnPlayer(null);
                gameDataManager.SetCurrentTurnTargetPlayer(null);
                SwitchTurn();

                netCode.ModifyGameData(gameDataManager.EncryptedData());
            }

            cardAnimator.DealDisplayingCards(localPlayer, Constants.PLAYER_INITIAL_CARDS);
            cardAnimator.DealDisplayingCards(remotePlayer[0], Constants.PLAYER_INITIAL_CARDS);
            cardAnimator.DealDisplayingCards(remotePlayer[1], Constants.PLAYER_INITIAL_CARDS);
            cardAnimator.DealDisplayingCards(remotePlayer[2], Constants.PLAYER_INITIAL_CARDS); 
        }

        protected override void OnTurnStarted()
        {
            if (NetworkClient.Instance.IsHost)                              //nekuzin zasto san izbrisa GameFlow() poziv iza svega
            {
                SwitchTurn();
                gameState = GameState.TurnSelectingNumber;

                gameDataManager.SetCurrentTurnPlayer(currentTurnPlayer);
                gameDataManager.SetGameState(gameState);

                netCode.ModifyGameData(gameDataManager.EncryptedData());
                netCode.NotifyOtherPlayersGameStateChanged();
            }
        }

        protected override void OnTurnConfirmedSelectedNumber()
        {
            if (currentTurnPlayer == localPlayer)
            {
                SetMessage($"Asking {currentTurnTargetPlayer.PlayerName} for {selectedRank}s...");
            }
            else
            {
                SetMessage($"{currentTurnPlayer.PlayerName} is asking for {selectedRank}s...");
            }

            if (NetworkClient.Instance.IsHost)
            {
                gameState = GameState.TurnWaitingForOpponentConfirmation;
                gameDataManager.SetGameState(gameState);

                netCode.ModifyGameData(gameDataManager.EncryptedData());
                netCode.NotifyOtherPlayersGameStateChanged();
            }

            //gameState = GameState.TurnWaitingForOpponentConfirmation;   //ovo 2 vjv ne bi tribalo bit ovde, moguce da uzrokuje bug
            //base.GameFlow();
        }

        protected override void OnTurnOpponentConfirmed()       //nista izmjenjeno?
        {
            List<byte> cardValuesFromTargetPlayer = gameDataManager.TakeCardValuesWithRankFromPlayer(currentTurnTargetPlayer, selectedRank);

            if (cardValuesFromTargetPlayer.Count > 0)
            {
                gameDataManager.AddCardValuesToPlayer(currentTurnPlayer, cardValuesFromTargetPlayer);

                bool senderIsLocalPlayer = currentTurnTargetPlayer == localPlayer;
                currentTurnTargetPlayer.SendDisplayingCardToPlayer(currentTurnPlayer, cardAnimator, cardValuesFromTargetPlayer, senderIsLocalPlayer);   //ovo takoder moze stvarat problem, sta ako sender nije local
                ResetSelectedPlayer();  //testing           //mozda ovo sta je komentirano bude stvaralo probleme u mp(vjv ne, al za svaki slucaj napomena,zadnje editat)
                ResetSelectedCard();    //testing
                if (NetworkClient.Instance.IsHost)
                {
                    gameState = GameState.TurnSelectingNumber;

                    gameDataManager.SetGameState(gameState);
                    netCode.ModifyGameData(gameDataManager.EncryptedData());
                }
            }
            else
            {
                ResetSelectedPlayer();  //testing
                ResetSelectedCard();    //testing
                if (NetworkClient.Instance.IsHost)
                {
                    gameState = GameState.TurnGoFish;

                    gameDataManager.SetGameState(gameState);
                    netCode.ModifyGameData(gameDataManager.EncryptedData());
                    netCode.NotifyOtherPlayersGameStateChanged();
                }
                //GameFlow();   //isto nekuzin zasto se ovo ne poziva, mozda bude stvaralo probleme kasnije?
            }
        }

        protected override void OnTurnGoFish()
        {
            SetMessage($"Go fish!");

            byte cardValue = gameDataManager.DrawCardValue();

            if (cardValue == Constants.POOL_IS_EMPTY)
            {
                Debug.LogError("Pool is empty");
                return;
            }

            if (Card.GetRank(cardValue) == selectedRank)
            {
                cardAnimator.DrawDisplayingCard(currentTurnPlayer, cardValue);
            }
            else
            {
                cardAnimator.DrawDisplayingCard(currentTurnPlayer);
                if (NetworkClient.Instance.IsHost)
                {
                    gameState = GameState.TurnStarted;
                }
            }

            gameDataManager.AddCardValueToPlayer(currentTurnPlayer, cardValue);

            if (NetworkClient.Instance.IsHost)
            {
                gameDataManager.SetGameState(gameState);
                netCode.ModifyGameData(gameDataManager.EncryptedData());
            }
        }

        public override void AllAnimationsFinished()
        {
            if (NetworkClient.Instance.IsHost)
            {
                netCode.NotifyOtherPlayersGameStateChanged();
            }
        }

        //*********************** User Interaction *************************//
        //ovde ce se mozda morat overrideat onbuttonselected isto

        public override void OnOkSelected()
        {
            //NISAN SIGURAN KAKO BI OVO RJESIJA PA CU KOPIRAT SA TUTORIALA I KASNIJE PRIKO TESTIRANJA USKLADIT

            if (gameState == GameState.TurnSelectingPlayer && localPlayer == currentTurnPlayer) 
            {
                if (selectedCard != null && currentTurnTargetPlayer != null)
                {
                    gameState = GameState.TurnConfirmedSelectedNumber;      //dodano za test 1.8
                    //netCode.NotifyOtherPlayersGameStateChanged(); //nije komano u 1.8 ali nisan sig bi li tribalo ic ovde

                    netCode.NotifyHostPlayerRankSelected((int)selectedCard.Rank);
                    netCode.NotifyHostPlayerTargetPlayerSelected(currentTurnTargetPlayer.PlayerId);
                }
                
                else
                {
                    Debug.Log("Logic error in MultiplayerGame :: OnOkSelected()!");
                }
            }

            else
            {   //moguce da je ovde bude problem, sta ako host klikne ok a nije jos izabra kartu
                netCode.NotifyHostPlayerOpponentConfirmed();
            }



            /*
            //kometano zbog testiranja maknit kasnije i priradit
            if (selectedCard != null && currentTurnTargetPlayer != null)
            {
                gameState = GameState.TurnConfirmedSelectedNumber;
                //ResetSelectedPlayer();    //mozda triba mozda ne      takoder triba maknit chose panel ako nije local player
                GameFlow();
            }

            if (selectedCard != null && currentTurnTargetPlayer == null)
            {
                ChoseThePlayerCardButtonPanel.SetActive(true);
                gameState = GameState.TurnSelectingPlayer;
                GameFlow();
            }

            if (currentTurnTargetPlayer == localPlayer && currentTurnPlayer != localPlayer && gameState != GameState.TurnSelectingPlayer && gameState != GameState.TurnConfirmedSelectedNumber)
            {   //testing
                List<byte> cardValuesFromTargetPlayer = gameDataManager.TakeCardValuesWithRankFromPlayer(currentTurnTargetPlayer, selectedRank);

                if (cardValuesFromTargetPlayer.Count > 0)
                {
                    gameDataManager.AddCardValuesToPlayer(currentTurnPlayer, cardValuesFromTargetPlayer);

                    bool senderIsLocalPlayer = currentTurnTargetPlayer == localPlayer;
                    currentTurnTargetPlayer.SendDisplayingCardToPlayer(currentTurnPlayer, cardAnimator, cardValuesFromTargetPlayer, senderIsLocalPlayer);
                    ResetSelectedPlayer();  //testing
                    ResetSelectedCard();    //testing
                    gameState = GameState.TurnSelectingNumber;
                }
                else
                {
                    ResetSelectedPlayer();  //testing
                    ResetSelectedCard();    //testing
                    gameState = GameState.TurnGoFish;
                    GameFlow();
                }
            }

            //ovaj dio ce ja mis ostat zakomentiran(ovo je original, ne koristin ga u 4igraca
            //if (gameState == GameState.TurnSelectingNumber && localPlayer == currentTurnPlayer)
            //{
            //    if (selectedCard != null)
            //    {
            //        gameState = GameState.TurnConfirmedSelectedNumber;
            //        GameFlow();
            //    }
            //}
            //else if (gameState == GameState.TurnWaitingForOpponentConfirmation && localPlayer == currentTurnTargetPlayer)
            //{
            //    gameState = GameState.TurnOpponentConfirmed;
            //    GameFlow();
            //}*/
        }

        //*********************** GameFlow() *******************************//
        protected override void GameFlow()
        {
            Debug.LogError("Should never be called MultiplayerGame :: GameFlow()!");
        }

        //*********************** NetCode Events **************************//
        public void OnGameDataReady(EncryptedData encryptedData)
        {
            if (NetworkClient.Instance.IsHost)
            {
                gameState = GameState.GameStarted;
                gameDataManager.SetGameState(gameState);

                netCode.ModifyGameData(gameDataManager.EncryptedData());

                netCode.NotifyOtherPlayersGameStateChanged();
            }//FALI ELSE, popravit, moguc uzrok bugova
        }

        public void OnGameDataChanged(EncryptedData encryptedData)
        {
            gameDataManager.ApplyEncryptedData(encryptedData);
            gameState = gameDataManager.GetGameState();
            currentTurnPlayer = gameDataManager.GetCurrentTurnPlayer();
            //ovde moze doc do razlike zbog odabira i sranja, fali i currentturntargetplayer(sta nije isti ka i u tutorialima)!!
            nextTurnPlayer = gameDataManager.GetNextTurnPlayer();
            selectedRank = gameDataManager.GetSelectedRank();
            currentTurnTargetPlayer = gameDataManager.GetCurrentTurnTargetPlayer();
        }

        public void OnGameStateChanged()
        {
            base.GameFlow();
        }

        public void OnRankSelected(Ranks rank)
        {
            selectedRank = rank;
            gameState = GameState.TurnSelectingPlayer;          //ovde se dijeli od tutoriala, posto se izabire sad player a ne ide dalje
                                                                //sve sta se napravilo za ranks tribalo bi napravit za selecting player
            gameDataManager.SetSelectedRank(selectedRank);
            gameDataManager.SetGameState(gameState);

            netCode.ModifyGameData(gameDataManager.EncryptedData());
            netCode.NotifyOtherPlayersGameStateChanged();
        }

        public void OnPlayerSelected(Player player)
        {
            currentTurnTargetPlayer = player;
            gameState = GameState.TurnConfirmedSelectedNumber;

            gameDataManager.SetCurrentTurnTargetPlayer(currentTurnTargetPlayer);
            gameDataManager.SetGameState(gameState);

            netCode.ModifyGameData(gameDataManager.EncryptedData());
            netCode.NotifyOtherPlayersGameStateChanged();
        }

        public void OnOpponentConfirmed()
        {
            gameState = GameState.TurnOpponentConfirmed;

            gameDataManager.SetGameState(gameState);

            netCode.ModifyGameData(gameDataManager.EncryptedData());
            netCode.NotifyOtherPlayersGameStateChanged();
        }
    }
}

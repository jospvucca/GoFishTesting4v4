using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity;
using UnityEngine.UI;

namespace GoFish
{
    public class Game : MonoBehaviour
    {
        public Text MessageText;

        protected CardAnimator cardAnimator;

        public GameDataManager gameDataManager;

        public List<Transform> PlayerPositions = new List<Transform>();
        public List<Transform> BookPositions = new List<Transform>();

        public GameObject ChoseThePlayerCardButtonPanel;

        [SerializeField]
        protected Player localPlayer;
        [SerializeField]
        protected Player[] remotePlayer;

        [SerializeField]
        protected Player currentTurnPlayer;
        [SerializeField]
        protected Player currentTurnTargetPlayer; //izminit ime po mogucnosti i maknit public
        [SerializeField]
        protected Player nextTurnPlayer;          //ovo bi tribalo izminit, current turn player klikne na ime drugih playera, dodan nextturnplayer radi lakseg snalazenja

        protected Card selectedCard;
        protected Ranks selectedRank;

        public enum GameState
        {
            Idel,
            GameStarted,
            TurnStarted,
            TurnSelectingNumber,
            TurnSelectingPlayer,    //dodano zbog vise igraca
            TurnConfirmedSelectedNumber,
            TurnWaitingForOpponentConfirmation,
            TurnOpponentConfirmed,
            TurnGoFish,
            GameFinished
        };

        public GameState gameState = GameState.Idel;

        protected void Awake()
        {

            localPlayer = new Player();
            localPlayer.PlayerId = "offline-player";
            localPlayer.PlayerName = "Player";
            localPlayer.Position = PlayerPositions[0].position;
            localPlayer.BookPosition = BookPositions[0].position;

            remotePlayer = new Player[3];

            remotePlayer[0] = new Player();
            remotePlayer[0].PlayerId = "offline-bot1";
            remotePlayer[0].PlayerName = "Bot1";
            remotePlayer[0].Position = PlayerPositions[1].position;
            remotePlayer[0].BookPosition = BookPositions[1].position;
            remotePlayer[0].IsAI = true;

            remotePlayer[1] = new Player();
            remotePlayer[1].PlayerId = "offline-bot2";
            remotePlayer[1].PlayerName = "Bot2";
            remotePlayer[1].Position = PlayerPositions[2].position;
            remotePlayer[1].BookPosition = BookPositions[2].position;
            remotePlayer[1].IsAI = true;

            remotePlayer[2] = new Player();
            remotePlayer[2].PlayerId = "offline-bot3";
            remotePlayer[2].PlayerName = "Bot3";
            remotePlayer[2].Position = PlayerPositions[3].position;
            remotePlayer[2].BookPosition = BookPositions[3].position;
            remotePlayer[2].IsAI = true;

            ChoseThePlayerCardButtonPanel.GetComponent<ButtonPlayerConfiguration>().AsignPlayersToButtons(remotePlayer[0].PlayerName, remotePlayer[1].PlayerName, remotePlayer[2].PlayerName);

            cardAnimator = FindObjectOfType<CardAnimator>();
            ChoseThePlayerCardButtonPanel.SetActive(false);

            currentTurnPlayer = null;
            currentTurnTargetPlayer = null;
            nextTurnPlayer = null;          //popravljanje sp igre

            gameDataManager = new GameDataManager(localPlayer, remotePlayer[0], remotePlayer[1], remotePlayer[2], "1");
        }

        protected void Start()
        {
            gameState = GameState.GameStarted;
            GameFlow();
        }

        void Update()
        {
            //currentTurnTargetPlayer = nextTurnPlayer;   //obavezno izbrisat i razvit logiku
        }

        //****************** Game Flow *********************//
        protected virtual void GameFlow()
        {
            if (gameState > GameState.GameStarted)
            {
                CheckPlayersBooks();
                ShowAndHidePlayersDisplayingCards();

                if (gameDataManager.GameFinished())
                {
                    gameState = GameState.GameFinished;
                }
            }

            switch (gameState)  //dodat jos jedan state za odabir igraca kojem se uzima karta
            {
                case GameState.Idel:
                    {
                        Debug.Log("IDEL");
                        break;
                    }
                case GameState.GameStarted:
                    {
                        Debug.Log("GameStarted");
                        OnGameStarted();
                        break;
                    }
                case GameState.TurnStarted:
                    {
                        Debug.Log("TurnStarted");
                        OnTurnStarted();
                        break;
                    }
                case GameState.TurnSelectingNumber:
                    {
                        Debug.Log("TurnSelectingNumber");
                        OnTurnSelectingNumber();
                        break;
                    }

                //**************** DODANO ZA VISE IGRACA ****************//
                case GameState.TurnSelectingPlayer:
                    {
                        Debug.Log("TurnSelectingPlayer");
                        OnTurnSelectingPlayer();
                        break;
                    }
                    ///////////////////////////////////////////////////////////
                case GameState.TurnConfirmedSelectedNumber:
                    {
                        Debug.Log("TurnComfirmedSelectedNumber");
                        OnTurnConfirmedSelectedNumber();
                        break;
                    }
                case GameState.TurnWaitingForOpponentConfirmation:
                    {
                        Debug.Log("TurnWaitingForOpponentConfirmation");
                        OnTurnWaitingForOpponentConfirmation();
                        break;
                    }
                case GameState.TurnOpponentConfirmed:
                    {
                        Debug.Log("TurnOpponentConfirmed");
                        OnTurnOpponentConfirmed();
                        break;
                    }
                case GameState.TurnGoFish:
                    {
                        Debug.Log("TurnGoFish");
                        OnTurnGoFish();
                        break;
                    }
                case GameState.GameFinished:
                    {
                        Debug.Log("GameFinished");
                        OnGameFinished();
                        break;
                    }
            }
        }

        protected virtual void OnGameStarted()
        {
            SwitchTurn();
            //gameDataManager = new GameDataManager(localPlayer, remotePlayer[0], remotePlayer[1], remotePlayer[2],"1");  //mozda redundantno
            gameDataManager.Shuffle();
            gameDataManager.DealCardValuesToPlayer(localPlayer, Constants.PLAYER_INITIAL_CARDS);
            gameDataManager.DealCardValuesToPlayer(remotePlayer[0], Constants.PLAYER_INITIAL_CARDS);
            gameDataManager.DealCardValuesToPlayer(remotePlayer[1], Constants.PLAYER_INITIAL_CARDS);
            gameDataManager.DealCardValuesToPlayer(remotePlayer[2], Constants.PLAYER_INITIAL_CARDS);

            cardAnimator.DealDisplayingCards(localPlayer, Constants.PLAYER_INITIAL_CARDS);
            cardAnimator.DealDisplayingCards(remotePlayer[0], Constants.PLAYER_INITIAL_CARDS);
            cardAnimator.DealDisplayingCards(remotePlayer[1], Constants.PLAYER_INITIAL_CARDS);
            cardAnimator.DealDisplayingCards(remotePlayer[2], Constants.PLAYER_INITIAL_CARDS);

            Debug.Log("Game :: OnGameStarted is finished.");
            gameState = GameState.TurnStarted;
        }

        protected virtual void OnTurnStarted()
        {
            //SwitchTurn();
            gameState = GameState.TurnSelectingNumber;
            GameFlow();
        }

        public void OnTurnSelectingNumber()
        {
            ResetSelectedCard();
            if (currentTurnPlayer == localPlayer)
            {
                SetMessage($"Your turn. Pick a card from your hand.");
            }
            else
            {
                SetMessage($"{currentTurnPlayer.PlayerName}'s turn");
            }

            if (currentTurnPlayer.IsAI)
            {
                selectedRank = gameDataManager.SelectRandomRanksFromPlayersCardValues(currentTurnPlayer);
                //gameState = GameState.TurnConfirmedSelectedNumber;
                gameState = GameState.TurnSelectingPlayer;
                //ResetSelectedPlayer();  //testing
                GameFlow();
            }
        }

        public void OnTurnSelectingPlayer()
        {
            //ResetSelectedPlayer();
            if (currentTurnPlayer == localPlayer)
            {
                ChoseThePlayerCardButtonPanel.SetActive(true);  //mozda ce ovde bit problem(mali), ako nije local player nece moc izabrat
                SetMessage($"Your turn. Pick a player who you want to ask for a card :{selectedRank}");
            }
            else
            {
                SetMessage($"{currentTurnPlayer.PlayerName}'s turn.");
            }

            if (currentTurnPlayer.IsAI)
            {
                List<Player> randomPlayer = new List<Player>();
                foreach (Player player in remotePlayer)
                    randomPlayer.Add(player);
                randomPlayer.Add(localPlayer);
                randomPlayer.Remove(currentTurnPlayer);

                int randomPlayerI = Random.Range(0, randomPlayer.Count);
                currentTurnTargetPlayer = randomPlayer[randomPlayerI];
                Debug.Log(currentTurnTargetPlayer + "\tfrom random.");

                gameState = GameState.TurnConfirmedSelectedNumber;
                GameFlow();
            }
        }

        protected virtual void OnTurnConfirmedSelectedNumber()
        {
            if (currentTurnPlayer == localPlayer)
            {
                SetMessage($"Asking {currentTurnTargetPlayer.PlayerName} for {selectedRank}s...");//dodat imena playera da se klikcu
            }
            else
            {
                SetMessage($"{currentTurnPlayer.PlayerName} is asking for {selectedRank}s...");
            }

            gameState = GameState.TurnWaitingForOpponentConfirmation;
            GameFlow();
        }

        public void OnTurnWaitingForOpponentConfirmation()
        {
            if (currentTurnTargetPlayer.IsAI)
            {
                gameState = GameState.TurnOpponentConfirmed;
                GameFlow();
            }
        }

        protected virtual void OnTurnOpponentConfirmed()
        {
            List<byte> cardValuesFromTargetPlayer = gameDataManager.TakeCardValuesWithRankFromPlayer(currentTurnTargetPlayer, selectedRank);
           
            if (cardValuesFromTargetPlayer.Count > 0)
            {
                gameDataManager.AddCardValuesToPlayer(currentTurnPlayer, cardValuesFromTargetPlayer);

                bool senderIsLocalPlayer = currentTurnTargetPlayer == localPlayer;
                currentTurnTargetPlayer.SendDisplayingCardToPlayer(currentTurnPlayer, cardAnimator, cardValuesFromTargetPlayer, senderIsLocalPlayer);
                ResetSelectedPlayer();  //testing           //mozda ovo sta je komentirano bude stvaralo probleme u mp(vjv ne, al za svaki slucaj napomena,zadnje editat)
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

        protected virtual void OnTurnGoFish()
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
                gameState = GameState.TurnStarted;
            }

            gameDataManager.AddCardValueToPlayer(currentTurnPlayer, cardValue);
        }

        public void OnGameFinished()
        {
            if (gameDataManager.Winner() == localPlayer)
            {
                SetMessage($"You WON!");
            }
            else
            {
                SetMessage($"You LOST!");
            }

            if(gameDataManager.Winner() == null)
            {
                SetMessage($"Probably a tie!(check debugs)");
            }
        }

        //****************** Helper Methods *********************//
        public void ResetSelectedCard()
        {
            if (selectedCard != null)
            {
                selectedCard.OnSelectedTransform(false);
                selectedCard = null;
                selectedRank = 0;
            }
        }

        //helper za vise igraca
        public void ResetSelectedPlayer()
        {
            Debug.Log("Logic error perhaps(should be called often) in Game :: ResetSelectedPlayer()!");
            if(currentTurnTargetPlayer != null)
            {
                currentTurnTargetPlayer = null;
            }
        }

        protected void SetMessage(string message)
        {
            MessageText.text = message;
        }

        public void SwitchTurn()
        {
            if (currentTurnPlayer == null || currentTurnPlayer.PlayerId == "null")
            {
                currentTurnPlayer = localPlayer;
                //gameDataManager.SetCurrentTurnPlayer(currentTurnPlayer);
                nextTurnPlayer = remotePlayer[0];
                return;
            }

            else if (currentTurnPlayer == localPlayer)
            {
                currentTurnPlayer = remotePlayer[0];
                //gameDataManager.SetCurrentTurnPlayer(currentTurnPlayer);
                nextTurnPlayer = remotePlayer[1];
            }

            else if (currentTurnPlayer == remotePlayer[0]) 
            {
                currentTurnPlayer = remotePlayer[1];
                //gameDataManager.SetCurrentTurnPlayer(currentTurnPlayer);
                nextTurnPlayer = remotePlayer[2];
            }

            else if (currentTurnPlayer == remotePlayer[1])
            {
                currentTurnPlayer = remotePlayer[2];
                //gameDataManager.SetCurrentTurnPlayer(currentTurnPlayer);
                nextTurnPlayer = localPlayer;
            }

            else if (currentTurnPlayer == remotePlayer[2])
            {
                currentTurnPlayer = localPlayer;
                //gameDataManager.SetCurrentTurnPlayer(currentTurnPlayer);
                nextTurnPlayer = remotePlayer[0];
            }

            else
            {
                Debug.LogWarning("Logic error in Game.cs :: SwitchTurn()!");
            }
        }

        public void PlayerShowBooksIfNecessary(Player player)
        {
            Dictionary<Ranks, List<byte>> books = gameDataManager.GetBooks(player);

            if (books != null)
            {
                foreach (var book in books)
                {
                    player.ReceiveBook(book.Key, cardAnimator);

                    gameDataManager.RemoveCardValuesFromPlayer(player, book.Value);
                }

                gameDataManager.AddBooksForPlayer(player, books.Count);
            }
        }

        public void CheckPlayersBooks()
        {
            List<byte> playerCardValues = gameDataManager.PlayerCards(localPlayer);
            localPlayer.SetCardValues(playerCardValues);
            PlayerShowBooksIfNecessary(localPlayer);

            playerCardValues = gameDataManager.PlayerCards(remotePlayer[0]);
            remotePlayer[0].SetCardValues(playerCardValues);
            PlayerShowBooksIfNecessary(remotePlayer[0]);

            playerCardValues = gameDataManager.PlayerCards(remotePlayer[1]);
            remotePlayer[1].SetCardValues(playerCardValues);
            PlayerShowBooksIfNecessary(remotePlayer[1]);

            playerCardValues = gameDataManager.PlayerCards(remotePlayer[2]);
            remotePlayer[2].SetCardValues(playerCardValues);
            PlayerShowBooksIfNecessary(remotePlayer[2]);
        }

        public void ShowAndHidePlayersDisplayingCards()
        {
            localPlayer.ShowCardValues();
            remotePlayer[0].HideCardValues();
            remotePlayer[1].HideCardValues();
            remotePlayer[2].HideCardValues();
        }

        //****************** User Interaction *********************//
        public void OnCardSelected(Card card)
        {
            if (gameState == GameState.TurnSelectingNumber)
            {
                if (card.OwnerId == currentTurnPlayer.PlayerId)
                {
                    if (selectedCard != null)
                    {
                        selectedCard.OnSelectedTransform(false);
                        selectedRank = 0;
                    }

                    selectedCard = card;
                    selectedRank = selectedCard.Rank;
                    selectedCard.OnSelectedTransform(true);
                    //selectedCard.transform.position = new Vector3(selectedCard.transform.position.x, selectedCard.transform.position.y + 1f, selectedCard.transform.position.z);
                    SetMessage($"Ask for {selectedCard.Rank}s ?");
                    ChoseThePlayerCardButtonPanel.SetActive(true);  //testing
                    gameState = GameState.TurnSelectingPlayer;      //testing
                }
            }
        }

        //vise igraca

            //RADI, ali samo sa if game state komentaron, nepravilan prolaz kroz stateove ocito,
            //sredit to i dalje bi tribalo bit u redu, ne zaboravit na clean code
        public void OnButtonSelected(Button button)     
        {
            if (currentTurnPlayer == localPlayer && gameState == GameState.TurnSelectingPlayer) 
            { 
            Debug.Log("Selected: Game :: OnButtonSelected(Button) for testing: \t " + button.gameObject.GetComponentInChildren<Text>().text);
                //ResetSelectedPlayer();
                //if (gameState == GameState.TurnSelectingPlayer)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (remotePlayer[i].PlayerName == button.gameObject.GetComponentInChildren<Text>().text)
                        {
                            currentTurnTargetPlayer = remotePlayer[i];
                            Debug.Log(currentTurnTargetPlayer);
                            SetMessage($"Ask {currentTurnTargetPlayer.PlayerName} for {selectedCard.Rank}s ?(after button)");
                            ChoseThePlayerCardButtonPanel.SetActive(false);
                        }

                    }

                    if (currentTurnTargetPlayer == null)
                    {
                        Debug.LogError("Logic error in Game :: OnButtonSelected(Button)! (In testing!)");
                    }
                }
            }
        }

        public virtual void OnOkSelected()
        {
            ////kometano zbog testiranja maknit kasnije i priradit
            //if (selectedCard != null && currentTurnTargetPlayer != null)
            //{
            //    gameState = GameState.TurnConfirmedSelectedNumber;
            //    //ResetSelectedPlayer();    //mozda triba mozda ne      takoder triba maknit chose panel ako nije local player
            //    GameFlow();
            //}

            //if(selectedCard != null && currentTurnTargetPlayer == null)
            //{
            //    ChoseThePlayerCardButtonPanel.SetActive(true);
            //    gameState = GameState.TurnSelectingPlayer;
            //    GameFlow();
            //}

            //if (currentTurnTargetPlayer == localPlayer && currentTurnPlayer != localPlayer && gameState != GameState.TurnSelectingPlayer && gameState != GameState.TurnConfirmedSelectedNumber)
            //{   //testing
            //    List<byte> cardValuesFromTargetPlayer = gameDataManager.TakeCardValuesWithRankFromPlayer(currentTurnTargetPlayer, selectedRank);

            //    if (cardValuesFromTargetPlayer.Count > 0)
            //    {
            //        gameDataManager.AddCardValuesToPlayer(currentTurnPlayer, cardValuesFromTargetPlayer);

            //        bool senderIsLocalPlayer = currentTurnTargetPlayer == localPlayer;
            //        currentTurnTargetPlayer.SendDisplayingCardToPlayer(currentTurnPlayer, cardAnimator, cardValuesFromTargetPlayer, senderIsLocalPlayer);
            //        ResetSelectedPlayer();  //testing
            //        ResetSelectedCard();    //testing
            //        gameState = GameState.TurnSelectingNumber;
            //    }
            //    else
            //    {
            //        ResetSelectedPlayer();  //testing
            //        ResetSelectedCard();    //testing
            //        gameState = GameState.TurnGoFish;
            //        GameFlow();
            //    }
            //}


                if (gameState == GameState.TurnSelectingPlayer && localPlayer == currentTurnPlayer)
                {
                    if (selectedCard != null)
                    {
                        gameState = GameState.TurnConfirmedSelectedNumber;
                        GameFlow();
                    }
                }
                else if (gameState == GameState.TurnWaitingForOpponentConfirmation && localPlayer == currentTurnTargetPlayer)
                {
                    gameState = GameState.TurnOpponentConfirmed;
                    GameFlow();
                }                       //SINGLE PLAYER FUNKCIONIRA I OVAKO, AKO BUDE PROBLEMA ISKOMBAT U MP
        }

        //****************** Animator Event *********************//
        public virtual void AllAnimationsFinished()
        {
            GameFlow();
        }
    }
}

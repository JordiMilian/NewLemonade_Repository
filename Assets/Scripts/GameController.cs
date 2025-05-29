using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public enum gameStates
{
    MainMenu,
    Playing,
    Tutorial,
    Question,
    PauseMenu,
    Bankrupt,
    Billionare
}
public class GameController : MonoBehaviour
{
    float currentMoney;

    event Action<float> OnMoneyUpdated;
    public float CurrentMoney
    {
        get { return currentMoney; }
        private set 
        {
            if(currentMoney != value)
             { currentMoney = value; OnMoneyUpdated?.Invoke(value); }
        }
    }
    public int CurrentLemons;
    public int LemonsPerBuy;
    public int LemonsPerSell;
    public int LemonsPerAutomaticSell;
    bool arePurchaseDisabled;

    public float BuyingPrice;
    public float SellingPrice;
    [Header("Start game stats")]
    [SerializeField] float StartingMoney;
    [SerializeField] int startingLemons;
    [SerializeField] int starting_lemonsPerBuy, starting_lemonsPerSell, starting_lemonsPerAutomaticSell;
    [SerializeField] float starting_buyingPrice, starting_SellingPrice;
    [Header("Money Testing")]
    [SerializeField] bool isTestingMoneyString;
    [SerializeField] float MoneyTestingAmount;
    [SerializeField] string MoneyTestingString;

    [Header ("Other References")]
    [SerializeField] QuestionsHolder questionsHolder;
    [SerializeField] GraphDisplayer graphDisplayer;
    [SerializeField] Animator Animator_QuestionUI, Animator_TableLemons, Animator_Tutorial;
    [HideInInspector] public List<Question> questions;
    [SerializeField] MusicLoopDistorder musicControl;
    [SerializeField] PostProcessingController postProController;
    [SerializeField] AudioDistortionController distortionController;
    [SerializeField] GameObject MainMenuCanvas;
    [SerializeField] Animator anim_enoughMoney, anim_enoughLemons;

    [Header("Game over screen canvases")]
    [SerializeField] GameObject GO_BillionareScreenRoot;
    [SerializeField] GameObject GO_BankruptScreenRoot;

    [Header("Audio Clips")]
    [SerializeField] AudioClip AudioClip_Buy;
    [SerializeField] AudioClip AudioClip_Sell, AudioClip_CantBuy, AudioClip_CantSell, AudioClip_GoalReached, AudioClip_AnswerPressed;

    [Serializable]
    public class Question
    {
        public string Title;
        public float MoneyGoal;
        public string QuestionText;
        public Dude_Controller.dudeTypes dudeToAppear;
        public Answer answer1, answer2;
        
    }
    [Serializable]
    public struct Answer
    {
        public string answerTxt;
        public float pricePercentOfGoal;
        public float price; 
        public UpgradeTypes upgradeType;
        public float variable;
    }
    private void Start()
    {
        GO_BillionareScreenRoot.SetActive(false);
        GO_BankruptScreenRoot.SetActive(false);

        questions = questionsHolder.questions;
        hidePurchaseButtons();
        disablePurchaseButtons();
        GO_QuestionsRoot.SetActive(false);
        SetUpMoneyBar();
        pauseAutomising = true;
        SetUpTimerBar();
        graphDisplayer.stopGraph = true;

        UpdateTextDisplays();
        TutorialCanvas.SetActive(false);

        AddNewState(gameStates.MainMenu);

        restartStats();
    }
    #region GAME STATES
    public Stack<gameStates> gameStatesStack = new Stack<gameStates>();

    public void AddNewState(gameStates newState)
    {
        gameStatesStack.Push(newState);
        ManageCurrentState();
    }
    void ManageCurrentState()
    {
        switch (gameStatesStack.Peek())
        {
            case gameStates.MainMenu:
                showMainMenu();
                break;
            case gameStates.Playing:
                UnpauseGame();
                showPurchaseButtons();
                enablePurchaseButtons();
                graphDisplayer.stopGraph = false;
                
                break;
            case gameStates.Tutorial:
                isTutorialPaused = false;
                TutorialCanvas.SetActive(true);
                showPurchaseButtons();
                break;
            case gameStates.Question:
                showQuestionUI();
                disablePurchaseButtons();
                PauseGame();
                break;
            case gameStates.PauseMenu:
                PauseGame();
                musicControl.AddLowFilter();
                bestTime_PauseCouting();
                Canvas_Pause.gameObject.SetActive(true);
                break;
            case gameStates.Bankrupt:
                PauseGame();
                BankrupedScreen();
                break;
            case gameStates.Billionare:
                PauseGame();
                BillionareScreen();
                break;
        }
        Debug.Log("Entered state: " + gameStatesStack.Peek());
    }
    public void CloseCurrentState()
    {
        gameStates currentState = gameStatesStack.Peek();

        switch (currentState)
        {
            case gameStates.MainMenu:
                break;
            case gameStates.Playing:
                PauseGame();
                break;
            case gameStates.Tutorial:
                TutorialCanvas.SetActive(false);
                Debug.Log("Exited tutorial");
                break;
            case gameStates.Question:
                hideQuestionUI();
                Dude_Controller.Instance.CallHideDude();
                UpdateTextDisplays();
                break;
            case gameStates.PauseMenu:
                UnpauseGame();
                musicControl.RemoveLowFilter();
                bestTime_ResumeCounting();
                Canvas_Pause.gameObject.SetActive(false);
                break;
            case gameStates.Bankrupt:
                GO_BankruptScreenRoot.SetActive(false);
                break;
            case gameStates.Billionare:
                GO_BillionareScreenRoot.SetActive(false);
                break;
        }
        gameStatesStack.Pop();
        Debug.Log("Closed state: " + currentState + " Now in state: " + gameStatesStack.Peek());

        ManageCurrentState();
    }
    public void closeStatesUntilReachingState(gameStates targetState)
    {
        while (gameStatesStack.Count > 1)
        {
            if(gameStatesStack.Peek() == targetState) { break; }

            CloseCurrentState();
        }
        ManageCurrentState();
    }
    #endregion
    #region MAIN MENU
    void PauseGame()
    {
        pauseAutomising = true;
        pauseTimer();
        graphDisplayer.stopGraph = true;
        disablePurchaseButtons();
        StopAllHolds();
        isTutorialPaused = true;
    }
    void UnpauseGame()
    {
        pauseAutomising = false;
        resumeTimer();
        graphDisplayer.stopGraph = false;
        enablePurchaseButtons();
        isTutorialPaused = false;
    }
    public void showMainMenu()
    {
        MainMenuCanvas.SetActive(true);
        postProController.SetPostProcesing(postProController.postPoInfo_MainMenu);
        //MusicLoopDistorder.musicDistortionStats baseStats = new MusicLoopDistorder.musicDistortionStats();
        //baseStats = MusicLoopDistorder.musicDistortionStats.CopyStats(baseStats, musicControl.musicStats_current);
        musicControl.TransitionFromCurrentStats(musicControl.musicStats_MainMenu, musicControl.seconds_TransitionToMainMenu);
        if(bestTimeSeconds > 99999) { TMP_BestTimeDisplay.gameObject.SetActive(false); }
        else { TMP_BestTimeDisplay.gameObject.SetActive(true); TMP_BestTimeDisplay.text = secondsFToString(bestTimeSeconds); }

    }
    void restartStats()
    {
        currentMoney = StartingMoney;
        CurrentLemons = startingLemons;
        LemonsPerBuy = starting_lemonsPerBuy;
        LemonsPerSell = starting_lemonsPerSell;
        SellingPrice = starting_SellingPrice;
        BuyingPrice = starting_buyingPrice;
        LemonsPerAutomaticSell = starting_lemonsPerAutomaticSell;
        isAutomatising = false;
        nextQuestionIndex = 0;
        UpdateTextDisplays();
    }
    public void Button_StartPlaying()
    {
        restartStats();
        restartTimer();
       
        MainMenuCanvas.SetActive(false);
        musicControl.TransitionStats(musicControl.musicStats_MainMenu, musicControl.musicStats_startQuestion, musicControl.seconds_TransitionToStartPlaying);
        SFX_PlayerSingleton.Instance.playSFX(graphDisplayer.AudioClip_GraphTransition);
        StartCoroutine(BeginGameCoroutine());
        postProController.SetPostProcesing(postProController.postPoinfo_startQuestions);
    }
    public void Button_RestartGame()
    {
        restartStats();
        restartTimer();
        closeStatesUntilReachingState(gameStates.Playing);

        musicControl.TransitionFromCurrentStats(musicControl.musicStats_startQuestion, 1);
        postProController.SetPostProcesing(postProController.postPoinfo_startQuestions);

        StartCoroutine(BeginGameCoroutine());
    }
    [Header("Start Game stuff")]
    [SerializeField] AnimationCurve FieldOfViewCurve;
    [SerializeField] float seconds_startGameTransition;
    [SerializeField] Volume postProcessingVolume;
    [SerializeField] int TutorialPhasesCount;
    [SerializeField] GameObject TutorialCanvas;
    public bool skipTutorial;
    bool isTutorialPaused = false;
    IEnumerator BeginGameCoroutine()
    {
        
        if (skipTutorial) { graphDisplayer.HideGraph(); }
        yield return StartCoroutine(LensDistortionPopUp());

       
        if (skipTutorial) 
        {
            graphDisplayer.RestartDisplaying();
            bestTime_StartNewCounting();
            AddNewState(gameStates.Playing);

            graphDisplayer.ShowGraph();
            TutorialCanvas.SetActive(false);
            yield break;
        }
        AddNewState(gameStates.Tutorial);
        UnpauseGame();
        graphDisplayer.RestartDisplaying();
        yield return new WaitForSeconds(1);

        int tutorialPhasesPased = 0;
        tutorialPhasesPased++;
        Animator_Tutorial.SetInteger("phases", tutorialPhasesPased); ;
        PauseGame();
        isTutorialPaused = false;
    Waitagain:
        if(Input.GetMouseButtonDown(0))
        {
            if (isTutorialPaused) { yield return null; goto Waitagain; }
        }
        else { yield return null; goto Waitagain; }

        tutorialPhasesPased++;
        Animator_Tutorial.SetInteger("phases",tutorialPhasesPased);
        Debug.Log("next shadow: " + tutorialPhasesPased);
        yield return new WaitForSeconds(0.75f);
       
        
        if (tutorialPhasesPased < TutorialPhasesCount) { goto Waitagain; }

        //skipTutorial = true;
        pauseAutomising = false;
        restartTimer();
        graphDisplayer.RestartDisplaying();

        bestTime_StartNewCounting();
        AddNewState(gameStates.Playing);
    }
    IEnumerator LensDistortionPopUp()
    {
        postProcessingVolume.profile.TryGet(out LensDistortion lensDistortion);
        float timer = 0;
        while (timer < seconds_startGameTransition)
        {
            timer += Time.deltaTime;
            lensDistortion.intensity.value = FieldOfViewCurve.Evaluate(timer / seconds_startGameTransition);
            yield return null;
        }
        lensDistortion.intensity.value = 0;
    }
    public void Button_ExitGame()
    {
        Application.Quit();
    }
    #endregion
    #region BEST TIME

    float bestTimeSeconds = 9999999f;
    float bestTime_CurrentTimeSeconds;
    Coroutine bestTime_currentCoroutine;
    void bestTime_StartNewCounting()
    {
        if(bestTime_currentCoroutine != null) { StopCoroutine(bestTime_currentCoroutine); }

        bestTime_CurrentTimeSeconds = 0;
        bestTime_currentCoroutine = StartCoroutine(bestTime_countingCoroutine());
    }
    void bestTime_PauseCouting()
    {
        if (bestTime_currentCoroutine != null) { StopCoroutine(bestTime_currentCoroutine); }
    }
    void bestTime_ResumeCounting()
    {
        if (bestTime_currentCoroutine != null) { StopCoroutine(bestTime_currentCoroutine); }
        bestTime_currentCoroutine = StartCoroutine(bestTime_countingCoroutine());
    }
    void bestTime_EndCounting()
    {
        if (bestTime_currentCoroutine != null) { StopCoroutine(bestTime_currentCoroutine); }
        if(bestTime_CurrentTimeSeconds < bestTimeSeconds) { bestTimeSeconds = bestTime_CurrentTimeSeconds; }
    }
    IEnumerator bestTime_countingCoroutine()
    {
        while(true)
        {
            bestTime_CurrentTimeSeconds += Time.deltaTime;
            //Debug.Log(bestTime_CurrentTimeSeconds);
            yield return null;
        }
    }
    static string secondsFToString(float rawSeconds)
    {
        float rawMinuts = rawSeconds / 60f;
        int minuts = Mathf.RoundToInt(rawMinuts - .5f);

        float remainingRawSeconds = rawSeconds - (minuts * 60);
        int seconds = Mathf.RoundToInt(remainingRawSeconds);

        return minuts + ":" + seconds;
    }

    #endregion
    #region AUTOMATIC SELLING AND BUYING
    [Header("Automatic Sell")]

    public float secondsToAutomaticSell;
    float SellingTimer;
    public bool isAutomatising;
    bool pauseAutomising;

    bool isHoldingBuy, isHoldingSell;
    float holdBuyTimer, holdSellTimner;
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            onPausePressed();
        }
        /*
        if (Input.GetKeyDown(KeyCode.M))
        {
            CurrentMoney = questionsHolder.questions[nextQuestionIndex].MoneyGoal;
            BuyLemons();
        }
        */
        if(isHoldingBuy)
        {
            holdBuyTimer += Time.deltaTime;
            if(holdBuyTimer > timeBetweenPurchases)
            {
                holdBuyTimer = 0;
                BuyLemons();
            }
        }
        if(isHoldingSell)
        {
            holdSellTimner += Time.deltaTime;
            if(holdSellTimner > timeBetweenPurchases)
            {
                holdSellTimner = 0;
                SellLemons();
            }
        }

        if (isTestingMoneyString) { MoneyTestingString = floatToMoneyString(MoneyTestingAmount); }


        if (pauseAutomising) { return; }

       if(isAutomatising)
       {
            SellingTimer += Time.deltaTime;
            if (SellingTimer >= secondsToAutomaticSell)
            {
                automaticPurchase();
                SellingTimer = 0;
            }
       }
        
    }
    
    #endregion
    #region BUY AND SELL
    public void BuyLemons()
    {
        if (arePurchaseDisabled) { return; }
        int affordableLemons = Mathf.RoundToInt((CurrentMoney / BuyingPrice) - 0.5f);

        if (affordableLemons < LemonsPerBuy)
        {
            if(affordableLemons == 0) 
            { 
                SFX_PlayerSingleton.Instance.playSFX(AudioClip_CantBuy,0.1f);
                anim_enoughMoney.SetTrigger("appear");
                return; }
            CurrentMoney -= affordableLemons * BuyingPrice;
            CurrentLemons += affordableLemons;
        }
        else
        {
            CurrentMoney -= LemonsPerBuy * BuyingPrice;
            CurrentLemons += LemonsPerBuy;
        }
        UpdateTextDisplays();
        SFX_PlayerSingleton.Instance.playSFX(AudioClip_Buy, 0.1f);
    }
    public void SellLemons()
    {
        if (arePurchaseDisabled) { return; }
        if (CurrentLemons < LemonsPerSell)
        {
            if(CurrentLemons == 0)
            { 
                SFX_PlayerSingleton.Instance.playSFX(AudioClip_CantBuy, 0.1f);
                anim_enoughLemons.SetTrigger("appear");
                return; }

            CurrentMoney += CurrentLemons * SellingPrice;
            CurrentLemons -= CurrentLemons;
        }
        else
        {
            CurrentMoney += LemonsPerSell * SellingPrice;
            CurrentLemons -= LemonsPerSell;
        }
        UpdateTextDisplays();
        SFX_PlayerSingleton.Instance.playSFX(AudioClip_Sell, 0.1f);
    }
    public void automaticPurchase()
    {
        if(CurrentLemons < LemonsPerAutomaticSell) 
        { 
            int neededLemons = LemonsPerAutomaticSell - CurrentLemons;
            CurrentLemons += neededLemons;
            CurrentMoney -= neededLemons * SellingPrice;
        }

        CurrentLemons -= LemonsPerAutomaticSell; 
        CurrentMoney += SellingPrice * LemonsPerAutomaticSell;

        UpdateTextDisplays();
    }
    #endregion
    #region Hold to BUY and SELL
    [Header("Hold to BUY and SELL")]
    [SerializeField] float timeBetweenPurchases = .25f;
    Coroutine buyingCoroutine, sellingCoroutine;
    public void OnBuyDown()
    {
        BuyLemons();
        isHoldingBuy = true;
        holdBuyTimer = 0;
    }

    public void OnBuyUp()
    {
        isHoldingBuy = false;
    }
    public void OnSellDown() 
    {
        SellLemons();
        isHoldingSell = true;
        holdSellTimner = 0;
    }
    public void OnSellUp() 
    {
        isHoldingSell = false; 
    }
    public void StopAllHolds()
    {
        isHoldingBuy = false;
        isHoldingSell = false;
    }
    #endregion
    #region ANSWER PROCESSING

    public enum UpgradeTypes
    {
        MultiplyUnitsPerExchange, AddBuyingPricePercent, AddSellingPricePercent, ImproveBothAutomationsPercent, StartAutomation, MultiplyLemonsPerAutomaticSell
    }
   
    public void AnswerPressed(int answer)
    {
       
        if (answer == 1) { ProcessAnswer(questions[nextQuestionIndex].answer1); }
        else if(answer == 2) { ProcessAnswer(questions[nextQuestionIndex].answer2); }

        CloseCurrentState();

        SFX_PlayerSingleton.Instance.playSFX(AudioClip_AnswerPressed);
        graphDisplayer.StartHeightTransition();
        graphDisplayer.displayGraph();

        restartTimer();

        

        //
        void ProcessAnswer(Answer answer)
        {
            switch (answer.upgradeType)
            {
                case UpgradeTypes.MultiplyUnitsPerExchange:
                    LemonsPerBuy *= (int)answer.variable;
                    LemonsPerSell *= (int)answer.variable;
                    break;
                case UpgradeTypes.AddSellingPricePercent:
                    SellingPrice += SellingPrice * (answer.variable / 100);
                    break;
                case UpgradeTypes.AddBuyingPricePercent:
                    BuyingPrice += BuyingPrice * (answer.variable / 100);
                    break;
                case UpgradeTypes.ImproveBothAutomationsPercent:
                    secondsToAutomaticSell += secondsToAutomaticSell * (answer.variable / 100);
                    break;
                case UpgradeTypes.StartAutomation:
                    isAutomatising = true;
                    secondsToAutomaticSell = answer.variable;
                    break;
                case UpgradeTypes.MultiplyLemonsPerAutomaticSell:
                    LemonsPerAutomaticSell *= (int)answer.variable;
                    break;

            }
            nextQuestionIndex++;
            CurrentMoney -= answer.price;
        }
    }
   
    #endregion
    #region GAME OVER TIMER

    [SerializeField] float TimeToReachGoal;
    float GameOverTimer;
    Coroutine currentTimer;
    IEnumerator TimerToReachGoalCoroutine()
    {
        while(GameOverTimer < TimeToReachGoal)
        {
            GameOverTimer += Time.deltaTime;
            graphDisplayer.UpdateTimerBar(GameOverTimer / TimeToReachGoal);
            yield return null;
        }
        AddNewState(gameStates.Bankrupt);
    }
    void BankrupedScreen()
    {
        GO_BankruptScreenRoot.SetActive(true);
        GO_BankruptScreenRoot.GetComponent<Animator>().SetTrigger("appear");
        disablePurchaseButtons();
        hideQuestionUI();
        pauseAutomising = true;
        StopAllHolds();
        musicControl.TransitionFromCurrentStats(musicControl.musicStats_MainMenu, musicControl.secondsToTransitionToBillionare);
        postProController.SetPostProcesing(postProController.postPoInfo_GameOverScreens);
    }
    void resumeTimer()
    {
        if(currentTimer != null) { StopCoroutine(currentTimer); }
        currentTimer = StartCoroutine(TimerToReachGoalCoroutine());
    }
    void pauseTimer()
    {
        if (currentTimer != null) { StopCoroutine(currentTimer); }
    }
    void restartTimer()
    {
        GameOverTimer = 0;
        resumeTimer();
    }
    #endregion
    #region UI Display
    [Header("BARS")]
    [SerializeField] Transform moneySlider_size1;
    [SerializeField] Transform timerSlider_size01;
    [SerializeField] Transform TfTimerBar;
    Vector2 timerBar_startingPos, timerBar_endPos;
    void SetUpTimerBar()
    {
        timerBar_startingPos = new Vector2(graphDisplayer.lineRenderer.transform.position.x, TfTimerBar.transform.position.y);
        timerBar_endPos = timerBar_startingPos + new Vector2(graphDisplayer.MaxWidth, 0);
    }
    [Header("Text Mesh Pros")]
    [SerializeField] TextMeshProUGUI TMP_moneyDisplay;
    [SerializeField] TextMeshProUGUI TMP_Lemons, TMP_Question, TMP_Answer1_Title, TMP_Answer2_Title;
    [SerializeField] TextMeshProUGUI TMP_Answer1_Price, TMP_Answer2_Price, TMP_Answer1_Upgrade, TMP_Answer2_Upgrade;
    [SerializeField] TextMeshProUGUI TMP_Buy_Amount, TMP_Sell_Amount, TMP_Buy_Price, TMP_Sell_Price, TMP_CurrentMoney_QuestionUI;
    [SerializeField] TextMeshProUGUI TMP_BestTimeDisplay;
    
    void UpdateTextDisplays()
    {
        TMP_moneyDisplay.text = floatToMoneyString(CurrentMoney);
        TMP_Lemons.text = CurrentLemons.ToString() + " Lemons";
        TMP_Buy_Amount.text = "Buy "+ LemonsPerBuy.ToString() + " Lemons";
        TMP_Sell_Amount.text = "Sell " + LemonsPerSell.ToString() + " Lemonades";
        TMP_Buy_Price.text = floatToMoneyString(LemonsPerBuy * BuyingPrice);
        TMP_Sell_Price.text = floatToMoneyString(LemonsPerSell * SellingPrice);
        TMP_Answer1_Upgrade.text = upgradeToDisplayString(questions[nextQuestionIndex].answer1);
        TMP_Answer2_Upgrade.text = upgradeToDisplayString(questions[nextQuestionIndex].answer2);

        //
        string upgradeToDisplayString(Answer answer)
        {
            switch (answer.upgradeType)
            {
                case UpgradeTypes.MultiplyUnitsPerExchange:
                    return "x" + answer.variable.ToString() + " Lemons per Deal";
                case UpgradeTypes.AddSellingPricePercent:
                    return "+" + answer.variable.ToString() + "% Selling Price";
                case UpgradeTypes.AddBuyingPricePercent:
                    return answer.variable.ToString() + "% Buying Price";
                case UpgradeTypes.StartAutomation:
                    return "1 Automatic Sell every " + answer.variable.ToString() + " seconds";
                case UpgradeTypes.ImproveBothAutomationsPercent:
                    return answer.variable.ToString() + "% seconds between Automatic Sells";
                case UpgradeTypes.MultiplyLemonsPerAutomaticSell:
                    return "x" + answer.variable.ToString() + " lemons per Automatic Sell";
            }
            return answer.variable.ToString() + " missing description";
        }
    }
    #region Float to Money String
    string floatToMoneyString(float amount)
    {

        if(amount < 10) { string rawNumbers = amount.ToString("F2"); return  "$ " + rawNumbers; }
        if (amount >= 10 && amount < 100) { string rawNumbers = amount.ToString("F1"); return "$ "+ rawNumbers; }


        string procesedNumber = amount.ToString("F0");
        int numberLength = procesedNumber.Length;

        if (numberLength > 3 && numberLength <= 6)
        {
            if(numberLength < 6) 
            {
                procesedNumber = procesedNumber.Remove(procesedNumber.Length - 2, 2);
                procesedNumber = procesedNumber.Insert(procesedNumber.Length - 1, ",");
            }
            else { procesedNumber = procesedNumber.Remove(procesedNumber.Length - 3, 3); }
            
            procesedNumber +="K";
        }
        else if(procesedNumber.Length > 6 && procesedNumber.Length <= 9)
        {
            if(numberLength < 9)
            {
                procesedNumber = procesedNumber.Remove(numberLength - 5, 5);
                procesedNumber = procesedNumber.Insert(procesedNumber.Length - 1, ",");
            }
            else { procesedNumber = procesedNumber.Remove(procesedNumber.Length - 6, 6); }
            
            procesedNumber += "M";
        }
        else if(procesedNumber.Length > 9)
        {
            procesedNumber = procesedNumber.Remove(procesedNumber.Length - 9, 9);
            procesedNumber += "B";
        }
        return  "$ " + procesedNumber;
    }
    #endregion

    [Header("Buying/Selling Buttons")]
    [SerializeField] GameObject Button_Buy;
    [SerializeField] GameObject Button_Sell;
    [Space(1)]
    [SerializeField] GameObject GO_QuestionsRoot;
    void disablePurchaseButtons()
    {
        arePurchaseDisabled = true;
    }
    void enablePurchaseButtons()
    {
        arePurchaseDisabled = false;
    }
    void hidePurchaseButtons()
    {
        Button_Buy.SetActive(false);
        Button_Sell.SetActive(false);
    }
    void showPurchaseButtons()
    {
        Button_Sell.SetActive(true);
        Button_Buy.SetActive(true);
    }
    void showQuestionUI()
    {
        GO_QuestionsRoot.SetActive(true);
        TMP_Question.text = questions[nextQuestionIndex].QuestionText;
        TMP_Answer1_Title.text = questions[nextQuestionIndex].answer1.answerTxt;
        TMP_Answer2_Title.text = questions[nextQuestionIndex].answer2.answerTxt;
        TMP_Answer1_Price.text = floatToMoneyString(questions[nextQuestionIndex].answer1.price);
        TMP_Answer2_Price.text = floatToMoneyString(questions[nextQuestionIndex].answer2.price);
        TMP_Answer1_Upgrade.text = questions[nextQuestionIndex].answer1.variable.ToString();
        TMP_Answer2_Upgrade.text = questions[nextQuestionIndex].answer2.variable.ToString();
        TMP_CurrentMoney_QuestionUI.text = floatToMoneyString(currentMoney);

        
        musicControl.AddLowFilter();
        
        Debug.Log("Goal reched: " + nextQuestionIndex);

        pauseAutomising = true;
    }
    void hideQuestionUI()
    {
        GO_QuestionsRoot.SetActive(false);
        Animator_TableLemons.SetTrigger("jump");
        musicControl.RemoveLowFilter();
        musicControl.SetQuestionsDistortion(nextQuestionIndex, questionsHolder.questions.Count -1);
        postProController.SetQuestionsPostProcessing(nextQuestionIndex, questionsHolder.questions.Count - 1);
        distortionController.SetAudioDistortion(nextQuestionIndex, questionsHolder.questions.Count - 1);
        pauseAutomising = false;
        StartCoroutine(LensDistortionPopUp());
    }
    #endregion
    #region NEXT QUESTION
    public int nextQuestionIndex = 0;
    void SetUpMoneyBar()
    {
        OnMoneyUpdated += onMoneyUpdated;
    }
    void onMoneyUpdated(float amount)
    {
        if (amount >= questions[nextQuestionIndex].MoneyGoal)
        {
            OnGoalReached();
        }
    }
    void OnGoalReached()
    {
        if(nextQuestionIndex == questionsHolder.questions.Count -1)
        {
            AddNewState(gameStates.Billionare);
            return;
        }


        graphDisplayer.makeNewPointAndDisplay(true);

        
        SFX_PlayerSingleton.Instance.playSFX(AudioClip_GoalReached);
        
        AddNewState(gameStates.Question);
        Animator_QuestionUI.SetTrigger("Appear");
    }
    [SerializeField] BillionareScreenController billionareScreenController;
    void BillionareScreen()
    {
        GO_BillionareScreenRoot.SetActive(true);
        musicControl.TransitionStats(musicControl.musicStats_endQuestion, musicControl.musicStats_Billionare, musicControl.secondsToTransitionToBillionare);
        graphDisplayer.stopGraph = true;
        billionareScreenController.StartBillionareCutscene();
        Debug.Log("Show billionare cutscene");
        postProController.SetPostProcesing(postProController.postPoInfo_GameOverScreens);
        bestTime_EndCounting();
    }

    #endregion
    #region PAUSE MENU
    [Header("Pause Menu")]
    [SerializeField] Canvas Canvas_Pause;
    [SerializeField] Slider slider_volume;
    public float GeneralVolumeMultiplier;
    void onPausePressed()
    {
        gameStates currentState = gameStatesStack.Peek();


        if(currentState == gameStates.PauseMenu) { CloseCurrentState(); return; }

        if(currentState == gameStates.Billionare)
        {
            billionareScreenController.SkipBillionareCutscene();
            return;
        }

        if(currentState == gameStates.MainMenu || currentState == gameStates.Bankrupt || currentState == gameStates.Billionare ) 
        { return; }

        AddNewState(gameStates.PauseMenu);
    }
    public void button_pause_resumeGame()
    {
        CloseCurrentState();
        
    }
    public void button_pause_restartGame()
    {
        musicControl.RestartMusicLoop();
        Button_RestartGame();

    }
    public void button_pause_exitGame()
    {
        Button_ExitGame();
    }
    public void slider_OnAudioValueChanged()
    {
        GeneralVolumeMultiplier = slider_volume.value;
        musicControl.UpdateVolume();
    }
    #endregion
}

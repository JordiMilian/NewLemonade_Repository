using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

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

        showMainMenu();
        restartStats();
    }
    #region MAIN MENU
    public void showMainMenu()
    {
        MainMenuCanvas.SetActive(true);
        postProController.SetPostProcesing(postProController.postPoInfo_MainMenu);
        //MusicLoopDistorder.musicDistortionStats baseStats = new MusicLoopDistorder.musicDistortionStats();
        //baseStats = MusicLoopDistorder.musicDistortionStats.CopyStats(baseStats, musicControl.musicStats_current);
        musicControl.TransitionFromCurrentStats(musicControl.musicStats_MainMenu, musicControl.seconds_TransitionToMainMenu);

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
    public void StartPlayingButton()
    {
        restartStats();
        restartTimer();
        showPurchaseButtons();
        
        pauseAutomising = true;
        
        MainMenuCanvas.SetActive(false);
        musicControl.TransitionStats(musicControl.musicStats_MainMenu, musicControl.musicStats_startQuestion, musicControl.seconds_TransitionToStartPlaying);
        SFX_PlayerSingleton.Instance.playSFX(graphDisplayer.AudioClip_GraphTransition);
        StartCoroutine(StartGameCoroutine());
        postProController.SetPostProcesing(postProController.postPoinfo_startQuestions);
    }
    public void Button_RestartGame()
    {
        GO_BankruptScreenRoot.SetActive(false);
        restartStats();
        restartTimer();
        musicControl.TransitionFromCurrentStats(musicControl.musicStats_startQuestion, 1);
        postProController.SetPostProcesing(postProController.postPoinfo_startQuestions);
        StartCoroutine(StartGameCoroutine());
    }
    [Header("Start Game stuff")]
    [SerializeField] AnimationCurve FieldOfViewCurve;
    [SerializeField] float seconds_startGameTransition;
    [SerializeField] Volume postProcessingVolume;
    [SerializeField] int TutorialPhasesCount;
    [SerializeField] GameObject TutorialCanvas;
    public bool skipTutorial;
    IEnumerator StartGameCoroutine()
    {
        int tutorialPhasesPased = 0;
        TutorialCanvas.SetActive(true);
        yield return StartCoroutine(LensDistortionPopUp());

        graphDisplayer.StartDisplaying();
        if (skipTutorial) 
        { 
            TutorialCanvas.SetActive(false);
            enablePurchaseButtons();
            pauseAutomising = false;
            graphDisplayer.stopGraph = false;
            yield break;
        }
        yield return new WaitForSeconds(1);
        Animator_Tutorial.SetTrigger("nextShadow");
        graphDisplayer.stopGraph = true;
        pauseTimer();
    Waitagain:
        
        while(Input.GetMouseButtonDown(0) == false)
        {
            yield return null;
        }
        Animator_Tutorial.SetTrigger("nextShadow");
        yield return new WaitForSeconds(0.5f);
        tutorialPhasesPased++;
        if(tutorialPhasesPased < TutorialPhasesCount) { goto Waitagain; }

        skipTutorial = true;
        pauseAutomising = false;
        restartTimer();
        graphDisplayer.stopGraph = false;
        enablePurchaseButtons();
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
    public void ExitGame()
    {
        Application.Quit();
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
        if (Input.GetKeyDown(KeyCode.M))
        {
            CurrentMoney = questionsHolder.questions[nextQuestionIndex].MoneyGoal;
            BuyLemons();
        }
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
            if(affordableLemons == 0) { SFX_PlayerSingleton.Instance.playSFX(AudioClip_CantBuy,0.1f); return; }
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
            if(CurrentLemons == 0) { SFX_PlayerSingleton.Instance.playSFX(AudioClip_CantBuy, 0.1f);return; }

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
    [SerializeField] float timeBetweenPurchases = .5f;
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

        hideQuestionUI();
        Dude_Controller.Instance.CallHideDude();
        enablePurchaseButtons();
        
        restartTimer();
        UpdateTextDisplays();

        graphDisplayer.stopGraph = false;
        graphDisplayer.updateGraphHeight();
        graphDisplayer.displayGraph();

        SFX_PlayerSingleton.Instance.playSFX(AudioClip_AnswerPressed);

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
        BankrupedScreen();
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
        StopCoroutine(currentTimer);
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
    void updateMoneyBar(float goal, float currentMoney)
    {
        float normalizedAmount = Mathf.InverseLerp(0, goal, currentMoney);
        moneySlider_size1.localScale = new Vector3(normalizedAmount, 1, 1);
    }
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
    
    void UpdateTextDisplays()
    {
        TMP_moneyDisplay.text = floatToMoneyString(CurrentMoney);
        TMP_Lemons.text = CurrentLemons.ToString();
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

        Animator_QuestionUI.SetTrigger("Appear");
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
        updateMoneyBar(questions[nextQuestionIndex].MoneyGoal, amount);
        if (amount >= questions[nextQuestionIndex].MoneyGoal)
        {
            OnGoalReached();
        }
    }
    void OnGoalReached()
    {
        pauseTimer();
        disablePurchaseButtons();
        if(nextQuestionIndex == questionsHolder.questions.Count -1)
        {
            BillionareScreen();
            graphDisplayer.stopGraph = true;
            StopAllHolds();
            OnMoneyUpdated -= onMoneyUpdated;
            return;
        }

        showQuestionUI();
        graphDisplayer.makeNewPointAndDisplay(true);
        graphDisplayer.stopGraph = true;
        
        StopAllHolds();
        SFX_PlayerSingleton.Instance.playSFX(AudioClip_GoalReached);
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
    }

    #endregion
}

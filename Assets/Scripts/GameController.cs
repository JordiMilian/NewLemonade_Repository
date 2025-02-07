using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class GameController : MonoBehaviour
{
    [SerializeField] float StartingMoney;
    float currentMoney;
    event Action<float> OnMoneyUpdated;
    event Action OnGoalUpdated;
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

    public float BuyingPrice;
    public float SellingPrice;
    [SerializeField] float MoneyTestingAmount;
    [SerializeField] string MoneyTestingString;

    [SerializeField] QuestionsHolder questionsHolder;
    [SerializeField] GraphDisplayer graphDisplayer;
    [HideInInspector] public List<Question> questions;
    [Serializable]
    public class Question
    {
        public string Title;
        public float MoneyGoal;
        public string QuestionText;
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
        questions = questionsHolder.questions;
        enablePurchaseButtons();
        hideQuestionUI();
        restartTimer();
        SetUpMoneyBar();
        CurrentMoney = StartingMoney;

        SetUpTimerBar();

        UpdateTextDisplays();
        
    }
    #region AUTOMATIC SELLING AND BUYING
    
    public float secondsToAutomaticSell;
    float SellingTimer;
    public bool isAutomatisingSell;
    bool pauseAutomising;

    bool isHoldingBuy, isHoldingSell;
    float holdBuyTimer, holdSellTimner;
    private void Update()
    {
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



        if (pauseAutomising) { return; }

       if(isAutomatisingSell)
       {
            SellingTimer += Time.deltaTime;
            if (SellingTimer >= secondsToAutomaticSell)
            {
                automaticPurchase();
                SellingTimer = 0;
            }
       }

        MoneyTestingString = floatToMoneyString(MoneyTestingAmount);
    }
    
    #endregion
    #region BUY AND SELL
    public void BuyLemons()
    {
        int affordableLemons = Mathf.RoundToInt((CurrentMoney / BuyingPrice) - 0.5f);
        if (affordableLemons < LemonsPerBuy)
        {
            CurrentMoney -= affordableLemons * BuyingPrice;
            CurrentLemons += affordableLemons;
        }
        else
        {
            CurrentMoney -= LemonsPerBuy * BuyingPrice;
            CurrentLemons += LemonsPerBuy;
        }
        UpdateTextDisplays();
    }
    public void SellLemons()
    {
        if (CurrentLemons < LemonsPerSell)
        {
            CurrentMoney += CurrentLemons * SellingPrice;
            CurrentLemons -= CurrentLemons;
        }
        else
        {
            CurrentMoney += LemonsPerSell * SellingPrice;
            CurrentLemons -= LemonsPerSell;
        }
        UpdateTextDisplays();
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
        enablePurchaseButtons();
        
        restartTimer();
        UpdateTextDisplays();

        graphDisplayer.stopGraph = false;
        graphDisplayer.updateGraphHeight();
        graphDisplayer.displayGraph();

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
                    isAutomatisingSell = true;
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
            updateTimerBar(GameOverTimer / TimeToReachGoal);
            yield return null;
        }
        //GAME OVER
        disablePurchaseButtons();
        hideQuestionUI();
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
    void updateTimerBar(float normalizedTime)
    {
        TfTimerBar.position = Vector2.Lerp(timerBar_startingPos,timerBar_endPos, normalizedTime);
    }
    [Header("Text Mesh Pros")]
    [SerializeField] TextMeshProUGUI TMP_moneyDisplay;
    [SerializeField] TextMeshProUGUI TMP_Lemons, TMP_Question, TMP_Answer1_Title, TMP_Answer2_Title;
    [SerializeField] TextMeshProUGUI TMP_Answer1_Price, TMP_Answer2_Price, TMP_Answer1_Upgrade, TMP_Answer2_Upgrade;
    [SerializeField] TextMeshProUGUI TMP_Buy_Amount, TMP_Sell_Amount, TMP_Buy_Price, TMP_Sell_Price;
    void UpdateTextDisplays()
    {
        TMP_moneyDisplay.text = floatToMoneyString(CurrentMoney);
        TMP_Lemons.text = CurrentLemons.ToString();
        TMP_Buy_Amount.text = LemonsPerBuy.ToString();
        TMP_Sell_Amount.text = LemonsPerSell.ToString();
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

        if(amount < 10) { string rawNumbers = amount.ToString("F2"); return rawNumbers + " $"; }
        if (amount >= 10 && amount < 100) { string rawNumbers = amount.ToString("F1"); return rawNumbers + " $"; }


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
        return procesedNumber + " $";
    }
    #endregion

    [Header("Buying/Selling Buttons")]
    [SerializeField] GameObject Button_Buy;
    [SerializeField] GameObject Button_Sell;
    [Space(1)]
    [SerializeField] GameObject GO_QuestionsRoot;
    void disablePurchaseButtons()
    {
        Button_Buy.SetActive(false);
        Button_Sell.SetActive(false);
    }
    void enablePurchaseButtons()
    {
        Button_Buy.SetActive(true);
        Button_Sell.SetActive(true);
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

        pauseAutomising = true;
    }
    void hideQuestionUI()
    {
        GO_QuestionsRoot.SetActive(false);

        pauseAutomising = false;
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
        if(nextQuestionIndex == questionsHolder.questions.Count)
        {
            BillionareScreen();
            return;
        }

        showQuestionUI();
        graphDisplayer.makeNewPointAndDisplay(true);
        graphDisplayer.stopGraph = true;

        StopAllHolds();
    }
    void BillionareScreen()
    {

    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalancingTool : MonoBehaviour
{
    [SerializeField] QuestionsHolder questionsHolder;
    [SerializeField] float PriceForFinalQuestion;
    [SerializeField] float startingValue;
    [HideInInspector] public float multiplicativeValue;

    private void Awake()
    {
        SetGoalsPrices();
        SetAnswersPrices();
    }
    void SetGoalsPrices()
    {
        float fakeFinalPrice = PriceForFinalQuestion / startingValue;

        multiplicativeValue = Mathf.Pow(fakeFinalPrice, 1f / questionsHolder.questions.Count);
        float previousValue = startingValue;
        Debug.Log("Multiplicative value = " + multiplicativeValue);
        for (int q = 0; q < questionsHolder.questions.Count; q++)
        {
            questionsHolder.questions[q].MoneyGoal = previousValue * multiplicativeValue;
            previousValue = questionsHolder.questions[q].MoneyGoal;
        }
    }
    void SetAnswersPrices()
    {
        for (int q = 0; q < questionsHolder.questions.Count; q++)
        {
            float GoalPrice = questionsHolder.questions[q].MoneyGoal;
            questionsHolder.questions[q].answer1.price = GoalPrice * (questionsHolder.questions[q].answer1.pricePercentOfGoal / 100);
            questionsHolder.questions[q].answer2.price = GoalPrice * (questionsHolder.questions[q].answer2.pricePercentOfGoal / 100);
        }
    }
    
}

using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;

public class GraphDisplayer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    [SerializeField] GameController gameController;
    [SerializeField] QuestionsHolder questionsHolder;
    [SerializeField] BalancingTool balancingTool;
    [Header("Stats")]
    [SerializeField] float PointsPerSeconds;
    float HeightPerMoney;
    [SerializeField] float MaxHeight;
    public float MaxWidth;
    [SerializeField] Transform startingPosTf;
    [SerializeField] int MaxDisplayPoints = 20;
    [SerializeField] int pointsPerSmooth;
    List<pointInfo> allPoints = new List<pointInfo>();
    public bool stopGraph;
    [Header("Goal bars manager")]
    [SerializeField] GameObject GoalBarPrefab;
    [SerializeField] Color currentBarColor, disabledBarColor;
    [SerializeField] int maxGoalBarsDisplaying = 3;
    List<GoalBar> instantiatedBars = new List<GoalBar>();
    public AudioClip AudioClip_GraphTransition;
    [SerializeField] Transform Tf_lemonIcon;

    public class pointInfo
    {
        public Vector3 currentPos;
        public Vector3 smoothPos;
        public float money;
        public bool isKeyPoint;
    }
   
    float timer;
    float secondsPerPoint;
    public void StartDisplaying()
    {
        secondsPerPoint = 1 / PointsPerSeconds;
        allPoints = new List<pointInfo>();

        pointInfo newPoint = new pointInfo();
        newPoint.money = gameController.CurrentMoney;
        newPoint.currentPos = startingPosTf.position;
        allPoints.Add(newPoint);
        HeightPerMoney = MaxHeight / questionsHolder.questions[0].MoneyGoal;
        Debug.Log("HeightPerMoner: " + HeightPerMoney);
        StartCoroutine(progresiveHeightChange(50f, questionsHolder.questions[0].MoneyGoal));

        stopGraph = false;
    }
    private void Update()
    {
        if (stopGraph) { return; }

        timer += Time.deltaTime;

        if(timer > secondsPerPoint)
        {
            timer = 0;
            makeNewPointAndDisplay();
            Tf_lemonIcon.position = allPoints[allPoints.Count - 1].smoothPos + lineRenderer.transform.position;
        }
    }
    public void makeNewPointAndDisplay(bool isKeyPoint = false)
    {
        Vector3 newPos = new Vector3(
                MaxWidth,
                HeightPerMoney * gameController.CurrentMoney,
                0);

        pointInfo newPoint = new pointInfo();
        newPoint.money = gameController.CurrentMoney;
        newPoint.currentPos = newPos;
        newPoint.isKeyPoint = isKeyPoint;

        allPoints.Add(newPoint);
        if (allPoints.Count > MaxDisplayPoints)
        {
            allPoints.RemoveAt(0);
        }

        displayGraph();
    }
    #region VERTICAL CONTROL
    [SerializeField] float timeToVerticalTransitions;
    public void updateGraphHeight()
    {
        StartCoroutine(progresiveHeightChange(
            questionsHolder.questions[gameController.nextQuestionIndex - 1].MoneyGoal,
            questionsHolder.questions[gameController.nextQuestionIndex].MoneyGoal));
    }
    IEnumerator progresiveHeightChange(float previousValue, float newValue)
    {
        if (gameController.nextQuestionIndex > 0) { SFX_PlayerSingleton.Instance.playSFX(AudioClip_GraphTransition, 0.05f); }
        foreach (GoalBar bar in instantiatedBars)
        {
            bar.Tf_GoalBar.GetComponent<SpriteRenderer>().color = disabledBarColor;
        }
        float timer = 0;
        while (timer < timeToVerticalTransitions)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / timeToVerticalTransitions;
            HeightPerMoney = MaxHeight / Mathf.Lerp(previousValue, newValue, normalizedTime);
            SetGoalBarsHeight();
            yield return null;
        }

        MakeNextGoalBar();
        SetGoalBarsHeight();
    }
    #region Goal bars management
    struct GoalBar
    {
        public float moneyGoal;
        public Transform Tf_GoalBar;
    }
    void MakeNextGoalBar()
    {
        
        GameObject newBar = Instantiate(GoalBarPrefab, Vector3.zero,Quaternion.identity);
        newBar.GetComponent<SpriteRenderer>().color = currentBarColor;
        GoalBar newGoalBarStruct = new GoalBar();
        newGoalBarStruct.moneyGoal = questionsHolder.questions[gameController.nextQuestionIndex].MoneyGoal;
        newGoalBarStruct.Tf_GoalBar = newBar.transform;

        instantiatedBars.Add(newGoalBarStruct);
        if(instantiatedBars.Count > maxGoalBarsDisplaying)
        {
            Destroy(instantiatedBars[0].Tf_GoalBar.gameObject);
            instantiatedBars.RemoveAt(0);
        }

    }
    void SetGoalBarsHeight()
    {
        foreach (GoalBar bar in instantiatedBars)
        {
            bar.Tf_GoalBar.position = new Vector3(
                0,
                lineRenderer.transform.position.y +  HeightPerMoney * bar.moneyGoal,
                0
                );
        }
    }
    #endregion
    #endregion
    public void displayGraph()
    {
        float distancePerPoint = MaxWidth / allPoints.Count;

        for (int p = 0; p < allPoints.Count; p++)
        {
            allPoints[p].currentPos = new Vector3(
                distancePerPoint * p,
                allPoints[p].money * HeightPerMoney,
                0
                ) + startingPosTf.position;
        }
        RenderLineRenderer();

        //
        void RenderLineRenderer()
        {
            lineRenderer.positionCount = allPoints.Count;
            lineRenderer.SetPositions(smoothOutPositions(allPoints.ToArray()));
        }

        Vector3[] smoothOutPositions(pointInfo[] points)
        {
            List<Vector3> smoothedVectors = new List<Vector3>();

            for (int p = 0; p < points.Length; p++)
            {
                int thisPointsPerSmooth = pointsPerSmooth;

                int posiblePoints = points.Length - p;
                if( posiblePoints < pointsPerSmooth )
                {
                    thisPointsPerSmooth = posiblePoints;
                }

                if (p < thisPointsPerSmooth)
                {
                    points[p].smoothPos = points[p].currentPos;
                    smoothedVectors.Add(points[p].smoothPos);
                    continue;
                }
                

                float allAddedHeights = points[p].currentPos.y;
                for (int i = 0; i < thisPointsPerSmooth; i++) 
                {
                    allAddedHeights += points[p - i].currentPos.y;
                    allAddedHeights += points[p + i].currentPos.y;
                }
                float mediaHeight = allAddedHeights / ((thisPointsPerSmooth * 2)+1);
                points[p].smoothPos = new Vector3(points[p].currentPos.x, mediaHeight, 0);
                smoothedVectors.Add(points[p].smoothPos);
            }
            return smoothedVectors.ToArray();
        }
    }

    [Header("Line renderer wide segment")]
    [SerializeField] float wideSegment_Lenght;
    [SerializeField] float wideSegment_maxWidth;
    [SerializeField] float defaultLineWidth;
    [SerializeField] Color wideSegment_Color, defaulLineColor;
    [SerializeField] float colorgradientWidth;

    public void UpdateTimerBar(float normalizedTime)
    {
        AnimationCurve newCurve = new AnimationCurve();
        newCurve.AddKey(normalizedTime, wideSegment_maxWidth);
        newCurve.AddKey(normalizedTime - (wideSegment_Lenght * 1.5f), defaultLineWidth);
        newCurve.AddKey(normalizedTime + (wideSegment_Lenght * 0.5f), defaultLineWidth);
        lineRenderer.widthCurve = newCurve;
        GradientColorKey[] newGradientColors = new GradientColorKey[2];
        newGradientColors[0].color = wideSegment_Color;
        newGradientColors[0].time = normalizedTime;
        newGradientColors[1].color = defaulLineColor;
        newGradientColors[1].time = normalizedTime + colorgradientWidth;
        Gradient newGrad = new Gradient();
        newGrad.SetKeys(newGradientColors, lineRenderer.colorGradient.alphaKeys);

        lineRenderer.colorGradient = newGrad;
    }



}

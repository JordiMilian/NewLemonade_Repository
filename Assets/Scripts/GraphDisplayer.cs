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
    [SerializeField] List<pointInfo> allPoints = new List<pointInfo>();
    public bool stopGraph;

    [Serializable]
    public class pointInfo
    {
        public Vector3 currentPos;
        public Vector3 smoothPos;
        public float money;
        public bool isKeyPoint;
    }
   
    float timer;
    float secondsPerPoint;
    private void Start()
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
    }
    private void Update()
    {
        if (stopGraph) { return; }

        timer += Time.deltaTime;

        if(timer > secondsPerPoint)
        {
            timer = 0;
            makeNewPointAndDisplay();
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
        float timer = 0;
        while (timer < timeToVerticalTransitions)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / timeToVerticalTransitions;
            HeightPerMoney = MaxHeight / Mathf.Lerp(previousValue, newValue, normalizedTime);
            yield return null;
        }
    }
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
                if (points[p].isKeyPoint || p == points.Length - 1 || p == 0)
                {
                    points[p].smoothPos = points[p].currentPos;
                    smoothedVectors.Add(points[p].smoothPos);
                    continue;
                }
                float midHeight = (points[p - 1].currentPos.y + points[p + 1].currentPos.y) / 2;
                float midHeightPlusOwn = (midHeight + points[p].currentPos.y) / 2;
                points[p].smoothPos = new Vector3(points[p].currentPos.x, midHeightPlusOwn, 0);
                smoothedVectors.Add(points[p].smoothPos);
            }
            return smoothedVectors.ToArray();
        }
    }

   
    
    
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Questions Holder", menuName = "Questions Holder")]
public class QuestionsHolder : ScriptableObject
{
    public List<GameController.Question> questions = new List<GameController.Question>();
}

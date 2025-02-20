using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dude_ShowQuestionDude : MonoBehaviour
{
    [SerializeField] GameController gameController;
    [SerializeField] Transform tf_dudeReferencePos;
    public void EV_ShowThisQuestionDude()
    {
        Dude_Controller.Instance.ShowDudeInPosition(
            gameController.questions[gameController.nextQuestionIndex].dudeToAppear,
            tf_dudeReferencePos.position);
    }
    
}

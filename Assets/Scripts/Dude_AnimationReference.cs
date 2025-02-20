using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dude_AnimationReference : MonoBehaviour
{
    [SerializeField] Transform Tf_dudePosition;
   public void EV_ShowDude(Dude_Controller.dudeTypes tipe)
    {
        Dude_Controller.Instance.ShowDudeInPosition(tipe, Tf_dudePosition.position);
    }
    public void EV_HideDude()
    {
        Dude_Controller.Instance.CallHideDude();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuySellButtons_ButtonUp : MonoBehaviour
{
    Animator buttonAnimator;
    private void Awake()
    {
        buttonAnimator = GetComponent<Animator>();
    }
    public void OnButtonDown() { buttonAnimator.SetBool("Pressing", true); buttonAnimator.SetTrigger("Downed"); }
    public void OnButtonUp() { buttonAnimator.SetBool("Pressing", false); buttonAnimator.SetTrigger("Clicked"); }
    public void OnButtonEntered() { buttonAnimator.SetBool("Hovering", true); }
    public void OnButtonExit() { buttonAnimator.SetBool("Hovering", false); }
}

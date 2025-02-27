using System.Collections;
using UnityEngine;

public class BillionareScreenController : MonoBehaviour
{
    [SerializeField] AnimationClip AniClips_billionares;
    [SerializeField] Animator animator_BillionareCanvas;
    [SerializeField] GameController gameController;
    Coroutine currentCutscene;
    public void StartBillionareCutscene()
    {
        currentCutscene = StartCoroutine(billionareCutscene());

    }
    public void SkipBillionareCutscene()
    {
        if(currentCutscene != null) { StopCoroutine(currentCutscene); }
        gameController.closeStatesUntilReachingState(gameStates.MainMenu);
    }
    IEnumerator billionareCutscene()
    {
        animator_BillionareCanvas.SetTrigger("start");

        yield return new WaitForSeconds(AniClips_billionares.length);
        gameController.closeStatesUntilReachingState(gameStates.MainMenu);

    }
}

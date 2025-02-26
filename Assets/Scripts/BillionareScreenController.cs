using System.Collections;
using UnityEngine;

public class BillionareScreenController : MonoBehaviour
{
    [SerializeField] AnimationClip AniClips_billionares;
    [SerializeField] Animator animator_BillionareCanvas;
    [SerializeField] GameController gameController;
    public void StartBillionareCutscene()
    {
        StartCoroutine(billionareCutscene());

    }
    bool canTransition;
    public void EV_CanTransition()
    {
        canTransition = true;
    }
    IEnumerator billionareCutscene()
    {
        animator_BillionareCanvas.SetTrigger("start");

        yield return new WaitForSeconds(AniClips_billionares.length);
        gameController.closeStatesUntilReachingState(gameStates.MainMenu);



        /*
        int passedText = 0;
        passedText++;
        animator_BillionareCanvas.SetInteger("textCount", passedText);
        yield return new WaitForSeconds(AniClips_billionares[passedText - 1].length);

    nextText:
        while (Input.GetMouseButtonDown(0) == false && canTransition == false) { yield return null; }

        canTransition = false;
        passedText++;
        animator_BillionareCanvas.SetInteger("textCount", passedText);
        Debug.Log("Passed billionare texts: "+ passedText);
        yield return new WaitForSeconds(AniClips_billionares[passedText - 1].length);
        if(passedText < AniClips_billionares.Length)
        {
            yield return null;
            goto nextText;
        }
        gameController.showMainMenu();
        gameObject.SetActive(false);
        gameController.skipTutorial = true;
        Debug.Log("LOADING MAIN MENU"); //To do
        */
    }
}

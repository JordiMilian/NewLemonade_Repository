using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dude_Controller : MonoBehaviour
{
    [SerializeField] SpriteRenderer dudeSprite;
    [SerializeField] Transform Tf_DudeRoot;
    [SerializeField] AnimationClip hideClip;
    Animator animator_DudeRoot;
    Coroutine currentCoroutine;
    bool isDudeShowing;
    [SerializeField] List<dudeInfo> list_dudeInfos = new List<dudeInfo>();

    public static Dude_Controller Instance;
    private void Awake()
    {
        Instance = this;
        animator_DudeRoot = Tf_DudeRoot.gameObject.GetComponent<Animator>();
    }
    public enum dudeTypes
    {
        happy, explaining, ups, doubt, upsTongue, laugh, money, serious, cool, sweaty
    }
    [Serializable]
    public struct dudeInfo
    {
        public dudeTypes dudeType;
        public Sprite sprite;
    }
    dudeInfo getInfoWithType(dudeTypes dudeType)
    {
        foreach (dudeInfo info in list_dudeInfos)
        {
            if (info.dudeType == dudeType) { return info; }
        }

        Debug.LogError("Missing dudeInfo in List:" + dudeType);
        return new dudeInfo();
    }

    
    public void ShowDudeInPosition(dudeTypes dudTp, Vector2 pos)
    {
        if (currentCoroutine != null) { StopCoroutine(currentCoroutine); }
        currentCoroutine = StartCoroutine(showDudeCoroutine());
        //
        IEnumerator showDudeCoroutine()
        {
            if (isDudeShowing)
            {
                yield return StartCoroutine(hideDudeCoroutine());
            }

            Tf_DudeRoot.transform.position = pos;
            dudeSprite.sprite = getInfoWithType(dudTp).sprite;
            animator_DudeRoot.SetTrigger("Appear");
            isDudeShowing = true;

        }
    }
    public void CallHideDude()
    {
        if(currentCoroutine !=  null) { StopCoroutine(currentCoroutine); }
        currentCoroutine = StartCoroutine(hideDudeCoroutine());

    }
    IEnumerator hideDudeCoroutine()
    {
        animator_DudeRoot.SetTrigger("Hide");
        isDudeShowing = false;
        yield return new WaitForSeconds(hideClip.length); ;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UI;

public class KillFeed : MonoBehaviour
{
    public Image Background;
    public Image KillStreakBackground;
    public TMP_Text KillStreak;
    public Image SourcePlayerAgentBackground;
    public Image SourcePlayerAgent;
    public TMP_Text SourcePlayerName;
    public Image Gun;
    public GameObject Penetration;
    public GameObject Headshot;
    public TMP_Text TargetPlayerName;
    public Image TargetPlayerBackground;
    public Image TargetPlayerAgent;


    private float targetFade = 1;
    [SerializeField] private float fadeSmoothness;
    [SerializeField] private CanvasGroup group;
    private void Start() => Invoke(nameof(SetFade), 50f);

    private void Update()
    {
        group.alpha = Mathf.Lerp(group.alpha, targetFade, fadeSmoothness);
    }

    void SetFade() => targetFade = 0;
}
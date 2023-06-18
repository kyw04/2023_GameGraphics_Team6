using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChangeManager : MonoBehaviour
{
    public static SceneChangeManager instance;
    public AnimationClip animationClip;
    
    private Animator changeSceneAnimator;

    public AnimationEvent[] animationEvents;

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(instance); }
    }

    private void Start()
    {
        changeSceneAnimator =  GetComponent<Animator>();
        animationEvents = animationClip.events;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name != "Start")
                ChangeScene("Start");
            else
                Application.Quit();
        }
    }

    public void ChangeScene(string name)
    {
        animationEvents[0].stringParameter = name;
        animationClip.events = animationEvents;

        changeSceneAnimator.SetTrigger("doChange");
    }

    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class SplashTransitionScript : MonoBehaviour {

    public Image mWhiteOverlay;

	// Use this for initialization
	void Start () 
    {
        DOTween.Init();
        Sequence mySequence = DOTween.Sequence();
        // Add a movement tween at the beginning
        mySequence.Append(mWhiteOverlay.DOFade(0f, 1f))
            .AppendInterval(3f)
            .Append(mWhiteOverlay.DOFade(1f, 1f))
            .Append(mWhiteOverlay.DOColor(Color.black, 1f)).OnComplete(LoadGame);
	}

    void LoadGame()
    {
        SceneManager.LoadScene(1);
    }
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Timer : MonoBehaviour {

    public Text timer;
    public Text score;

    float scoreValue;
    float timeCountdown;
    float currentTime;
    Color initialColour = Color.white;
    Color warningColour = Color.red;
    bool scored;

    void Awake()
    {
        timer = timer.GetComponent<Text>();
        score = score.GetComponent<Text>();
        timeCountdown = 5.0f;
        currentTime = timeCountdown;
        scoreValue = 0;
    }
	
	void FixedUpdate () {
        scored = false;
	    if(!scored)
        {
            currentTime -= Time.deltaTime;
            if(currentTime <= 0)
            {
                currentTime = timeCountdown;
                if (scoreValue <= 0)
                {
                    scoreValue = 0;
                }
                else
                {
                    scoreValue -= 5.0f;
                }
            }
        }
        if(Input.GetButtonDown("Fire1"))
        {
            Scoring();
        }
        timer.text = "" + currentTime.ToString("f0");
        score.text = "" + scoreValue.ToString("f0");
	}

    void Scoring()
    {
        scored = true;
        scoreValue += currentTime;
        currentTime = timeCountdown;
    }
}

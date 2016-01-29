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
        timeCountdown = 5f;
        scoreValue = 0;
    }
	
	void FixedUpdate () {
        scored = false;
	    if(!scored)
        {
            timeCountdown -= Time.deltaTime;
            currentTime = timeCountdown;
            if(timeCountdown <= 0)
            {
                timeCountdown = 5f;
                scoreValue -= 5f;
            }
        }
        if(Input.GetButtonDown("Fire1"))
        {
            Scoring();
        }
        timer.text = "Remaining time : " + timeCountdown.ToString("f0");
        score.text = "Score : " + scoreValue.ToString("f0");
	}

    void Scoring()
    {
        scored = true;
        scoreValue += currentTime;
        timeCountdown = 5f;
    }
}

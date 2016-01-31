using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
	public static HUD instance;
	public Text timer;
	public Text score;
    public Text round;
    public Text currentNumber;

	void Awake()
	{
		instance = this;
		timer = timer.GetComponent<Text>();
	    score = score.GetComponent<Text>();
	}

	public void SetTimer (string val)
	{
		timer.text = val;
	}

	public void SetScore (string val)
	{
		score.text = val;
	}

	public void UpdateTimer (float val)
	{
		timer.text = "" + val.ToString("f0");
	}

	public void UpdateScore (float val)
	{
		score.text = "" + val.ToString("f0");
	}

    public void UpdateRound (float val)
    {
        round.text = "" + val.ToString("f0");
    }

    public void UpdateCurrentNumber (float val)
    {
        currentNumber.text = "" + val.ToString("f0");
    }
}

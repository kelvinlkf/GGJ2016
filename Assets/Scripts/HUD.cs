using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
	public static HUD instance;
	public Text timer;
	public Text score;

	void Awake()
	{
		instance = this;
		timer = timer.GetComponent<Text>();
	    score = score.GetComponent<Text>();
	}

	public void UpdateTimer (float val)
	{
		timer.text = "" + val.ToString("f0");
	}

	public void UpdateScore (float val)
	{
		score.text = "" + val.ToString("f0");
	}
}

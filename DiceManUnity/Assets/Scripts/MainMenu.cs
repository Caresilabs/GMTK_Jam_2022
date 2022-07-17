using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public AudioClip StartSound;

    // Start is called before the first frame update
    void Start()
    {

        var timeText = GameObject.Find("TimeText");
        if (timeText)
        {
            float totalTime = 0f;
            for (int i = 0; i < GlobalManager.LevelTimes.Length; i++)
            {
                totalTime += GlobalManager.LevelTimes[i];
            }

            if (totalTime > 0)
            {
                timeText.GetComponent<TextMeshProUGUI>().text = $"Full game record:\n{Mathf.Round(totalTime)} sec";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GlobalManager.PlaySFX(StartSound);
            GlobalManager.Instance.NextLevel();
        }
    }
}

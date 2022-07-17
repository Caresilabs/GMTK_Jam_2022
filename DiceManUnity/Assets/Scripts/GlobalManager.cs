using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalManager : MonoBehaviour
{
    private static GlobalManager _instance;

    public static GlobalManager Instance { get { return _instance; } }

    private int currentLevel = 0;




    private const int maxLevel = 3;

    public static float[] LevelTimes = new float[maxLevel];


    public RenderTexture GameTexture;


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            _instance = this;

            GameTexture.height = (int)((Screen.height / (float)Screen.width ) * GameTexture.width);
        }
    }


    public static void PlaySFX(AudioClip clip)
    {
        if (clip)
            Instance.transform.GetChild(1).GetComponent<AudioSource>().PlayOneShot(clip);
    }
   


    public void MainMenu()
    {
        currentLevel = 0;
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void NextLevel()
    {
        if (currentLevel == 0)
        {
            // Reset times
            for (int i = 0; i < maxLevel; i++)
            {
                LevelTimes[i] = 0;
            }
        }


        currentLevel++;

        if (currentLevel > maxLevel)
        {
            MainMenu();
            return;
        }

        SceneManager.LoadSceneAsync("Level"+ currentLevel);
    }

    public void SetTime(float time)
    {
        if (currentLevel > 0)
            LevelTimes[currentLevel - 1] = time;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}

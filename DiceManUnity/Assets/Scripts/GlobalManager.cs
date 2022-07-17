using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalManager : MonoBehaviour
{
    private static GlobalManager _instance;

    public static GlobalManager Instance { get { return _instance; } }

    private int currentLevel = 0;

    private readonly int maxLevel = 1;



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
        }
    }


    public void MainMenu()
    {
        currentLevel = 0;
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void NextLevel()
    {
        currentLevel++;

        if (currentLevel > maxLevel)
        {
            MainMenu();
            return;
        }

        SceneManager.LoadSceneAsync("Level"+ currentLevel);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

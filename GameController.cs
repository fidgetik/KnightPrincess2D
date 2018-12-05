using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public enum GameState
{
    Play,
    Pause
}

public delegate void UpdateHeroParametersHandler(HeroParameters parameters);

public delegate void InventoryUsedCallback(InventoryUIButton uiButton);

public class GameController : MonoBehaviour
{
    public event UpdateHeroParametersHandler OnUpdateHeroParameters;

    [SerializeField] private Audio _audioManager;
    [SerializeField] private List<InventoryItem> _inventory;
    [SerializeField] private HeroParameters _hero;

    private GameState _state;
    private int _dragonKillExperience;
    private int _score;
    private int _dragonHitScore = 10;
    private int _dragonKillScore = 50;
    private Knight _knight;
    private static GameController _instance;

    #region Properties

    public Knight _Knight
    {
        get { return _knight; }
        set { _knight = value; }
    }

    public static GameController Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gameController = Instantiate(Resources.Load("Prefabs/GameController")) as GameObject;
                _instance = gameController.GetComponent<GameController>();
            }

            return _instance;
        }
    }

    public GameState State
    {
        get { return _state; }
        set
        {
            if (value == GameState.Pause)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }

            _state = value;
        }
    }


    public HeroParameters Hero
    {
        get { return _hero; }
        set { _hero = value; }
    }

    public int Score
    {
        get { return _score; }
        set
        {
            _score = value;
            HUD.Instance.SetScore(_score.ToString());
        }
    }

    public Audio AudioManager
    {
        get { return _audioManager; }
        set { _audioManager = value; }
    }

    public List<InventoryItem> Inventory
    {
        get { return _inventory; }
        set { _inventory = value; }
    }

    #endregion

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        _knight = FindObjectOfType<Knight>();
        _inventory = new List<InventoryItem>();
        DontDestroyOnLoad(gameObject);
        InitializedAudioManager();
    }

    public void StartNewLevel()
    {
        HUD.Instance.SetScore(Score.ToString());
        if (OnUpdateHeroParameters != null)
        {
            OnUpdateHeroParameters(_hero);
        }

        State = GameState.Play;
        AudioManager.PlayMusic();
    }

    public void Hit(IDestructable victim)
    {
        if (victim.GetType() == typeof(Dragon))
        {
            AudioManager.PlaySoundRandomPitch("DM-CGS-22");
            Score += _dragonHitScore;
        }

        if (victim.GetType() == typeof(Knight))
        {
            AudioManager.PlaySoundRandomPitch("DM-CGS-02");
            HUD.Instance.HealthBar.value = victim.Health;
        }
    }

    public void Killed(IDestructable victim)
    {
        if (victim.GetType() == typeof(Dragon))
        {
            Score += _dragonKillScore;
            Princess.DragonCount--;
            _dragonKillExperience = Random.Range(1, 3);
            _hero.Experience += _dragonKillExperience;
            Destroy((victim as MonoBehaviour).gameObject);
        }

        if (victim.GetType() == typeof(Knight))
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        HUD.Instance.ShowLevelLoseWindow();
    }

    public void AddEnviromentItem(InventoryItem itemData)
    {
        InventoryUIButton newUiButton = HUD.Instance.AddNewInventoryItem(itemData);
        InventoryUsedCallback callback = new InventoryUsedCallback(InventoryItemUsed);
        newUiButton.Callback = callback;
        newUiButton.ItemData = itemData;
        _inventory.Add(itemData);
        AudioManager.PlaySoundRandomPitch("DM-CGS-32");
    }

    public void InventoryItemUsed(InventoryUIButton item)
    {
        switch (item.ItemData.CrystalType)
        {
            case CrystallType.Blue:
                _hero.Speed += item.ItemData.Quantity / 10f;
                break;
            case CrystallType.Red:
                _hero.Damage += item.ItemData.Quantity / 10f;
                break;
            case CrystallType.Green:
                _hero.MaxHealth += item.ItemData.Quantity / 10f;
                break;
            default:
                Debug.LogError("Wrong crystal type!");
                break;
        }

        _inventory.Remove(item.ItemData);
        Destroy(item.gameObject);
        if (OnUpdateHeroParameters != null)
        {
            OnUpdateHeroParameters(_hero);
        }

        AudioManager.PlaySoundRandomPitch("DM-CGS-28");
    }

    public void LoadNextLevel()
    {
        ButtonSound();
        SceneManager.LoadScene(2, LoadSceneMode.Single);
        State = GameState.Play;
    }

    public void RestartLevel()
    {
        ButtonSound();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        State = GameState.Play;
    }

    public void LoadMainMenu()
    {
        ButtonSound();
        Score = 0;
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void PrincessFound()
    {
        AudioManager.PlaySoundRandomPitch("DM-CGS-45");
        HUD.Instance.ShowLevelWonWindow();
    }

    public void InGameMenu()
    {
        ButtonSound();
        HUD.Instance.ShowInGameWindow();
    }

    public void GameComplete()
    {
        AudioManager.PlaySoundRandomPitch("DM-CGS-45");
        HUD.Instance.ShowGameCompleteWindow();
    }

    public void LevelUp()
    {
        if (OnUpdateHeroParameters != null)
        {
            AudioManager.PlaySoundRandomPitch("DM-CGS-15");
            OnUpdateHeroParameters(_hero);
        }
    }

    private void InitializedAudioManager()
    {
        _audioManager.SourceMusic = gameObject.AddComponent<AudioSource>();
        _audioManager.SourceSFX = gameObject.AddComponent<AudioSource>();
        _audioManager.SourceRandomPitchSFX = gameObject.AddComponent<AudioSource>();
        gameObject.AddComponent<AudioListener>();
    }

    private void ButtonSound()
    {
        AudioManager.PlaySoundRandomPitch("DM-CGS-20");
    }
}
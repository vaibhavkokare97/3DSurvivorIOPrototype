using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("COINS")]
    [SerializeField] private TextMeshProUGUI _coinText;

    [Header("GAME OVER")]
    [SerializeField] private RectTransform _gameOverPanel;
    [SerializeField] private Button _gameRestartButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        GameManager.Instance.onCoinCountChange += ChangeCoinUIText;
        GameManager.Instance.OnHealthChange += ChangeHealthBar;

        _gameRestartButton.onClick.AddListener(() => RestartGame());
    }

    private void RestartGame()
    {
        SceneManager.LoadScene("Game");
    }

    void Start()
    {

    }

    void ChangeCoinUIText(int coinsValue)
    {
        _coinText.text = $"Coins: {coinsValue}";
    }

    void ChangeHealthBar(float healthValue)
    {
        _gameOverPanel.gameObject.SetActive(healthValue <= 0);
    }

    private void OnDisable()
    {
        GameManager.Instance.onCoinCountChange -= ChangeCoinUIText;
        GameManager.Instance.OnHealthChange -= ChangeHealthBar;
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}

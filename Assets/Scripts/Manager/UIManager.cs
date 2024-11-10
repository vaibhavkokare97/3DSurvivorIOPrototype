using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _coinText;

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

    }

    private void OnDisable()
    {
        GameManager.Instance.onCoinCountChange -= ChangeCoinUIText;
        GameManager.Instance.OnHealthChange -= ChangeHealthBar;
        Instance = null;
    }
}

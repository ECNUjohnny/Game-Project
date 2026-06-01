using TMPro;
using UnityEngine;

[RequireComponent(typeof(PlayerShooter))]
public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Player Assets")]

    public int gold = 100;

    public int[] ammo;

    private PlayerShooter playerShooter;

    [Header("UI refs")]

    public TextMeshProUGUI goldText;

    public TextMeshProUGUI reserveAmmoText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        playerShooter = GetComponent<PlayerShooter>();
    }

    public void UpdateUI()
    {
        if (goldText != null) goldText.text = $"money: {gold}";
        
    }
}

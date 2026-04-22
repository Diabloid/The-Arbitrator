using UnityEngine;
using TMPro;
using System.Collections;

public class HealGemsUI : MonoBehaviour
{
    [Header("Зв'язки")]
    [SerializeField] private PlayerStats stats;
    [SerializeField] private TextMeshProUGUI HealGemText;

    private Coroutine _warningCoroutine;

    private void OnEnable()
    {
        if (stats != null)
        {
            stats.OnHealGemsChanged += UpdateUI;
            stats.OnHealGemFull += ShowFullWarning;
            UpdateUI();
        }
    }

    private void OnDisable()
    {
        if (stats != null)
        {
            stats.OnHealGemsChanged -= UpdateUI;
            stats.OnHealGemFull -= ShowFullWarning;
        }
    }

    private void UpdateUI()
    {
        if (_warningCoroutine != null) return;

        HealGemText.text = stats.currentHealGems.ToString();
        if (stats.currentHealGems >= stats.maxHealGems)
        {
            HealGemText.color = Color.yellow;
        }
        else
        {
            HealGemText.color = Color.white;
        }
    }

    private void ShowFullWarning()
    {
        if (_warningCoroutine != null) StopCoroutine(_warningCoroutine);
        _warningCoroutine = StartCoroutine(WarningRoutine());
    }

    private IEnumerator WarningRoutine()
    {
        HealGemText.text = "FULL";
        HealGemText.color = Color.red;

        yield return new WaitForSeconds(1.0f);

        _warningCoroutine = null;
        UpdateUI();
    }
}
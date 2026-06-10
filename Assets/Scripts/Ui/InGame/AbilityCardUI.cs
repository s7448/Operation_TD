using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityCardUI : MonoBehaviour
{
    [Header("Header Row")]
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Description")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Level Bars")]
    [SerializeField] private Image[] levelBars; 

    [Header("Button")]
    [SerializeField] private Button actionButton;
    [SerializeField] private Image buttonBackground;
    [SerializeField] private TextMeshProUGUI buttonText;

    private static readonly Color ColorActive = Color.black;
    private static readonly Color ColorInactive = new Color(0.898f, 0.906f, 0.922f);
    private static readonly Color ColorDisabledBg = new Color(0.95f, 0.96f, 0.96f);
    private static readonly Color ColorDisabledText = new Color(0.61f, 0.64f, 0.69f);

    private AbilityType abilityType;

    public void Setup(AbilityType type)
    {
        abilityType = type;
        if (AbilityManager.Instance == null) return;

        AbilityData data = AbilityManager.Instance.GetData(type);

        if (abilityNameText != null && data != null)
            abilityNameText.text = data.abilityName.ToUpper();
        if (descriptionText != null && data != null)
            descriptionText.text = data.description;

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnButtonClicked);
            Debug.Log($"{type} — listener added, button interactable: {actionButton.interactable}");
        }
        else
        {
            Debug.LogError($"{type} — actionButton is NULL!");
        }

        Refresh();
    }

    public void Refresh()
    {
        if (AbilityManager.Instance == null) return;

        if (abilityType == AbilityType.Radar)
        {
            RefreshRadar();
            return;
        }

        AbilityData data = AbilityManager.Instance.GetData(abilityType);
        if (data == null) return;

        bool isUnlocked = AbilityManager.Instance.IsUnlocked(abilityType);
        bool canUnlock = AbilityManager.Instance.CanUnlock(abilityType);
        bool canUpgrade = AbilityManager.Instance.CanUpgrade(abilityType);
        int level = AbilityManager.Instance.GetUpgradeLevel(abilityType);

        if (levelText != null)
            levelText.text = $"{level}/3";

        if (levelBars != null)
        {
            for (int i = 0; i < levelBars.Length; i++)
            {
                if (levelBars[i] == null) continue;
                levelBars[i].color = i < level ? ColorActive : ColorInactive;
            }
        }


        if (!isUnlocked)
        {
            buttonText.text = $"UNLOCK ({data.unlockCost}$)";
            SetButtonState(canUnlock);
        }
        else if (abilityType == AbilityType.Radar)
        {
            buttonText.text = "UNLOCKED";
            SetButtonState(false);
        }
        else if (level >= data.upgradeTiers.Length)
        {
            buttonText.text = "MAX LEVEL";
            SetButtonState(false);
        }
        else
        {
            int cost = data.upgradeTiers[level].cost;
            buttonText.text = $"UPGRADE ({cost}$)";
            SetButtonState(canUpgrade);
        }
    }

    private void RefreshRadar()
    {
        bool isUnlocked = AbilityManager.Instance.IsUnlocked(AbilityType.Radar);
        bool canUnlock = AbilityManager.Instance.CanUnlock(AbilityType.Radar);
        bool canPlace = AbilityManager.Instance.CanPlaceRadar();
        int cooldown = AbilityManager.Instance.GetCooldownRemaining(AbilityType.Radar);

        if (levelText != null)
            levelText.text = isUnlocked ? "UNLOCKED" : "";

        if (actionButton != null)
        {
            if (!isUnlocked)
            {
                buttonText.text = $"UNLOCK (500$)";
                SetButtonState(canUnlock);
            }
            else if (cooldown > 0)
            {
                buttonText.text = $"COOLDOWN ({cooldown} ROUNDS)";
                SetButtonState(false);
            }
            else if (!canPlace)
            {
                buttonText.text = "ALREADY PLACED";
                SetButtonState(false);
            }
            else
            {
                buttonText.text = "READY TO PLACE";
                SetButtonState(false);
            }
        }
    }

    private void SetButtonState(bool interactable)
    {
        Debug.Log($"{abilityType} SetButtonState: {interactable}, actionButton null: {actionButton == null}");
        if (actionButton == null) return;
        actionButton.interactable = interactable;
        if (buttonBackground) buttonBackground.color = interactable ? Color.white : ColorDisabledBg;
        if (buttonText) buttonText.color = interactable ? Color.black : ColorDisabledText;
    }

    private void OnButtonClicked()
    {
        Debug.Log($"OnButtonClicked — type: {abilityType}, isUnlocked: {AbilityManager.Instance.IsUnlocked(abilityType)}, CanUnlock: {AbilityManager.Instance.CanUnlock(abilityType)}");

        if (!AbilityManager.Instance.IsUnlocked(abilityType))
            AbilityManager.Instance.UnlockAbility(abilityType);
        else
            AbilityManager.Instance.UpgradeAbility(abilityType);
    }
}
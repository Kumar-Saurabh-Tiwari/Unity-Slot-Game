using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class SlotMachineGame : MonoBehaviour
{
    private static readonly string[] SymbolResourceNames =
    {
        "slot-symbol2",
        "slot-symbol3",
        "slot-symbol4",
        "slot-symbol1"
    };

    private static readonly int[] BetOptions = { 5, 10, 25 };

    private readonly SlotReel[] _reels = new SlotReel[3];
    private readonly System.Random _random = new System.Random();

    private Sprite[] _symbolSprites;
    private Text _balanceText;
    private Text _betText;
    private Text _winText;
    private Text _statusText;
    private Text _spinButtonText;
    private Button _spinButton;
    private int _balance = 500;
    private int _betOptionIndex = 1;
    private bool _spinInProgress;

    private void Start()
    {
        Application.targetFrameRate = 60;
        LoadSprites();
        BuildInterface();
        RefreshLabels();
        SetStatus("MATCH ALL 3 SYMBOLS TO WIN");
    }

    private void LoadSprites()
    {
        _symbolSprites = new Sprite[SymbolResourceNames.Length];
        for (int i = 0; i < SymbolResourceNames.Length; i++)
        {
            _symbolSprites[i] = LoadSprite("SlotMachine/" + SymbolResourceNames[i]);
        }
    }

    private void BuildInterface()
    {
        GameObject canvasObject = new GameObject("Slot Machine Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        EnsureEventSystem();
        CreateImage(canvas.transform, "Neon Background", LoadSprite("SlotMachine/bg_gradient"),
            Vector2.zero, new Vector2(1280f, 720f), true);
        CreateImage(canvas.transform, "Jackpot Cabinet", LoadSprite("SlotMachine/slot-machine1"),
            Vector2.zero, new Vector2(760f, 589f), true);

        float[] reelPositions = { -116f, 4f, 124f };
        for (int i = 0; i < _reels.Length; i++)
        {
            GameObject reelObject = new GameObject("Reel " + (i + 1), typeof(RectTransform));
            _reels[i] = reelObject.AddComponent<SlotReel>();
            _reels[i].Initialize(canvas.transform, new Vector2(reelPositions[i], -34f),
                new Vector2(104f, 198f), 48f, 58f, _symbolSprites, _random);
        }

        _balanceText = CreateText(canvas.transform, "Balance", "BALANCE  500", 24,
            new Vector2(-185f, -210f), new Vector2(310f, 42f), new Color(1f, 0.84f, 0.3f));
        _betText = CreateText(canvas.transform, "Bet", "BET  10", 22,
            new Vector2(0f, -210f), new Vector2(200f, 42f), Color.white);
        _winText = CreateText(canvas.transform, "Win", "WIN  0", 22,
            new Vector2(185f, -210f), new Vector2(280f, 42f), new Color(0.4f, 1f, 0.72f));
        _statusText = CreateText(canvas.transform, "Status", string.Empty, 22,
            new Vector2(0f, -254f), new Vector2(700f, 40f), Color.white);

        CreateButton(canvas.transform, "Bet Down", "-", new Vector2(-260f, -310f),
            new Vector2(64f, 54f), new Color(0.2f, 0.2f, 0.46f), ChangeBetDown);
        CreateButton(canvas.transform, "Bet Up", "+", new Vector2(-174f, -310f),
            new Vector2(64f, 54f), new Color(0.2f, 0.2f, 0.46f), ChangeBetUp);
        _spinButton = CreateButton(canvas.transform, "Spin", "SPIN", new Vector2(0f, -310f),
            new Vector2(230f, 62f), new Color(0.91f, 0.2f, 0.12f), Spin);
        _spinButtonText = _spinButton.GetComponentInChildren<Text>();
        CreateButton(canvas.transform, "Add Credits", "+500", new Vector2(245f, -310f),
            new Vector2(120f, 54f), new Color(0.16f, 0.44f, 0.33f), AddCredits);

        CreateText(canvas.transform, "Payout Guide", "CHERRY 3X     LEMON 2X     BELL 5X     7 10X",
            16, new Vector2(0f, -354f), new Vector2(700f, 28f), new Color(0.86f, 0.83f, 1f));
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("Event System", typeof(EventSystem),
            typeof(StandaloneInputModule));
        eventSystemObject.GetComponent<StandaloneInputModule>().forceModuleActive = true;
    }

    private Sprite LoadSprite(string resourcePath)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            throw new InvalidOperationException("Could not load required artwork: " + resourcePath);
        }

        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), 100f);
    }

    private Image CreateImage(Transform parent, string objectName, Sprite sprite,
        Vector2 position, Vector2 size, bool preserveAspect)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = preserveAspect;
        return image;
    }

    private Text CreateText(Transform parent, string objectName, string value, int fontSize,
        Vector2 position, Vector2 size, Color color)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.text = value;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private Button CreateButton(Transform parent, string objectName, string label,
        Vector2 position, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = color;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        Text buttonLabel = CreateText(buttonObject.transform, "Label", label, 22,
            Vector2.zero, size, Color.white);
        buttonLabel.raycastTarget = false;
        return button;
    }

    private void Spin()
    {
        if (_spinInProgress)
        {
            return;
        }

        int bet = BetOptions[_betOptionIndex];
        if (_balance < bet)
        {
            SetStatus("NOT ENOUGH CREDITS - PRESS +500");
            return;
        }

        _balance -= bet;
        _spinInProgress = true;
        _spinButton.interactable = false;
        _spinButtonText.text = "SPINNING";
        _winText.text = "WIN  0";

        int[] outcome = new int[_reels.Length];
        for (int i = 0; i < outcome.Length; i++)
        {
            outcome[i] = _random.Next(_symbolSprites.Length);
        }

        SetStatus("GOOD LUCK!");
        RefreshLabels();
        StartCoroutine(CompleteSpin(outcome, bet));
    }

    private IEnumerator CompleteSpin(int[] outcome, int bet)
    {
        for (int i = 0; i < _reels.Length; i++)
        {
            _reels[i].SpinTo(outcome[i], i * 0.28f, 1.85f, _random);
        }

        while (_reels[0].IsSpinning || _reels[1].IsSpinning || _reels[2].IsSpinning)
        {
            yield return null;
        }

        if (outcome[0] == outcome[1] && outcome[1] == outcome[2])
        {
            int multiplier = GetPayoutMultiplier(outcome[0]);
            int winnings = bet * multiplier;
            _balance += winnings;
            _winText.text = "WIN  " + winnings;
            SetStatus(multiplier == 10 ? "JACKPOT!  10X PAYOUT!" : "THREE OF A KIND!  " + multiplier + "X PAYOUT");
        }
        else
        {
            SetStatus("NO MATCH - TRY AGAIN");
        }

        _spinInProgress = false;
        _spinButton.interactable = true;
        _spinButtonText.text = "SPIN";
        RefreshLabels();
    }

    private int GetPayoutMultiplier(int symbolIndex)
    {
        switch (symbolIndex)
        {
            case 0: return 3;
            case 1: return 2;
            case 2: return 5;
            case 3: return 10;
            default: throw new ArgumentOutOfRangeException("symbolIndex");
        }
    }

    private void ChangeBetDown()
    {
        if (_spinInProgress)
        {
            return;
        }

        _betOptionIndex = (_betOptionIndex + BetOptions.Length - 1) % BetOptions.Length;
        RefreshLabels();
    }

    private void ChangeBetUp()
    {
        if (_spinInProgress)
        {
            return;
        }

        _betOptionIndex = (_betOptionIndex + 1) % BetOptions.Length;
        RefreshLabels();
    }

    private void AddCredits()
    {
        if (_spinInProgress)
        {
            return;
        }

        _balance += 500;
        RefreshLabels();
        SetStatus("500 CREDITS ADDED");
    }

    private void RefreshLabels()
    {
        if (_balanceText != null)
        {
            _balanceText.text = "BALANCE  " + _balance;
            _betText.text = "BET  " + BetOptions[_betOptionIndex];
        }
    }

    private void SetStatus(string message)
    {
        if (_statusText != null)
        {
            _statusText.text = message;
        }
    }
}

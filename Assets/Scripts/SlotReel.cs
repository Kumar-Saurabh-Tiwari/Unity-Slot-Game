using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class SlotReel : MonoBehaviour
{
    private readonly Image[] _symbols = new Image[3];
    private Sprite[] _availableSymbols;
    private System.Random _random;
    private float _rowSpacing;

    public bool IsSpinning { get; private set; }

    public void Initialize(Transform parent, Vector2 position, Vector2 viewportSize,
        float symbolSize, float rowSpacing, Sprite[] symbols, System.Random random)
    {
        _availableSymbols = symbols;
        _random = random;

        RectTransform viewport = GetComponent<RectTransform>();
        viewport.SetParent(parent, false);
        viewport.anchorMin = new Vector2(0.5f, 0.5f);
        viewport.anchorMax = new Vector2(0.5f, 0.5f);
        viewport.anchoredPosition = position;
        viewport.sizeDelta = viewportSize;

        Image viewportImage = gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0.38f, 0.68f, 0.76f, 0.14f);
        Mask mask = gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        _rowSpacing = rowSpacing;
        for (int i = 0; i < _symbols.Length; i++)
        {
            GameObject symbolObject = new GameObject("Symbol " + (i + 1),
                typeof(RectTransform), typeof(Image));
            symbolObject.transform.SetParent(transform, false);

            RectTransform symbolRect = symbolObject.GetComponent<RectTransform>();
            symbolRect.anchorMin = new Vector2(0.5f, 0.5f);
            symbolRect.anchorMax = new Vector2(0.5f, 0.5f);
            symbolRect.anchoredPosition = new Vector2(0f, (1 - i) * _rowSpacing);
            symbolRect.sizeDelta = new Vector2(symbolSize, symbolSize);

            _symbols[i] = symbolObject.GetComponent<Image>();
            _symbols[i].preserveAspect = true;
            _symbols[i].sprite = _availableSymbols[_random.Next(_availableSymbols.Length)];
        }
    }

    public void SpinTo(int resultIndex, float startDelay, float duration, System.Random random)
    {
        StartCoroutine(SpinRoutine(resultIndex, startDelay, duration, random));
    }

    private IEnumerator SpinRoutine(int resultIndex, float startDelay, float duration, System.Random random)
    {
        IsSpinning = true;
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        float elapsed = 0f;
        float recycleThreshold = _rowSpacing * 1.5f;
        float recycleDistance = _rowSpacing * 3f;

        while (elapsed < duration)
        {
            float deltaTime = Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float speed = Mathf.Lerp(180f, 650f, Mathf.Sin(progress * Mathf.PI));
            float movement = speed * deltaTime;

            for (int i = 0; i < _symbols.Length; i++)
            {
                RectTransform symbolRect = (RectTransform)_symbols[i].transform;
                symbolRect.anchoredPosition += Vector2.up * movement;
                if (symbolRect.anchoredPosition.y > recycleThreshold)
                {
                    symbolRect.anchoredPosition -= Vector2.up * recycleDistance;
                    _symbols[i].sprite = _availableSymbols[random.Next(_availableSymbols.Length)];
                }
            }

            elapsed += deltaTime;
            yield return null;
        }

        yield return StartCoroutine(Settle(resultIndex));
        IsSpinning = false;
    }

    private IEnumerator Settle(int resultIndex)
    {
        Image[] orderedSymbols = new Image[_symbols.Length];
        Array.Copy(_symbols, orderedSymbols, _symbols.Length);
        Array.Sort(orderedSymbols, delegate(Image first, Image second)
        {
            return second.rectTransform.anchoredPosition.y.CompareTo(first.rectTransform.anchoredPosition.y);
        });

        orderedSymbols[1].sprite = _availableSymbols[resultIndex];
        orderedSymbols[0].sprite = _availableSymbols[_random.Next(_availableSymbols.Length)];
        orderedSymbols[2].sprite = _availableSymbols[_random.Next(_availableSymbols.Length)];

        Vector2[] startingPositions = new Vector2[orderedSymbols.Length];
        Vector2[] targetPositions =
        {
            new Vector2(0f, _rowSpacing),
            Vector2.zero,
            new Vector2(0f, -_rowSpacing)
        };
        for (int i = 0; i < orderedSymbols.Length; i++)
        {
            startingPositions[i] = orderedSymbols[i].rectTransform.anchoredPosition;
        }

        const float settleDuration = 0.24f;
        float elapsed = 0f;
        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / settleDuration));
            for (int i = 0; i < orderedSymbols.Length; i++)
            {
                orderedSymbols[i].rectTransform.anchoredPosition =
                    Vector2.Lerp(startingPositions[i], targetPositions[i], progress);
            }

            yield return null;
        }

        for (int i = 0; i < orderedSymbols.Length; i++)
        {
            orderedSymbols[i].rectTransform.anchoredPosition = targetPositions[i];
        }
    }
}

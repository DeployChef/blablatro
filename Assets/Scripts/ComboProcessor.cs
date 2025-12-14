using CardScripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum HandType
{
    None,
    HighCard,
    Pair,
    TwoPair,
    ThreeOfAKind,
    Straight,
    Flush,
    FullHouse,
    FourOfAKind,
    StraightFlush,
    FiveOfAKind // если у тебя есть джокеры/повторы
}

public class ComboProcessor : MonoBehaviour
{
    private string _comboName;
    [SerializeField] private TextMeshProUGUI comboText;

    [Header("Letter pop animation")]
    [SerializeField] float popScale = 1.3f;      // во сколько раз увеличивается буква
    [SerializeField] float popDuration = 0.06f;  // длительность одного «попа»
    [SerializeField] float delayBetweenChars = 0.005f; // задержка между буквами

    Coroutine _animateRoutine;

    [SerializeField] private ScoreCalculator currentScore;

    public void SelectedChanged(List<Card> selectedCards)
    {
        if (selectedCards == null || selectedCards.Count == 0)
        {
            UpdateComboName("");
            currentScore.SetBasicScore(0, 0);
            return;
        }

        var handType = EvaluateHand(selectedCards);

        switch (handType)
        {
            case HandType.HighCard:
                UpdateComboName("High Card");
                currentScore.SetBasicScore(5, 1);
                break;

            case HandType.Pair:
                UpdateComboName("Pair");
                currentScore.SetBasicScore(10, 2);
                break;

            case HandType.TwoPair:
                UpdateComboName("Two Pair");
                currentScore.SetBasicScore(15, 2);
                break;

            case HandType.ThreeOfAKind:
                UpdateComboName("Three of a kind");
                currentScore.SetBasicScore(30, 3);
                break;

            case HandType.Straight:
                UpdateComboName("Straight");
                currentScore.SetBasicScore(40, 4);
                break;

            case HandType.Flush:
                UpdateComboName("Flush");
                currentScore.SetBasicScore(45, 4);
                break;

            case HandType.FullHouse:
                UpdateComboName("Full House");
                currentScore.SetBasicScore(60, 5);
                break;

            case HandType.FourOfAKind:
                UpdateComboName("Four of a kind");
                currentScore.SetBasicScore(80, 6);
                break;

            case HandType.StraightFlush:
                UpdateComboName("Straight Flush");
                currentScore.SetBasicScore(100, 8);
                break;

            default:
                UpdateComboName("");
                currentScore.SetBasicScore(0, 0);
                break;
        }
    }


    private HandType EvaluateHand(IReadOnlyList<Card> cards)
    {
        if (cards == null || cards.Count == 0)
            return HandType.None;

        // сортируем по рангу
        var ordered = cards.OrderBy(c => c.Rank).ToList();

        // группы по рангу и масти
        var rankGroups = ordered
            .GroupBy(c => c.Rank)
            .Select(g => g.Count())
            .OrderByDescending(c => c)
            .ToList();

        bool isFlush = cards.GroupBy(c => c.Suit).Any(g => g.Count() >= 5);
        bool isStraight = IsStraight(ordered);

        // пяти-одинаковых можно оставить на будущее
        if (rankGroups.FirstOrDefault() == 4)
            return HandType.FourOfAKind;

        if (rankGroups.FirstOrDefault() == 3 && rankGroups.Skip(1).Any(c => c >= 2))
            return HandType.FullHouse;

        if (isFlush && isStraight)
            return HandType.StraightFlush;

        if (isFlush)
            return HandType.Flush;

        if (isStraight)
            return HandType.Straight;

        if (rankGroups.FirstOrDefault() == 3)
            return HandType.ThreeOfAKind;

        if (rankGroups.Count(c => c == 2) >= 2)
            return HandType.TwoPair;

        if (rankGroups.FirstOrDefault() == 2)
            return HandType.Pair;

        return HandType.HighCard;
    }

    private bool IsStraight(List<Card> ordered)
    {
        if (ordered.Count < 5)
            return false;

        // убираем дубликаты по рангу
        var distinctRanks = ordered
            .Select(c => c.Rank)
            .Distinct()
            .OrderBy(r => r)
            .ToList();

        if (distinctRanks.Count < 5)
            return false;

        int run = 1;
        for (int i = 1; i < distinctRanks.Count; i++)
        {
            // считаем подряд идущие ранги
            if ((int)distinctRanks[i] == (int)distinctRanks[i - 1] + 1)
            {
                run++;
                if (run >= 5)
                    return true;
            }
            else
            {
                run = 1;
            }
        }

        // упрощённо — без специальной обработки A‑2‑3‑4‑5, если нужно — допишем отдельно
        return false;
    }

    private void UpdateComboName(string newName)
    {
        _comboName = newName;

        if (!comboText)
            return;

        if (_animateRoutine != null)
            StopCoroutine(_animateRoutine);

        comboText.text = _comboName;

        // если строка пустая — полностью чистим меш
        if (string.IsNullOrEmpty(_comboName))
        {
            comboText.ForceMeshUpdate(true, true);   // принудительно пересобрать текст[web:191][web:194]
            comboText.textInfo.ClearMeshInfo(false); // очистить вершины/кэш[web:197]
            comboText.UpdateVertexData();            // отправить пустой меш на GPU[web:190]
            return;
        }

        if (!string.IsNullOrWhiteSpace(_comboName))
            _animateRoutine = StartCoroutine(AnimateLettersPop());
    }

    IEnumerator AnimateLettersPop()
    {
        comboText.ForceMeshUpdate();
        TMP_TextInfo textInfo = comboText.textInfo;

        int charCount = textInfo.characterCount;
        if (charCount == 0)
            yield break;

        // сохраняем исходные вершины
        Vector3[][] originalVerts = new Vector3[textInfo.meshInfo.Length][];
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            originalVerts[i] = textInfo.meshInfo[i].vertices.Clone() as Vector3[];
        }

        // идём по буквам
        for (int i = 0; i < charCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible)
                continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // центр буквы
            Vector3 charMidBaseline =
                (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) * 0.5f;

            // локальный оффсет к центру
            Vector3 offset = charMidBaseline;

            float t = 0f;
            while (t < popDuration)
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01(t / popDuration);

                // простая «выпуклая» кривая: 0 → 1 → 0
                float curve = 1f - (normalized - 0.5f) * (normalized - 0.5f) * 4f;
                float scale = Mathf.Lerp(1f, popScale, curve);

                // восстановить базовые вершины
                for (int j = 0; j < 4; j++)
                    vertices[vertexIndex + j] = originalVerts[materialIndex][vertexIndex + j];

                // перенести в ноль, масштабировать вокруг центра, вернуть назад
                for (int j = 0; j < 4; j++)
                {
                    Vector3 v = vertices[vertexIndex + j];
                    v -= offset;
                    v *= scale;
                    v += offset;
                    vertices[vertexIndex + j] = v;
                }

                // применяем изменения в mesh
                comboText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

                yield return null;
            }

            // вернуть букву в исходный масштаб
            for (int j = 0; j < 4; j++)
                vertices[vertexIndex + j] = originalVerts[materialIndex][vertexIndex + j];

            comboText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

            // задержка перед следующей буквой
            yield return new WaitForSeconds(delayBetweenChars);
        }
    }

    List<Card> GetHighCard(List<Card> cards)
    {
        return cards
            .OrderByDescending(c => c.Rank)
            .Take(1)
            .ToList();
    }

    List<Card> GetNOfAKind(List<Card> cards, int n)
    {
        var group = cards
            .GroupBy(c => c.Rank)
            .FirstOrDefault(g => g.Count() == n);

        return group?.ToList() ?? new List<Card>();
    }

    List<Card> GetTwoPairs(List<Card> cards)
    {
        var pairs = cards
            .GroupBy(c => c.Rank)
            .Where(g => g.Count() == 2)
            .OrderByDescending(g => g.Key)
            .Take(2)
            .SelectMany(g => g)
            .ToList();

        return pairs;
    }

    List<Card> GetFlushCards(List<Card> cards)
    {
        var flushGroup = cards
            .GroupBy(c => c.Suit)
            .FirstOrDefault(g => g.Count() >= 5);

        if (flushGroup == null)
            return new List<Card>();

        return flushGroup
            .OrderByDescending(c => c.Rank)
            .Take(5)
            .ToList();
    }

    List<Card> GetStraightCards(List<Card> cards)
    {
        var ordered = cards
            .OrderBy(c => c.Rank)
            .ToList();

        var distinctRanks = ordered
            .GroupBy(c => c.Rank)
            .Select(g => g.First()) // по одной карте на ранг
            .OrderBy(c => c.Rank)
            .ToList();

        if (distinctRanks.Count < 5)
            return new List<Card>();

        List<Card> bestStraight = new();

        int run = 1;
        List<Card> currentRun = new() { distinctRanks[0] };

        for (int i = 1; i < distinctRanks.Count; i++)
        {
            if ((int)distinctRanks[i].Rank == (int)distinctRanks[i - 1].Rank + 1)
            {
                run++;
                currentRun.Add(distinctRanks[i]);

                if (run >= 5)
                    bestStraight = new List<Card>(currentRun);
            }
            else
            {
                run = 1;
                currentRun.Clear();
                currentRun.Add(distinctRanks[i]);
            }
        }

        if (bestStraight.Count >= 5)
            return bestStraight.TakeLast(5).ToList();

        return new List<Card>();
    }

    List<Card> GetFullHouseCards(List<Card> cards)
    {
        var groups = cards
            .GroupBy(c => c.Rank)
            .OrderByDescending(g => g.Count())
            .ThenByDescending(g => g.Key)
            .ToList();

        var three = groups.FirstOrDefault(g => g.Count() >= 3);
        if (three == null)
            return new List<Card>();

        var pair = groups
            .Where(g => g.Key != three.Key && g.Count() >= 2)
            .FirstOrDefault();

        if (pair == null)
            return new List<Card>();

        var result = new List<Card>();
        result.AddRange(three.Take(3));
        result.AddRange(pair.Take(2));
        return result;
    }

    List<Card> GetStraightFlushCards(List<Card> cards)
    {
        // ищем флеш по масти, потом стрит в этих картах
        var flushGroup = cards
            .GroupBy(c => c.Suit)
            .FirstOrDefault(g => g.Count() >= 5);

        if (flushGroup == null)
            return new List<Card>();

        return GetStraightCards(flushGroup.ToList());
    }

    public List<Card> GetComboCards(List<Card> selectedCards)
    {
        if (selectedCards == null || selectedCards.Count == 0)
            return new List<Card>();

        var handType = EvaluateHand(selectedCards);

        switch (handType)
        {
            case HandType.HighCard:
                return GetHighCard(selectedCards);

            case HandType.Pair:
                return GetNOfAKind(selectedCards, 2);

            case HandType.TwoPair:
                return GetTwoPairs(selectedCards);

            case HandType.ThreeOfAKind:
                return GetNOfAKind(selectedCards, 3);

            case HandType.Straight:
                return GetStraightCards(selectedCards);

            case HandType.Flush:
                return GetFlushCards(selectedCards);

            case HandType.FullHouse:
                return GetFullHouseCards(selectedCards);

            case HandType.FourOfAKind:
                return GetNOfAKind(selectedCards, 4);

            case HandType.StraightFlush:
                return GetStraightFlushCards(selectedCards);

            default:
                return new List<Card>();
        }
    }
}

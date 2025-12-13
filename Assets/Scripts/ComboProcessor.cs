using CardScripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComboProcessor : MonoBehaviour
{
    private string _comboName;
    [SerializeField] private TextMeshProUGUI comboText;

    [Header("Letter pop animation")]
    [SerializeField] float popScale = 1.3f;      // во сколько раз увеличивается буква
    [SerializeField] float popDuration = 0.06f;  // длительность одного «попа»
    [SerializeField] float delayBetweenChars = 0.005f; // задержка между буквами

    Coroutine _animateRoutine;

    [SerializeField] private CurrentScore currentScore;

    public void SelectedChanged(List<Card> selectedCards)
    {
        switch (selectedCards.Count)
        {
            case 1:
                {
                    UpdateComboName("High Card");
                    currentScore.SetBasicScore(5, 1);
                    break;
                }
            case 2:
                {
                    UpdateComboName("Pair");
                    currentScore.SetBasicScore(10, 2);
                    break;
                }
            case 3:
                {
                    UpdateComboName("Three of a kind");
                    currentScore.SetBasicScore(30, 3);
                    break;
                }
            case 4:
                {
                    UpdateComboName("Four of a kind");
                    currentScore.SetBasicScore(40, 4);
                    break;
                }
            case 5:
                {
                    UpdateComboName("Five");
                    currentScore.SetBasicScore(100, 8);
                    break;
                }
            default:
                {
                    UpdateComboName("");
                    currentScore.SetBasicScore(0, 0);
                    break;
                }
        }
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
}

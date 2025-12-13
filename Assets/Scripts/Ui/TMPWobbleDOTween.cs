using DG.Tweening;
using TMPro;
using UnityEngine;

public class TMPWobbleDOTween : MonoBehaviour
{
    public float amplitude = 5f;   // высота волны
    public float frequency = 5f;   // как часто волна меняется по буквам
    public float speed = 2f;       // скорость анимации

    TextMeshProUGUI tmp;
    TMP_TextInfo textInfo;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // обновляем mesh
        tmp.ForceMeshUpdate();
        textInfo = tmp.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
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

            // переносим в ноль
            Vector3 offset = charMidBaseline;
            for (int j = 0; j < 4; j++)
                vertices[vertexIndex + j] -= offset;

            // фаза волны: зависит от времени и индекса буквы
            float wave = Mathf.Sin(Time.time * speed + i / frequency) * amplitude;

            // смещение по Y
            Vector3 waveOffset = new Vector3(0f, wave, 0f);
            for (int j = 0; j < 4; j++)
                vertices[vertexIndex + j] += offset + waveOffset;
        }

        // применяем изменённые вершины обратно
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            tmp.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}

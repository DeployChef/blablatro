using TMPro;
using UnityEngine;

public class Card : MonoBehaviour
{
    public bool IsDragged { get; set; }
    public Vector3 HolderPosition { get; set; }

    [SerializeField] private TextMeshProUGUI _nameText;
    
    public void Initialize(string name)
    {
        if(_nameText)
            _nameText.text = name;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

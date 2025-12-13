using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Визуал")]
    public Sprite artwork;

    [Header("Логика")]
    public Suit suit;
    public Rank rank;
}

public enum Suit
{
    Hearts,   // червы
    Diamonds, // бубны
    Clubs,    // трефы
    Spades    // пики
}

public enum Rank
{
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13,
    Ace = 14
}

public static class RankExtensions
{
    public static int ToGameValue(this Rank rank)
    {
        switch (rank)
        {
            case Rank.Two: return 2;
            case Rank.Three: return 3;
            case Rank.Four: return 4;
            case Rank.Five: return 5;
            case Rank.Six: return 6;
            case Rank.Seven: return 7;
            case Rank.Eight: return 8;
            case Rank.Nine: return 9;
            case Rank.Ten: return 10;
            case Rank.Jack: return 10;
            case Rank.Queen: return 10;
            case Rank.King: return 10;
            case Rank.Ace: return 11;
            default: return 0;
        }
    }
}
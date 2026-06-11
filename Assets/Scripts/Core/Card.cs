using System;
using UnityEngine;

namespace BeloteFreeze.Core
{
    public enum Suit { Spades, Hearts, Diamonds, Clubs }
    public enum Rank { Seven, Eight, Nine, Jack, Queen, King, Ten, Ace }

    [Serializable]
    public class Card
    {
        public Suit Suit;
        public Rank Rank;

        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        // Valeur de la carte hors atout
        public int NormalValue()
        {
            return Rank switch
            {
                Rank.Jack  => 2,
                Rank.Queen => 3,
                Rank.King  => 4,
                Rank.Ten   => 10,
                Rank.Ace   => 11,
                _          => 0
            };
        }

        // Valeur de la carte à l'atout
        public int TrumpValue()
        {
            return Rank switch
            {
                Rank.Nine  => 14,
                Rank.Jack  => 20,
                Rank.Queen => 3,
                Rank.King  => 4,
                Rank.Ten   => 10,
                Rank.Ace   => 11,
                _          => 0
            };
        }

        // Valeur selon si la carte est atout ou non
        public int Value(Suit trump)
        {
            return Suit == trump ? TrumpValue() : NormalValue();
        }

        // Ordre normal (pour comparer dans la même couleur)
        public int NormalOrder()
        {
            return Rank switch
            {
                Rank.Seven => 0,
                Rank.Eight => 1,
                Rank.Nine  => 2,
                Rank.Jack  => 3,
                Rank.Queen => 4,
                Rank.King  => 5,
                Rank.Ten   => 6,
                Rank.Ace   => 7,
                _          => 0
            };
        }

        // Ordre à l'atout
        public int TrumpOrder()
        {
            return Rank switch
            {
                Rank.Seven => 0,
                Rank.Eight => 1,
                Rank.Queen => 2,
                Rank.King  => 3,
                Rank.Ten   => 4,
                Rank.Ace   => 5,
                Rank.Nine  => 6,
                Rank.Jack  => 7,
                _          => 0
            };
        }

        // Vérifie si cette carte bat une autre carte (selon les règles belote)
        public bool Beats(Card other, Suit trump)
        {
            if (Suit == trump && other.Suit != trump) return true;
            if (Suit != trump && other.Suit == trump) return false;
            if (Suit == other.Suit)
            {
                return Suit == trump
                    ? TrumpOrder() > other.TrumpOrder()
                    : NormalOrder() > other.NormalOrder();
            }
            return false; // couleur différente, pas atout
        }

        public string SuitSymbol()
        {
            return Suit switch
            {
                Suit.Spades   => "♠",
                Suit.Hearts   => "♥",
                Suit.Diamonds => "♦",
                Suit.Clubs    => "♣",
                _             => "?"
            };
        }

        public string RankSymbol()
        {
            return Rank switch
            {
                Rank.Seven => "7",
                Rank.Eight => "8",
                Rank.Nine  => "9",
                Rank.Jack  => "V",
                Rank.Queen => "D",
                Rank.King  => "R",
                Rank.Ten   => "10",
                Rank.Ace   => "A",
                _          => "?"
            };
        }

        public override string ToString() => $"{RankSymbol()}{SuitSymbol()}";
    }
}

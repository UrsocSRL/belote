using System.Collections.Generic;

namespace BeloteFreeze.Core
{
    public enum PlayerSeat { South = 0, West = 1, North = 2, East = 3 }

    public class Player
    {
        public PlayerSeat Seat;
        public string Name;
        public bool IsHuman;
        public List<Card> Hand = new();

        // Indice de l'équipe : 0 = Nous (Sud+Nord), 1 = Eux (Ouest+Est)
        public int TeamIndex => (Seat == PlayerSeat.South || Seat == PlayerSeat.North) ? 0 : 1;

        public Player(PlayerSeat seat, bool isHuman)
        {
            Seat = seat;
            IsHuman = isHuman;
            Name = seat switch
            {
                PlayerSeat.South => "Vous",
                PlayerSeat.West  => "Ouest",
                PlayerSeat.North => "Nord",
                PlayerSeat.East  => "Est",
                _                => "?"
            };
        }

        public void SetHand(List<Card> cards)
        {
            Hand = new List<Card>(cards);
        }

        public bool RemoveCard(Card card)
        {
            return Hand.Remove(card);
        }

        public bool HasSuit(Suit suit)
        {
            return Hand.Exists(c => c.Suit == suit);
        }

        public bool HasTrump(Suit trump)
        {
            return Hand.Exists(c => c.Suit == trump);
        }

        public List<Card> GetSuit(Suit suit)
        {
            return Hand.FindAll(c => c.Suit == suit);
        }

        public List<Card> GetTrumps(Suit trump)
        {
            return Hand.FindAll(c => c.Suit == trump);
        }
    }
}

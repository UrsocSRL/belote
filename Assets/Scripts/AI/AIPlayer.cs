using System.Collections.Generic;
using System.Linq;
using BeloteFreeze.Core;
using BeloteFreeze.Rules;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// IA Officielle V1 — Bible IA Officielle V1
    /// La sélection de carte passe systématiquement par <see cref="CardSelector"/>,
    /// qui évalue une liste ordonnée de règles (<see cref="ICardSelectionRule"/>) :
    /// chacune représente une "situation de référence" de la bible comportementale.
    /// Cette architecture est conçue pour accueillir des centaines de situations
    /// supplémentaires sans modifier ChooseCard.
    /// Anti-comportements : ne jamais couper son partenaire,
    ///   ne pas jeter un As inutilement, ne pas gaspiller un gros atout.
    /// </summary>
    public class AIPlayer
    {
        // Mémoire IA
        private readonly List<Card>       _playedCards = new();
        private readonly HashSet<Suit>[]  _suitVoid;   // couleurs épuisées par joueur
        private Suit                      _trump;
        private int                       _takerSeat = -1;
        private readonly CardSelector     _selector;

        public AIPlayer()
        {
            _suitVoid = new HashSet<Suit>[4];
            for (int i = 0; i < 4; i++) _suitVoid[i] = new HashSet<Suit>();

            // Ordre de priorité des situations : la première règle applicable
            // détermine la carte jouée. Les futures situations de la bible IA
            // experte viendront s'insérer ici, dans l'ordre voulu.
            _selector = new CardSelector(new ICardSelectionRule[]
            {
                new TakerOpenWithJackRule(),
                new TakerContinueTrumpChaseRule(),
                new TakerFullTrumpControlRule(),
                new OpenTrumpWithoutJackRule(),
                new LeadCardRule(),
                new PartnerMasterRule(),
                new OpponentMasterRule(),
                new DefaultDiscardRule(), // toujours applicable — garantit un retour
            });
        }

        public void Reset(Suit trump, int takerSeat)
        {
            _trump     = trump;
            _takerSeat = takerSeat;
            _playedCards.Clear();
            for (int i = 0; i < 4; i++) _suitVoid[i].Clear();
        }

        /// <summary>
        /// Mémorise une carte jouée et met à jour les couleurs épuisées.
        /// </summary>
        public void RecordPlay(int playerSeat, Card card, Suit leadSuit)
        {
            _playedCards.Add(card);

            // Si le joueur n'a pas suivi la couleur demandée (et n'a pas coupé),
            // on note qu'il est "void" dans cette couleur.
            if (card.Suit != leadSuit && card.Suit != _trump)
                _suitVoid[playerSeat].Add(leadSuit);
        }

        /// <summary>
        /// Tour 1 — Règle de prise V2.
        /// Cas 1 : Valet d'atout présent et ≥3 atouts au total après récupération
        ///         de la carte retournée → PRENDRE.
        /// Cas 2 : 9 d'atout présent, Valet absent, et ≥4 atouts au total après
        ///         récupération de la carte retournée → PRENDRE.
        /// Cas 3 : ni Valet ni 9 → PASSER, quelle que soit la force des autres cartes.
        /// </summary>
        public bool ShouldTakeTrump1(Player player, Card trumpCard)
        {
            var inSuit = player.GetSuit(trumpCard.Suit);
            bool hasJack = inSuit.Any(c => c.Rank == Rank.Jack);
            bool hasNine = inSuit.Any(c => c.Rank == Rank.Nine);
            int totalAfterRecovery = inSuit.Count + 1; // + la carte retournée, récupérée par le preneur

            if (hasJack && totalAfterRecovery >= 3) return true;
            if (hasNine && !hasJack && totalAfterRecovery >= 4) return true;
            return false;
        }

        /// <summary>
        /// Tour 2 — Choisit une couleur (hors couleur exclue) si le joueur a
        /// au moins 4 cartes de cette couleur, ou le Valet + le 9.
        /// </summary>
        public (bool take, Suit suit) ShouldTakeTrump2(Player player, Suit excluded)
        {
            foreach (Suit s in System.Enum.GetValues(typeof(Suit)))
            {
                if (s == excluded) continue;
                var inSuit = player.GetSuit(s);
                bool hasJackAndNine = inSuit.Any(c => c.Rank == Rank.Jack) && inSuit.Any(c => c.Rank == Rank.Nine);
                if (inSuit.Count >= 4 || hasJackAndNine) return (true, s);
            }
            return (false, excluded);
        }

        /// <summary>
        /// Point d'entrée unique de sélection de carte. Construit le contexte
        /// de décision (<see cref="TrickContext"/>) puis délègue au
        /// <see cref="CardSelector"/>.
        /// </summary>
        public Card ChooseCard(
            Player player,
            List<Card> allowedCards,
            List<TrickPlay> currentTrick)
        {
            var ctx = TrickContext.Build(player, allowedCards, currentTrick, _trump, _playedCards, _suitVoid, _takerSeat);
            return _selector.ChooseBestCard(ctx);
        }

        // ── MÉMOIRE ──────────────────────────────────────────────────────────
        public bool IsTrumpPlayed(Rank rank) =>
            _playedCards.Any(c => c.Suit == _trump && c.Rank == rank);

        public List<Card> GetRemainingTrumps(Player player) =>
            player.GetTrumps(_trump)
                  .Where(c => !_playedCards.Any(p => p.Suit == c.Suit && p.Rank == c.Rank))
                  .ToList();
    }
}

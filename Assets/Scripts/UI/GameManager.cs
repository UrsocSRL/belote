using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeloteFreeze.Core;
using BeloteFreeze.Rules;
using BeloteFreeze.AI;

namespace BeloteFreeze.UI
{
    /// <summary>
    /// GameManager — Bible Demo V0.1
    /// Chef d'orchestre : distribution, prise d'atout, déroulement des plis, scores.
    /// Aucun bouton Jouer : une carte touchée est immédiatement jouée.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        public UIManager UIManager;

        [Header("Settings")]
        [Tooltip("Délai IA en secondes")]
        public float AIDelay = 0.6f;

        // ── Objets métier ─────────────────────────────────────────────────────
        private readonly Deck          _deck          = new();
        private readonly Player[]      _players       = new Player[4];
        private readonly TrickManager  _trickMgr      = new();
        private readonly TrumpManager  _trumpMgr      = new();
        private readonly ScoreManager  _scoreMgr      = new();
        private readonly BeloteTracker _beloteTracker = new();
        private readonly AIPlayer      _ai            = new();
        public  readonly GameState     State          = new();

        // ── Contrôle de flux ─────────────────────────────────────────────────
        private bool _waitingForHuman;

        // ─────────────────────────────────────────────────────────────────────
        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            InitPlayers();
            StartNewGame();
        }

        void InitPlayers()
        {
            _players[0] = new Player(PlayerSeat.South, isHuman: true);
            _players[1] = new Player(PlayerSeat.West,  isHuman: false);
            _players[2] = new Player(PlayerSeat.North, isHuman: false);
            _players[3] = new Player(PlayerSeat.East,  isHuman: false);
        }

        // ── Nouvelle partie ───────────────────────────────────────────────────
        public void StartNewGame()
        {
            _scoreMgr.Reset();
            State.DealerSeat = 0;
            StartNewHand();
        }

        // ── Nouvelle manche ───────────────────────────────────────────────────
        public void StartNewHand()
        {
            State.ResetRound();
            foreach (var p in _players) p.Hand.Clear();

            var (hands, trumpCard) = _deck.Deal(State.DealerSeat);
            for (int i = 0; i < 4; i++) _players[i].SetHand(hands[i]);

            _trumpMgr.StartTrump(trumpCard, State.DealerSeat);
            State.Phase = GamePhase.TrumpSelection;

            UIManager?.OnDeal(_players, trumpCard);
            StartCoroutine(TrumpSelectionLoop());
        }

        // ── PRISE D'ATOUT ─────────────────────────────────────────────────────
        IEnumerator TrumpSelectionLoop()
        {
            while (!_trumpMgr.IsComplete)
            {
                int  asker   = _trumpMgr.CurrentAskerSeat;
                bool isHuman = _players[asker].IsHuman;

                UIManager?.OnTrumpAsk(asker, _trumpMgr.Phase, _trumpMgr.TrumpCard);

                if (isHuman)
                {
                    _waitingForHuman = true;
                    yield return new WaitUntil(() => !_waitingForHuman);
                }
                else
                {
                    yield return new WaitForSeconds(AIDelay);
                    AITrumpDecision(asker);
                }
            }

            if (_trumpMgr.Phase == TrumpPhase.Redeal)
            {
                // Cas 14 — Nouvelle donne
                UIManager?.ShowMessage("Tout le monde passe — nouvelle donne", 1.2f);
                yield return new WaitForSeconds(1.5f);
                State.DealerSeat = (State.DealerSeat + 1) % 4;
                StartNewHand();
                yield break;
            }

            // Atout validé
            State.Trump           = _trumpMgr.ChosenTrump!.Value;
            State.TakerSeat       = _trumpMgr.TakerSeat;
            State.TakerTeam       = _players[_trumpMgr.TakerSeat].TeamIndex;
            State.TrickLeaderSeat = (State.DealerSeat + 1) % 4;

            _trickMgr.Reset(State.Trump);
            _beloteTracker.Reset(State.Trump);
            _beloteTracker.DetectHolder(_players);
            _ai.Reset(State.Trump);

            State.BeloteHolderSeat = _beloteTracker.HolderSeat;
            State.Phase = GamePhase.Playing;

            UIManager?.OnTrumpChosen(State.Trump, State.TakerSeat);
            yield return new WaitForSeconds(0.3f);
            StartCoroutine(PlayLoop());
        }

        void AITrumpDecision(int seat)
        {
            if (_trumpMgr.Phase == TrumpPhase.Round1)
            {
                if (_ai.ShouldTakeTrump1(_players[seat], _trumpMgr.TrumpCard))
                    _trumpMgr.Take(seat);
                else
                    _trumpMgr.Pass();
            }
            else
            {
                var (take, suit) = _ai.ShouldTakeTrump2(_players[seat], _trumpMgr.TrumpCard.Suit);
                if (take) _trumpMgr.TakeSuit(seat, suit);
                else      _trumpMgr.Pass();
            }
        }

        // ── Décisions humaines — prise d'atout ────────────────────────────────
        public void HumanTake()              { _trumpMgr.Take(0);            _waitingForHuman = false; }
        public void HumanTakeSuit(Suit suit) { _trumpMgr.TakeSuit(0, suit); _waitingForHuman = false; }
        public void HumanPass()              { _trumpMgr.Pass();             _waitingForHuman = false; }

        // ── BOUCLE DE JEU ─────────────────────────────────────────────────────
        IEnumerator PlayLoop()
        {
            // FIX ligne 173 — condition robuste : 8 plis, peu importe la synchro des mains
            while (_trickMgr.TrickCount < 8)
            {
                int leader = State.TrickLeaderSeat;
                State.CurrentPlayerSeat = leader;
                State.Phase = GamePhase.Playing;

                // 4 joueurs jouent dans l'ordre
                for (int i = 0; i < 4; i++)
                {
                    int seat = (leader + i) % 4;
                    State.CurrentPlayerSeat = seat;
                    UIManager?.OnPlayerTurn(seat);

                    if (_players[seat].IsHuman)
                    {
                        var ok = _trickMgr.GetAllowedCards(_players[0]);
                        UIManager?.HighlightAllowedCards(ok);
                        _waitingForHuman = true;
                        yield return new WaitUntil(() => !_waitingForHuman);
                    }
                    else
                    {
                        yield return new WaitForSeconds(AIDelay);
                        var ok   = _trickMgr.GetAllowedCards(_players[seat]);
                        var card = _ai.ChooseCard(_players[seat], ok, _trickMgr.CurrentTrick);
                        ExecutePlay(seat, card);
                    }
                }

                // Résolution du pli
                State.Phase = GamePhase.TrickEnd;
                yield return new WaitForSeconds(0.4f);

                var (winnerSeat, pts) = _trickMgr.ResolveTrick();
                int winnerTeam = _players[winnerSeat].TeamIndex;
                State.RoundPoints[winnerTeam] += pts;
                State.TricksWon[winnerSeat]++;
                State.TrickLeaderSeat = winnerSeat;

                UIManager?.OnTrickEnd(winnerSeat, pts);
                yield return new WaitForSeconds(0.5f);
            }

            yield return StartCoroutine(EndHand());
        }

        // ── JOUER UNE CARTE ───────────────────────────────────────────────────
        /// <summary>
        /// Bible Demo : une carte touchée est immédiatement jouée. Pas de bouton Jouer.
        /// </summary>
        public void HumanCardTouched(Card card)
        {
            if (!_waitingForHuman) return;
            var ok = _trickMgr.GetAllowedCards(_players[0]);
            if (!ok.Contains(card)) return;

            ExecutePlay(0, card);
            _waitingForHuman = false;
        }

        void ExecutePlay(int seat, Card card)
        {
            Suit leadSuit = _trickMgr.CurrentTrick.Count > 0
                ? _trickMgr.CurrentTrick[0].Card.Suit
                : card.Suit;

            _players[seat].RemoveCard(card);
            _trickMgr.PlayCard(seat, card);
            _ai.RecordPlay(seat, card, leadSuit);

            // Cas 6/7 — Belote / Rebelote
            if (_beloteTracker.OnCardPlayed(seat, card, out string announcement))
            {
                State.BeloteAnnounced   = _beloteTracker.BeloteAnnounced;
                State.RebeloteAnnounced = _beloteTracker.RebeloteAnnounced;
                UIManager?.ShowBeloteAnnounce(seat, announcement);
            }

            UIManager?.OnCardPlayed(seat, card, _trickMgr.CurrentTrick);
        }

        // ── FIN DE MANCHE ─────────────────────────────────────────────────────
        IEnumerator EndHand()
        {
            State.Phase = GamePhase.RoundEnd;

            int usTotal   = State.TricksWon[0] + State.TricksWon[2];
            int themTotal = State.TricksWon[1] + State.TricksWon[3];

            var result = _scoreMgr.ComputeRound(
                State.RoundPoints[0], State.RoundPoints[1],
                usTotal, themTotal,
                State.TakerTeam,
                _beloteTracker.IsComplete,
                _beloteTracker.BeloteTeam
            );

            UIManager?.OnRoundEnd(result, _scoreMgr.TotalScoreUs, _scoreMgr.TotalScoreThem);
            yield return null;
        }

        // ── API publique pour UIManager ───────────────────────────────────────

        /// <summary>
        /// FIX — Expose le nombre de cartes d'un joueur IA (pour RenderAIBack).
        /// </summary>
        public int GetPlayerHandCount(int seat)
        {
            if (seat < 0 || seat >= _players.Length) return 0;
            return _players[seat].Hand.Count;
        }

        /// <summary>
        /// FIX — Expose le dernier pli pour le bouton LastTrick.
        /// </summary>
        public List<TrickPlay> GetLastTrick()
        {
            return _trickMgr.LastTrick ?? new List<TrickPlay>();
        }

        // ── Nouvelle manche (appelée par UIManager) ───────────────────────────
        public void OnNextHandRequested()
        {
            State.DealerSeat = (State.DealerSeat + 1) % 4;
            StartNewHand();
        }
    }
}

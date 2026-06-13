using BeloteFreeze.Core;

namespace BeloteFreeze.AI
{
    /// <summary>
    /// Représente une "situation de référence" de la bible comportementale
    /// de l'IA experte (ex : "partenaire maître, rien à gagner à charger").
    /// Les règles sont évaluées dans l'ordre par <see cref="CardSelector"/> ;
    /// la première qui s'applique détermine la carte jouée.
    /// </summary>
    public interface ICardSelectionRule
    {
        /// <summary>Nom descriptif (debug / logs).</summary>
        string Name { get; }

        /// <summary>
        /// Tente de choisir une carte pour ce contexte.
        /// Retourne false si la situation ne correspond pas à cette règle.
        /// </summary>
        bool TryChoose(TrickContext ctx, out Card chosen);
    }
}

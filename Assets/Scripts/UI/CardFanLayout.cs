using UnityEngine;

namespace BeloteFreeze.UI
{
    /// <summary>
    /// Dispose les cartes enfants en eventail (rotation + arc) au lieu d'un alignement horizontal.
    /// Se reapplique automatiquement quand des cartes sont ajoutees/retirees (RenderHumanHand).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class CardFanLayout : MonoBehaviour
    {
        [Tooltip("Decalage horizontal entre deux cartes (chevauchement si < largeur de carte)")]
        public float CardSpacing = 64f;

        [Tooltip("Angle max (en degres) applique aux cartes des extremites")]
        public float MaxAngle = 16f;

        [Tooltip("Hauteur de l'arc (les cartes du centre sont plus hautes)")]
        public float ArcHeight = 36f;

        void OnTransformChildrenChanged() => Arrange();

        void OnEnable() => Arrange();

        public void Arrange()
        {
            int count = transform.childCount;
            if (count == 0) return;

            float totalWidth = (count - 1) * CardSpacing;
            float startX = -totalWidth / 2f;

            for (int i = 0; i < count; i++)
            {
                if (transform.GetChild(i) is not RectTransform child) continue;

                float t = count > 1 ? (float)i / (count - 1) : 0.5f;     // 0..1
                float centered = (t - 0.5f) * 2f;                        // -1..1

                float x = startX + i * CardSpacing;
                float y = ArcHeight * (1f - centered * centered);        // arc vers le haut au centre
                float angle = Mathf.Lerp(MaxAngle, -MaxAngle, t);

                child.anchoredPosition = new Vector2(x, y);
                child.localRotation    = Quaternion.Euler(0, 0, angle);
            }
        }
    }
}

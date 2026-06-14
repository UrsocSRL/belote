using UnityEngine;

namespace BeloteFreeze.UI
{
    /// <summary>
    /// Adapte la zone de table (TableZone) a l'orientation de l'appareil.
    /// Portrait : table plus haute que large, image pivotee de 90° (table.png est un ovale "paysage").
    /// Paysage   : table plus large que haute, image non pivotee.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TableOrientationController : MonoBehaviour
    {
        public Vector2 PortraitSize  = new Vector2(560, 680);
        public Vector2 LandscapeSize = new Vector2(740, 460);

        [Tooltip("RectTransform de l'image de table (pivotee en portrait car table.png est un ovale paysage)")]
        public RectTransform TableImage;

        RectTransform _rt;
        bool _lastLandscape;
        bool _initialized;

        void Awake() => _rt = GetComponent<RectTransform>();

        void Update() => Apply();

        void Apply()
        {
            bool isLandscape = Screen.width > Screen.height;
            if (_initialized && isLandscape == _lastLandscape) return;

            _rt.sizeDelta = isLandscape ? LandscapeSize : PortraitSize;
            if (TableImage != null)
            {
                TableImage.localEulerAngles = new Vector3(0, 0, isLandscape ? 0f : 90f);
                // En portrait, table.png est pivote de 90° ; comme le dessin
                // n'est pas centre verticalement dans son cadre, la rotation
                // se traduit par un decalage horizontal qu'on compense ici.
                TableImage.anchoredPosition = new Vector2(isLandscape ? 0f : 24f, 0f);
            }

            _lastLandscape = isLandscape;
            _initialized = true;
        }
    }
}

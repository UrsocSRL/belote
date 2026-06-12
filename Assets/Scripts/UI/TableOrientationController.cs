using UnityEngine;

namespace BeloteFreeze.UI
{
    /// <summary>
    /// Adapte la zone de table (TableZone) a l'orientation de l'appareil.
    /// Portrait : table plus haute que large. Paysage : table plus large que haute.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TableOrientationController : MonoBehaviour
    {
        public Vector2 PortraitSize  = new Vector2(420, 600);
        public Vector2 LandscapeSize = new Vector2(640, 380);

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
            _lastLandscape = isLandscape;
            _initialized = true;
        }
    }
}

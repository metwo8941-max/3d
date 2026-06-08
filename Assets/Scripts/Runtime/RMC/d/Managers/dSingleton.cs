using RMC.d.Scenes;
using UnityEngine;
using UnityEngine.UIElements;

namespace RMC.d
{
    /// <summary>
    /// A game may have multiple scenes. One way to have functionality
    /// easily come along to each new Scene is with a light Singleton pattern.
    /// <remarks>
    /// This is a simplified implementation.
    /// </remarks>
    /// </summary>
    public class dSingleton : MonoBehaviour
    {
        //  Singleton ------------------------------------
        public static dSingleton Instance { get { return _instance; } }
        private static dSingleton _instance;

        //  Properties ------------------------------------
        public ThemeManager ThemeManager { get { return _themeManager; } }

        //  Fields ----------------------------------------
        [Header("Styles")]
        [SerializeField]
        private StyleSheet _lightStyleSheet;

        [SerializeField]
        private StyleSheet _darkStyleSheet;
        private ThemeManager _themeManager;


        //  Unity Methods ---------------------------------
        protected void Start()
        {
            _instance = this;
            _themeManager = new ThemeManager(_lightStyleSheet, _darkStyleSheet);
        }


        //  Methods ---------------------------------------


        //  Event Handlers --------------------------------
    }
}
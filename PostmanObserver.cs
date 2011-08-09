using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Web.UI;

namespace MisterPostman
{
    /// <summary>
    /// Object that observe changes of a control state.
    /// </summary>
    public class PostmanObserver
    {
        private static PropertyInfo _viewStateProperty;

        private List<string> _checksums;

        static PostmanObserver()
        {
            // Initialize ViewState PropertyInfo, a protected property of Control type.
            _viewStateProperty = typeof(Control).GetProperty("ViewState", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Creates a new object that observe changes of a specific control.
        /// </summary>
        /// <param name="target">Control to observe.</param>
        public PostmanObserver(Control target)
        {
            TargetControl = target;

            _checksums = new List<string>();
        }

        /// <summary>
        /// Control to observe.
        /// </summary>
        public Control TargetControl { get; private set; }

        /// <summary>
        /// All taken checksums.
        /// </summary>
        public string[] Checksums
        {
            get { return _checksums.ToArray(); }
        }

        /// <summary>
        /// Gets a checksum of the control state (protected ViewState).
        /// </summary>
        public void TakeChecksum()
        {
            // Gets the ViewState (StateBag) of the observed control and transform in a dictionary.
            var controlStateBag = GetViewStateOf(TargetControl);
            var bagDictionary = GenerateStateBagDictionary(controlStateBag);

            using (var memoryStream = new MemoryStream())
            {
                // Create a binary representation of dictionary and transforms it in a MD5 hash.
                new BinaryFormatter().Serialize(memoryStream, bagDictionary);
                var md5HashBytes = MD5.Create().ComputeHash(memoryStream.ToArray());

                // Adds a MD5 hash in checksums list.
                _checksums.Add(Convert.ToBase64String(md5HashBytes));
            }
        }

        /// <summary>
        /// If the control state changed between the taken checksums.
        /// </summary>
        public bool IsChanged
        {
            get
            {
                // If has just one checksum, can't determine if state changed.
                if(_checksums.Count < 1) return false;

                return _checksums.Exists(s => s != _checksums.First());
            }
        }

        /// <summary>
        /// Get invisible ViewState (protected property) of a Control.
        /// </summary>
        private StateBag GetViewStateOf(Control control)
        {
            return _viewStateProperty.GetValue(control, null) as StateBag;
        }

        /// <summary>
        /// Transforms a ViewState in a dictionary to generate a serialized state of values to creates checksums.
        /// </summary>
        private static Dictionary<string, object> GenerateStateBagDictionary(StateBag bagTarget)
        {
            var bagDictionary = new Dictionary<string, object>();

            foreach (var key in bagTarget.Keys)
            {
                var stringKey = key as string;

                bagDictionary.Add(stringKey, bagTarget[stringKey]);
            }

            return bagDictionary;
        }
    }
}
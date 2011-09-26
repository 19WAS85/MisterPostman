using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;

namespace MisterPostman
{
    /// <summary>
    /// Object that active Postman in a page.
    /// </summary>
    public class PostmanActivator
    {
        private Page _page;
        private Control[] _controls;
        private PostmanObserver[] _observers;
        private Stopwatch _stopwatch;

        /// <summary>
        /// Creates a activator to a page.
        /// </summary>
        public PostmanActivator(Page page)
        {
            _page = page;

            // Handles page events to check changed controls.
            _page.Load += new EventHandler(page_Load);
            _page.SaveStateComplete += new EventHandler(page_SaveStateComplete);

            _stopwatch = new Stopwatch();
        }

        void page_Load(object sender, EventArgs e)
        {
            _stopwatch.Start();

            // Gets all page controls (ignores literals).
            _controls = FlattenHierachy(_page);
            _observers = new PostmanObserver[_controls.Length];

            for (int i = 0; i < _controls.Length; i++)
            {
                // Set contitional UpdatePanels.
                var updatePanel = _controls[i] as UpdatePanel;
                if (updatePanel != null)
                {
                    updatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;
                    updatePanel.ChildrenAsTriggers = false;
                }

                // Create an observer to each control and takes the initial checksum.
                _observers[i] = new PostmanObserver(_controls[i]);
                _observers[i].TakeChecksum();
            }

            _stopwatch.Stop();
        }

        void page_SaveStateComplete(object sender, EventArgs e)
        {
            _stopwatch.Start();

            foreach (var o in _observers)
            {
                // Takes the last checksum.
                o.TakeChecksum();

                if (o.IsChanged)
                {
                    // If control state changed, get the parent UpdatePanel to update.
                    var updatePanel = GetUpdatePanelOf(o.TargetControl);

                    if (updatePanel != null) updatePanel.Update();
                }
            }

            _stopwatch.Stop();
        }

        /// <summary>
        /// Gets the UpdatePanel of a Control.
        /// </summary>
        private UpdatePanel GetUpdatePanelOf(Control control)
        {
            if (control.Parent == null) return null;

            if(control.Parent is UpdatePanel) return control.Parent as UpdatePanel;
            else return GetUpdatePanelOf(control.Parent);
        }

        /// <summary>
        /// Get all page controls, recursively, ignoring irrelevant controls.
        /// </summary>
        public static Control[] FlattenHierachy(Control root)
        {
            if (IgnoreControl(root)) return new Control[0];

            var list = new List<Control>() { root };

            if (root.HasControls())
            {
                foreach (Control c in root.Controls)
                {
                    list.AddRange(FlattenHierachy(c));
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Verifies if FlattenHierachy goes to ignore this control.
        /// </summary>
        private static bool IgnoreControl(Control root)
        {
            return root is LiteralControl;
        }
    }
}

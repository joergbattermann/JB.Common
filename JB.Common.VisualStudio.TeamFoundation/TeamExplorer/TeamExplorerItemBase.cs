// -----------------------------------------------------------------------
// <copyright file="TeamExplorerItemBase.cs" company="Joerg Battermann">
//   Copyright (c) 2015 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System.ComponentModel.Composition;

namespace JB.VisualStudio.TeamFoundation.TeamExplorer
{
    [PartNotDiscoverable]
    public abstract class TeamExplorerItemBase : TeamExplorerBase
    {
        private bool _isEnabled;
        private bool _isVisible;
        private string _text;

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            protected set
            {
                _isEnabled = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is visible.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible
        {
            get { return _isVisible; }
            protected set
            {
                _isVisible = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the text.
        /// </summary>
        /// <value>
        ///     The text.
        /// </value>
        public string Text
        {
            get { return _text; }
            protected set
            {
                _text = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     If this instance can 'execute', 'start', 'trigger' etc an activity (depends on the ITeamFoundation* interface it is
        ///     used with), this is the trigger for it
        /// </summary>
        public virtual void Execute()
        {
            //var a = KnownMonikers.
        }

        /// <summary>
        ///     Invalidates this instance and thereby requesting a re-draw of its content, if applicable.
        ///     Re-Evaluation of <see cref="IsVisible" /> and <see cref="IsEnabled" /> should also be be (re-)performed in here.
        /// </summary>
        public virtual void Invalidate()
        {
        }
    }
}
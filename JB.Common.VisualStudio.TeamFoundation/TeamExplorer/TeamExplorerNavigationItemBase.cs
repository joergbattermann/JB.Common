// -----------------------------------------------------------------------
// <copyright file="TeamExplorerNavigationItemBase.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media;
using Microsoft.TeamFoundation.Controls;

namespace JB.VisualStudio.TeamFoundation.TeamExplorer
{
    /// <summary>
    ///     Base class for <see cref="ITeamExplorerNavigationItem" /> and <see cref="ITeamExplorerNavigationItem2" />
    ///     implementations.
    ///     These are basically the 'main' buttons / functionalities shown on the Team Explorer home page.
    ///     Besides deriving from this class, implementations also have to be marked with the
    ///     <see cref="TeamExplorerNavigationItemAttribute" />
    ///     in order to be picked up by visual studio. This Attribute wants a guid but also a priority - TFS / Team Explorer
    ///     ones are defined in
    ///     <see cref="TeamExplorerNavigationItemPriority" /> so one can take those and choose a value before (lower) or after
    ///     (higher values) them.
    /// </summary>
    [PartNotDiscoverable]
    public abstract class TeamExplorerNavigationItemBase : TeamExplorerItemBase, ITeamExplorerNavigationItem2
    {
        private int _argbColor;
        private DrawingBrush _icon;
        private Image _image;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeamExplorerNavigationItemBase" /> class.
        /// </summary>
        protected TeamExplorerNavigationItemBase()
        {
            IsVisible = false;
            IsEnabled = true;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            _icon = null;

            base.Dispose();
        }

        /// <summary>
        ///     Gets or sets the image.
        /// </summary>
        /// <value>
        ///     The image.
        /// </value>
        public Image Image
        {
            get { return _image; }
            protected set
            {
                _image = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the color of the ARGB.
        /// </summary>
        /// <value>
        ///     The color of the ARGB.
        /// </value>
        public int ArgbColor
        {
            get { return _argbColor; }
            protected set
            {
                _argbColor = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the icon.
        /// </summary>
        /// <value>
        ///     The icon.
        /// </value>
        public object Icon
        {
            get { return _icon; }
            protected set
            {
                Debug.Assert(value is DrawingBrush, "Icon must be a DrawingBrush");

                _icon = (DrawingBrush) value;
                RaisePropertyChanged();
            }
        }
    }
}
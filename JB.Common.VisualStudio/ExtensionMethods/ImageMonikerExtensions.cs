// -----------------------------------------------------------------------
// <copyright file="ImageMonikerExtensions.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace JB.VisualStudio.ExtensionMethods
{
    /// <summary>
    /// Extension Methods for <see cref="Microsoft.VisualStudio.Imaging.Interop.ImageMoniker"/> instances.
    /// </summary>
    public static class ImageMonikerExtensions
    {
        /// <summary>
        /// Converts the given <paramref name="imageMoniker"/> to its <see cref="BitmapSource"/> counterpart.
        /// </summary>
        /// <param name="imageMoniker">The image moniker.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="visualStudioImageService">The visual studio image service.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Must be a positive, non-zero value
        /// or
        /// Must be a positive, non-zero value
        /// </exception>
        public static BitmapSource ToBitmapSource(this ImageMoniker imageMoniker, int width, int height, IVsImageService2 visualStudioImageService)
        {
            // based on https://github.com/madskristensen/ExtensibilityTools/blob/master/src/Misc/Commands/ImageMonikerDialog.xaml.cs#L47

            if (visualStudioImageService == null) throw new ArgumentNullException(nameof(visualStudioImageService));
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Must be a positive, non-zero value");
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Must be a positive, non-zero value");

            if (Microsoft.VisualStudio.Imaging.ExtensionMethods.IsNullImage(imageMoniker))
                return null;

            ImageAttributes imageAttributes = new ImageAttributes
            {
                Flags = (uint)_ImageAttributesFlags.IAF_RequiredFlags,
                ImageType = (uint)_UIImageType.IT_Bitmap,
                Format = (uint)_UIDataFormat.DF_WPF,
                LogicalHeight = height,
                LogicalWidth = width,
                StructSize = Marshal.SizeOf(typeof(ImageAttributes))
            };

            IVsUIObject result = visualStudioImageService.GetImage(imageMoniker, imageAttributes);

            object data;
            result.get_Data(out data);

            return data as BitmapSource;
        }
    }
}
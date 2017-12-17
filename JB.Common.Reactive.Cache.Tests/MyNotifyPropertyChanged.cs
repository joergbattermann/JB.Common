// -----------------------------------------------------------------------
// <copyright file="MyNotifyPropertyChanged.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JB.Reactive.Cache.Tests
{
    public class MyNotifyPropertyChanged<TKey, TValue> : INotifyPropertyChanged
    {
        private TValue _firstProperty;
        private TValue _secondProperty;

        /// <summary>
        ///     Gets or sets the first property.
        /// </summary>
        /// <value>
        ///     The first property.
        /// </value>
        public TValue FirstProperty
        {
            get
            {
                return _firstProperty;
            }
            set
            {
                if (ValueComparer.Equals(value, _firstProperty))
                    return;

                _firstProperty = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Gets the key.
        /// </summary>
        /// <value>
        ///     The key.
        /// </value>
        public TKey Key { get; }

        /// <summary>
        ///     Gets or sets the second property.
        /// </summary>
        /// <value>
        ///     The second property.
        /// </value>
        public TValue SecondProperty
        {
            get
            {
                return _secondProperty;
            }
            set
            {
                if (ValueComparer.Equals(value, _secondProperty))
                    return;

                _secondProperty = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Gets the comparer.
        /// </summary>
        /// <value>
        ///     The comparer.
        /// </value>
        private IEqualityComparer<TValue> ValueComparer { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MyNotifyPropertyChanged{TKey,TValue}" /> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="firstProperty">The first property.</param>
        /// <param name="secondProperty">The second property.</param>
        /// <param name="valueComparer">The value comparer.</param>
        public MyNotifyPropertyChanged(TKey key, TValue firstProperty = default(TValue), TValue secondProperty = default(TValue), IEqualityComparer<TValue> valueComparer = null)
        {
            Key = key;

            _firstProperty = firstProperty;
            _secondProperty = secondProperty;

            ValueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
        }

        /// <summary>
        ///     Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Raises the <see cref="PropertyChanged" /> event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
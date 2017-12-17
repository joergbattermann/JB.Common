// -----------------------------------------------------------------------
// <copyright file="BufferWhileTests.cs" company="Joerg Battermann">
//   Copyright (c) 2017 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// <summary></summary>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using FluentAssertions;
using JB.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Xunit;

namespace JB.Reactive.Tests.Linq
{
    public class BufferWhileTests
    {
        [Theory]
        [InlineData(0, 1)]
        [InlineData(10, 100)]
        [InlineData(0, 1000)]
        public void BufferWhileShouldBufferWhilePredicateIsTrue(int start, int count)
        {
            // given
            var testScheduler = new TestScheduler();
            var observable = System.Reactive.Linq.Observable.Range(start, count).ObserveOn(testScheduler).BufferWhile(() => true);
            var observer = testScheduler.CreateObserver<IList<int>>();
            observable.Subscribe(observer);

            // when below range end
            testScheduler.AdvanceBy(count);

            // then
            observer.Messages.Count.Should().Be(0);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(10, 100)]
        [InlineData(0, 1000)]
        public void BufferWhileShouldReleaseBufferOnCompleted(int start, int count)
        {
            // given
            var testScheduler = new TestScheduler();
            var observable = System.Reactive.Linq.Observable.Range(start, count).ObserveOn(testScheduler).BufferWhile(() => true);
            var observer = testScheduler.CreateObserver<IList<int>>();
            observable.Subscribe(observer);

            // when producer ran to completion
            testScheduler.AdvanceBy(count + 1);
            
            // then
            observer.Messages.Count.Should().Be(2);
            observer.Messages[0].Value.Kind.Should().Be(NotificationKind.OnNext);
            observer.Messages[0].Value.Value.Count.Should().Be(count);

            observer.Messages[1].Value.Kind.Should().Be(NotificationKind.OnCompleted);
        }
    }
}
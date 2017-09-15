﻿// <copyright file="TestReporter.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics.Filters;
using App.Metrics.Internal.NoOp;
using App.Metrics.Reporting;

namespace App.Metrics.Facts.TestHelpers
{
    public class TestReporter : IReportMetrics
    {
        private readonly bool _pass;
        private readonly Exception _throwEx;

        public TestReporter(bool pass = true, Exception throwEx = null)
        {
            _pass = throwEx == null && pass;
            _throwEx = throwEx;
            FlushInterval = TimeSpan.FromMilliseconds(10);
            Filter = new NullMetricsFilter();
        }

        public TestReporter(TimeSpan interval, bool pass = true, Exception throwEx = null)
        {
            FlushInterval = interval;
            _pass = throwEx == null && pass;
            _throwEx = throwEx;
            Filter = new NullMetricsFilter();
        }

        public IFilterMetrics Filter { get; set; }

        public TimeSpan FlushInterval { get; set; }

        public Task<bool> FlushAsync(MetricsDataValueSource metricsData, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}
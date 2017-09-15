﻿// <copyright file="DefaultMetricsReportRunner.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics.Counter;
using App.Metrics.Formatters;
using App.Metrics.Logging;
using App.Metrics.Reporting;

namespace App.Metrics.Internal
{
    public class DefaultMetricsReportRunner : IRunMetricsReports
    {
        private static readonly ILog Logger = LogProvider.For<DefaultMetricsReportRunner>();
        private readonly IMetrics _metrics;
        private readonly CounterOptions _failedCounter;
        private readonly MetricsReporterCollection _reporters;

        private readonly CounterOptions _successCounter;

        public DefaultMetricsReportRunner(IMetrics metrics, MetricsReporterCollection reporters)
        {
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
            _reporters = reporters ?? throw new ArgumentNullException(nameof(reporters));

            _successCounter = new CounterOptions
                              {
                                  Context = AppMetricsConstants.InternalMetricsContext,
                                  MeasurementUnit = Unit.Items,
                                  ResetOnReporting = true,
                                  Name = "report_success"
                              };

            _failedCounter = new CounterOptions
                             {
                                 Context = AppMetricsConstants.InternalMetricsContext,
                                 MeasurementUnit = Unit.Items,
                                 ResetOnReporting = true,
                                 Name = "report_failed"
                             };
        }

        /// <inheritdoc />
        public IEnumerable<Task> RunAllAsync(CancellationToken cancellationToken = default)
        {
            return _reporters.Select(reporter => FlushMetrics(_metrics, cancellationToken, reporter));
        }

        private async Task FlushMetrics(IMetrics metrics, CancellationToken cancellationToken, IReportMetrics reporter)
        {
            try
            {
                Logger.ReportRunning(reporter);

                var result = await reporter.FlushAsync(metrics.Snapshot.Get(reporter.Filter), cancellationToken);

                if (result)
                {
                    metrics.Measure.Counter.Increment(_successCounter, reporter.GetType().FullName);
                }
                else
                {
                    metrics.Measure.Counter.Increment(_failedCounter, reporter.GetType().FullName);
                    Logger.ReportFailed(reporter);
                }
            }
            catch (Exception ex)
            {
                metrics.Measure.Counter.Increment(_failedCounter, reporter.GetType().FullName);
                Logger.ReportFailed(reporter, ex);
            }
        }
    }
}
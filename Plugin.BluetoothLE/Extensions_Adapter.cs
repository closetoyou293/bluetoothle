﻿using System;
using System.Reactive.Linq;


namespace Plugin.BluetoothLE
{
    public static partial class Extensions
    {
        public static bool CanOpenSettings(this IAdapter adapter) => adapter.Features.HasFlag(AdapterFeatures.OpenSettings);
        public static bool CanViewPairedDevices(this IAdapter adapter) => adapter.Features.HasFlag(AdapterFeatures.ViewPairedDevices);
        public static bool CanControlAdapterState(this IAdapter adapter) => adapter.Features.HasFlag(AdapterFeatures.ControlAdapterState);
        public static bool CanPerformLowPoweredScans(this IAdapter adapter) => adapter.Features.HasFlag(AdapterFeatures.LowPoweredScan);


        public static IObservable<IDevice> ScanForUniqueDevices(this IAdapter adapter, ScanConfig config = null) => adapter
            .Scan(config)
            .Distinct(x => x.Device.Uuid)
            .Select(x => x.Device);


        public static IObservable<IScanResult> ScanWhenAdapterReady(this IAdapter adapter, ScanConfig config = null) => adapter
            .WhenStatusChanged()
            .Where(x => x == AdapterStatus.PoweredOn)
            .Select(x => adapter.Scan(config))
            .Switch();


        public static IObservable<IScanResult> ScanInterval(this IAdapter adapter, TimeSpan timeSpan, ScanConfig config = null)
            => Observable.Create<IScanResult>(ob =>
            {
                var scanner = adapter
                    .Scan(config)
                    .Subscribe(ob.OnNext);

                var timer = Observable
                    .Interval(timeSpan)
                    .Subscribe(x =>
                    {
                        if (scanner == null)
                        {
                            scanner = adapter
                                .Scan()
                                .Subscribe(ob.OnNext);
                        }
                        else
                        {
                            scanner.Dispose();
                            scanner = null;
                        }
                    });

                return () =>
                {
                    timer.Dispose();
                    scanner?.Dispose();
                };
            });
    }
}
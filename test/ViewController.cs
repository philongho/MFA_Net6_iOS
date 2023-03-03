using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using UIKit;
using WSA.Foundation.Acoustic.Abstractions;
using WSA.Foundation.Acoustic.Abstractions.Models;
using WSA.Foundation.Audio.Abstractions.Contracts;
using WSA.Foundation.Locator.Autofac;
using WSA.Foundation.Locator.Basic;
using WSA.Foundation.Persistency.Abstractions;
using WSA.Foundation.Shared.Abstractions.Contracts;
using WSA.Foundation.Shared.Abstractions.Models;

namespace test
{
    [Register("ViewController")]
    public partial class ViewController : UIViewController
    {
        private IHearingSystemFactory _hearingSystemFactory;
        private IHearingInstrumentParameterBuilder _hearingInstrumentParameterBuilder;


        public ViewController() : base()
        {
        }

        public ViewController(IntPtr handle) : base(handle)
        {

        }

        public override void ViewDidLoad()
        {
            IHearingSystem hearingSystem = null;
            base.ViewDidLoad();
            ResolveDependencies();

            var pair = new UIButton(new CGRect(0, 200, View.Frame.Width, 60));
            pair.SetTitle("Test Arc Pairing", UIControlState.Normal);
            pair.SetTitleColor(UIColor.Black, UIControlState.Normal);
            pair.TouchUpInside += async (sender, e) =>
            {
                pair.Enabled = false;
                pair.Alpha = (nfloat)0.5;
                hearingSystem = await _hearingSystemFactory.CreateAsync(new HashSet<Side> { Side.Left }, Brand.BasicSiemensSignia, CancellationToken.None);

                var infoExtension = hearingSystem.GetExtension<IDeviceInformationExtension>();
                var addressExtension = hearingSystem.GetExtension<IAcousticAddressExtension>();
                var guid = infoExtension.GetGuid(Side.Left);
                var address = await addressExtension.GetAsync(Side.Left, CancellationToken.None);


                var devices = new Dictionary<Side, AcousticDeviceInformation>();
                devices.Add(Side.Left, new AcousticDeviceInformation(address.Address, guid, Brand.BasicSiemensSignia));

                _hearingInstrumentParameterBuilder.ForDevices(devices.ToDictionary(item => item.Key, item => item.Value.Guid));
                _hearingInstrumentParameterBuilder.WithNumberOfPrograms(6);

                await _hearingInstrumentParameterBuilder.BuildAsync();

                hearingSystem = await _hearingSystemFactory.CreateAsync(devices, CancellationToken.None);
                pair.Enabled = true;
                pair.Alpha = 1;
            };
            pair.BackgroundColor = UIColor.Brown;


            var settings = new UIButton(new CGRect(0, 400, View.Frame.Width, 60));
            settings.SetTitle("Test Arc Settings", UIControlState.Normal);
            settings.SetTitleColor(UIColor.Black, UIControlState.Normal);
            settings.BackgroundColor = UIColor.Brown;


            settings.TouchUpInside += async (sender, e) =>
            {
                settings.Enabled = false;
                settings.Alpha = (nfloat)0.5;
                await hearingSystem.SelectAsync(CancellationToken.None);

                var programExtension = hearingSystem.GetExtension<IProgramExtension>();

                await programExtension.WriteAsync(ProgramId.Program1, CancellationToken.None, ReceiverSide.Left);

                var volumeExtension = hearingSystem.GetExtension<IVolumeExtension>();
                await volumeExtension.WriteAsync(new Volume() { SliderValue = 11 }, CancellationToken.None, ReceiverSide.Left);

                var soundBalanceExtension = hearingSystem.GetExtension<ISoundBalanceExtension>();
                await soundBalanceExtension.WriteAsync(new SoundBalance() { SliderValue = 0 }, CancellationToken.None, ReceiverSide.Left);

                var bands = new EqualizerBands(-9, -6, 3, 6);
                var fourBandEqualizerExtension = hearingSystem.GetExtension<IFourBandEqualizerExtension>();
                await fourBandEqualizerExtension.WriteAsync(bands, CancellationToken.None, ReceiverSide.Left);
                settings.Enabled = true;
                settings.Alpha = 1;
            };

            View.AddSubview(pair);
            View.AddSubview(settings);
        }


        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public void ResolveDependencies()
        {
            _hearingSystemFactory = Locator.Current.Resolve<IHearingSystemFactory>();
            _hearingInstrumentParameterBuilder = Locator.Current.Resolve<IHearingInstrumentParameterBuilder>();
        }
    }

    public class Volume : IVolume
    {
        public int? Percentage { get; set; }
        public int? SliderValue { get; set; }
    }

    public class SoundBalance : ISoundBalance
    {
        public int? Percentage { get; set; }
        public int? SliderValue { get; set; }
    }
}

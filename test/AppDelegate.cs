using DryIoc;
using Foundation;
using UIKit;
using WSA.Foundation.Acoustic.Abstractions.Models;
using WSA.Foundation.Acoustic.iOS;
using WSA.Foundation.Locator.Basic;
using WSA.Foundation.Locator.DryIoc;
using WSA.Foundation.Logger.iOS;
using WSA.Foundation.Shared.Abstractions.Models;
using WSA.Foundation.Logger.AppCenter;
using Autofac;
using WSA.Foundation.Locator.Autofac;

namespace test
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIResponder, IUIApplicationDelegate
    {

        [Export("window")]
        public UIWindow Window { get; set; }

        [Export("application:didFinishLaunchingWithOptions:")]
        public bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method

            var containerBuilder = new ContainerBuilder();
            var container = new AutofacLocatorService(containerBuilder);

            RegisterOnLocatorInitialized(container);
            var locatorBasicServiceCollection = new LocatorBasicServiceCollectionAdapter(container);
            BasicLocator.Initialize(container);
            Logger.Init(locatorBasicServiceCollection, container);
            AppCenter.Init(locatorBasicServiceCollection, container);
            Acoustic.Init(locatorBasicServiceCollection, container);

            container.Build();

            return true;
        }

        protected void RegisterOnLocatorInitialized(IBasicLocator locator)
        {
            locator.RegisterOnLocatorIsInitialized(LocatorInitialized);
        }

        protected void LocatorInitialized(IBasicLocator locator)
        {
            locator.DeregisterFromLocatorIsInitialized(LocatorInitialized);
        }

        // UISceneSession Lifecycle

        [Export("application:configurationForConnectingSceneSession:options:")]
        public UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
        {
            // Called when a new scene session is being created.
            // Use this method to select a configuration to create the new scene with.
            return UISceneConfiguration.Create("Default Configuration", connectingSceneSession.Role);
        }

        [Export("application:didDiscardSceneSessions:")]
        public void DidDiscardSceneSessions(UIApplication application, NSSet<UISceneSession> sceneSessions)
        {
            // Called when the user discards a scene session.
            // If any sessions were discarded while the application was not running, this will be called shortly after `FinishedLaunching`.
            // Use this method to release any resources that were specific to the discarded scenes, as they will not return.
        }
    }
}



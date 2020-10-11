using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Tash;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Components;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.TashHost.Core;
using Autofac;

namespace Aspenlaub.Net.GitHub.CSharp.TashHost {
    /// <summary>
    /// Interaction logic for TashHostWindow.xaml
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public partial class TashHostWindow : IDisposable {
        private ITashAccessor TashAccessor { get; }
        private DispatcherTimer vDispatcherTimer;
        private SynchronizationContext UiSynchronizationContext { get; }
        private DateTime vUiThreadLastActiveAt, vStatusLastConfirmedAt;
        private readonly int vProcessId;

        public TashHostWindow() {
            InitializeComponent();
            var container = new ContainerBuilder().UseTashHost().Build();
            TashAccessor = new TashAccessor(container.Resolve<IDvinRepository>());
            UiSynchronizationContext = SynchronizationContext.Current;
            vProcessId = Process.GetCurrentProcess().Id;
            UpdateUiThreadLastActiveAt();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            TashAccessor.ConfirmDeadWhileClosing(vProcessId);
        }

        public void Dispose() {
            vDispatcherTimer?.Stop();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e) {
            Environment.Exit(0);
        }

        private async Task ConnectAndMakeTashRegistrationAsync() {
            var tashErrorsAndInfos = await TashAccessor.EnsureTashAppIsRunningAsync();
            if (tashErrorsAndInfos.AnyErrors()) {
                MessageBox.Show(string.Join("\r\n", tashErrorsAndInfos.Errors), Properties.Resources.CouldNotConnectToTash, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

            var statusCode = await TashAccessor.PutControllableProcessAsync(Process.GetCurrentProcess());
            if (statusCode != HttpStatusCode.Created) {
                MessageBox.Show(string.Format(Properties.Resources.CouldNotMakeTashRegistration, statusCode.ToString()), Properties.Resources.TashHostWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private async void LoustWindow_OnLoadedAsync(object sender, RoutedEventArgs e) {
            await ConnectAndMakeTashRegistrationAsync();
            CreateAndStartTimer();
        }

        private void CreateAndStartTimer() {
            vDispatcherTimer = new DispatcherTimer();
            vDispatcherTimer.Tick += LoustWindow_TickAsync;
            vDispatcherTimer.Interval = TimeSpan.FromSeconds(7);
            vDispatcherTimer.Start();
        }

        private async void LoustWindow_TickAsync(object sender, EventArgs e) {
            UiSynchronizationContext.Send(x => UpdateUiThreadLastActiveAt(), null);
            if (vStatusLastConfirmedAt == vUiThreadLastActiveAt) { return; }

            var statusCode = await TashAccessor.ConfirmAliveAsync(vProcessId, vUiThreadLastActiveAt, ControllableProcessStatus.Busy);
            if (statusCode == HttpStatusCode.NoContent) {
                vStatusLastConfirmedAt = vUiThreadLastActiveAt;
                UiSynchronizationContext.Post(x => ShowLastCommunicatedTimeStamp(), null);
                return;
            }

            UiSynchronizationContext.Post(x => { CommunicateCouldNotConfirmStatusToTashThenStop(statusCode); }, null);
        }

        private void CommunicateCouldNotConfirmStatusToTashThenStop(HttpStatusCode statusCode) {
            var s = string.Format(Properties.Resources.CouldNotConfirmStatusToTash, statusCode.ToString());
            MonitorBox.Text = MonitorBox.Text + "\r\n" + s;
        }

        private void UpdateUiThreadLastActiveAt() {
            if (Dispatcher?.CheckAccess() != true) {
                MessageBox.Show(Properties.Resources.ConfirmationToTashNotFromUiThread, Properties.Resources.TashHostWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            vUiThreadLastActiveAt = DateTime.Now;
        }

        private void ShowLastCommunicatedTimeStamp() {
            StatusConfirmedAt.Text = vStatusLastConfirmedAt.Year > 2000 ? vStatusLastConfirmedAt.ToLongTimeString() : "";
        }

    }
}

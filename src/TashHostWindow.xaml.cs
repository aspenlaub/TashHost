using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Tash;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Components;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.TashHost.Core;
using Autofac;

namespace Aspenlaub.Net.GitHub.CSharp.TashHost;

/// <summary>
/// Interaction logic for TashHostWindow.xaml
/// </summary>
// ReSharper disable once UnusedMember.Global
public partial class TashHostWindow : IDisposable {
    private ITashAccessor TashAccessor { get; }
    private DispatcherTimer DispatcherTimer;
    private SynchronizationContext UiSynchronizationContext { get; }
    private DateTime UiThreadLastActiveAt, StatusLastConfirmedAt;
    private readonly int ProcessId;

    public TashHostWindow() {
        InitializeComponent();
        var container = new ContainerBuilder().UseTashHost("TashHost").Build();
        TashAccessor = new TashAccessor(container.Resolve<IDvinRepository>(), container.Resolve<ISimpleLogger>(), container.Resolve<ILogConfiguration>());
        UiSynchronizationContext = SynchronizationContext.Current;
        ProcessId = Process.GetCurrentProcess().Id;
        UpdateUiThreadLastActiveAt();
    }

    private async void OnWindowClosingAsync(object sender, System.ComponentModel.CancelEventArgs e) {
        await TashAccessor.ConfirmDeadWhileClosingAsync(ProcessId);
    }

    public void Dispose() {
        DispatcherTimer?.Stop();
    }

    private void OnCloseButtonClickAsync(object sender, RoutedEventArgs e) {
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

    private async void OnLoustWindowLoadedAsync(object sender, RoutedEventArgs e) {
        await ConnectAndMakeTashRegistrationAsync();
        CreateAndStartTimer();
    }

    private void CreateAndStartTimer() {
        DispatcherTimer = new DispatcherTimer();
        DispatcherTimer.Tick += LoustWindow_TickAsync;
        DispatcherTimer.Interval = TimeSpan.FromSeconds(7);
        DispatcherTimer.Start();
    }

    private async void LoustWindow_TickAsync(object sender, EventArgs e) {
        UiSynchronizationContext.Send(x => UpdateUiThreadLastActiveAt(), null);
        if (StatusLastConfirmedAt == UiThreadLastActiveAt) { return; }

        var statusCode = await TashAccessor.ConfirmAliveAsync(ProcessId, UiThreadLastActiveAt, ControllableProcessStatus.Busy);
        if (statusCode == HttpStatusCode.NoContent) {
            StatusLastConfirmedAt = UiThreadLastActiveAt;
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

        UiThreadLastActiveAt = DateTime.Now;
    }

    private void ShowLastCommunicatedTimeStamp() {
        StatusConfirmedAt.Text = StatusLastConfirmedAt.Year > 2000 ? StatusLastConfirmedAt.ToLongTimeString() : "";
    }

}
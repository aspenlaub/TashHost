using Aspenlaub.Net.GitHub.CSharp.Dvin.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Tash;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Components;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.TashHost.Core;
using Autofac;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Aspenlaub.Net.GitHub.CSharp.TashHost;

/// <summary>
/// Interaction logic for TashHostWindow.xaml
/// </summary>
// ReSharper disable once UnusedMember.Global
public partial class TashHostWindow : IDisposable {
    private ITashAccessor TashAccessor { get; }
    private DispatcherTimer _DispatcherTimer;
    private SynchronizationContext UiSynchronizationContext { get; }
    private DateTime _UiThreadLastActiveAt, _StatusLastConfirmedAt;
    private readonly int _ProcessId;

    public TashHostWindow() {
        InitializeComponent();
        var container = new ContainerBuilder().UseTashHost("TashHost").Build();
        TashAccessor = new TashAccessor(container.Resolve<IDvinRepository>(), container.Resolve<ISimpleLogger>(), container.Resolve<ILogConfiguration>(),
            container.Resolve<IMethodNamesFromStackFramesExtractor>());
        UiSynchronizationContext = SynchronizationContext.Current;
        _ProcessId = Process.GetCurrentProcess().Id;
        UpdateUiThreadLastActiveAt();
    }

    private async void OnWindowClosingAsync(object sender, System.ComponentModel.CancelEventArgs e) {
        await TashAccessor.ConfirmDeadWhileClosingAsync(_ProcessId);
    }

    public void Dispose() {
        _DispatcherTimer?.Stop();
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
        _DispatcherTimer = new DispatcherTimer();
        _DispatcherTimer.Tick += TashHostWindow_TickAsync;
        _DispatcherTimer.Interval = TimeSpan.FromSeconds(7);
        _DispatcherTimer.Start();
    }

    private async void TashHostWindow_TickAsync(object sender, EventArgs e) {
        UiSynchronizationContext.Send(_ => UpdateUiThreadLastActiveAt(), null);
        if (_StatusLastConfirmedAt == _UiThreadLastActiveAt) { return; }

        var statusCode = await TashAccessor.ConfirmAliveAsync(_ProcessId, _UiThreadLastActiveAt, ControllableProcessStatus.Busy);
        if (statusCode == HttpStatusCode.NoContent) {
            var processes = await TashAccessor.GetControllableProcessesAsync();
            if (processes.Any(p => p.ProcessId == _ProcessId)) {
                _StatusLastConfirmedAt = _UiThreadLastActiveAt;
                UiSynchronizationContext.Post(_ => ShowLastCommunicatedTimeStamp(), null);
                return;
            }

            UiSynchronizationContext.Post(_ => { TashHostNoLongerAmongTashProcesses(); }, null);
            return;
        }

        UiSynchronizationContext.Post(_ => { CommunicateCouldNotConfirmStatusToTashThenStop(statusCode); }, null);
    }

    private void CommunicateCouldNotConfirmStatusToTashThenStop(HttpStatusCode statusCode) {
        var s = string.Format(Properties.Resources.CouldNotConfirmStatusToTash, statusCode.ToString());
        MonitorBox.Text = MonitorBox.Text + "\r\n" + s;
    }

    private void TashHostNoLongerAmongTashProcesses() {
        var s = Properties.Resources.TashHostNoLongerAmongTashProcesses;
        MonitorBox.Text = MonitorBox.Text + "\r\n" + s;
    }

    private void UpdateUiThreadLastActiveAt() {
        if (Dispatcher?.CheckAccess() != true) {
            MessageBox.Show(Properties.Resources.ConfirmationToTashNotFromUiThread, Properties.Resources.TashHostWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
            return;
        }

        _UiThreadLastActiveAt = DateTime.Now;
    }

    private void ShowLastCommunicatedTimeStamp() {
        StatusConfirmedAt.Text = _StatusLastConfirmedAt.Year > 2000 ? _StatusLastConfirmedAt.ToLongTimeString() : "";
    }

}
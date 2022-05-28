using Aspenlaub.Net.GitHub.CSharp.Dvin.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Autofac;

namespace Aspenlaub.Net.GitHub.CSharp.TashHost.Core;

public static class TashHostContainerBuilder {
    public static ContainerBuilder UseTashHost(this ContainerBuilder builder, string applicationName) {
        builder.UseDvinAndPegh(applicationName, new DummyCsArgumentPrompter());
        return builder;
    }
}
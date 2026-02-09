using Aspenlaub.Net.GitHub.CSharp.Dvin.Components;
using Autofac;

namespace Aspenlaub.Net.GitHub.CSharp.TashHost.Core;

public static class TashHostContainerBuilder {
    public static ContainerBuilder UseTashHost(this ContainerBuilder builder, string applicationName) {
        builder.UseDvinAndPegh(applicationName);
        return builder;
    }
}
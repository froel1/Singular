using System;
using System.Collections.Generic;
using System.Text;

namespace Singular.CorePlatform.Game.EGT.Core.Interfaces
{
    public interface IIntegrationInstance
    {
        string Name { get; }

        string IntegrationLabel { get; }

        string AdapterConfigurationPath { get; }
    }
}

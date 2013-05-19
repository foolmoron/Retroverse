using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Retroverse
{
    public interface IReversible
    {
        IMemento GenerateMementoFromCurrentFrame();
    }
}

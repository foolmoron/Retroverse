using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Retroverse
{
    public interface IMemento
    {
        Object Target { get; set; }
        void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame);
    }
}

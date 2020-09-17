using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcEngineTutorialSetup
{
    interface IFixture
    {
        void Setup();

        void Cleanup();
    }
}

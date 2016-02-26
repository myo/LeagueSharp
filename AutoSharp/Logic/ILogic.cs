using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSharp.Logic
{
    public interface ILogic
    {
        bool ShouldTakeAction();
        void TakeAction(EventArgs args);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExerciseTracker.Core
{
    public interface IParameterReceiver<TParameter>
    {
        void ReceiveParameter(TParameter parameter);
    }
}

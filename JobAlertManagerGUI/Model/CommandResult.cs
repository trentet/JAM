using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobAlertManagerGUI.Model
{
    public class CommandResult<T>
    {
        private T resultObject;

        public T ResultObject { get => resultObject; set => resultObject = value; }

        public CommandResult(object resultObject)
        {
            ResultObject = (T)resultObject;
        }
    }
}

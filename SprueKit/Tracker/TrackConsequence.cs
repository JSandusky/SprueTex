using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notify
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple =true)]
    public class TrackConsequenceAttribute : System.Attribute
    {
        public TrackConsequenceAttribute(string method)
        {
            Method = method;
        }

        public string Method { get; set; }

        public void Trigger(object onWho)
        {
            var method = onWho.GetType().GetMethod(Method);
            if (method != null)
                method.Invoke(onWho, null);
        }
    }
}

using Serilog;

namespace MCOP.Core.Exceptions
{
    public class McopException : Exception
    {
        public McopException()
            : base("Unknown exception") { }

        public McopException(string msg) : base(msg)
        {
            Log.Error(msg);
        }

        public McopException(Exception inner, string msg) : base(msg, inner)
        {

            Log.Error(inner, inner.TargetSite?.ReflectedType?.FullName ?? "");
        }
    }

}

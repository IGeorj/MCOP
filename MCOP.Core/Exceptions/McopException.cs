using Serilog;

namespace MCOP.Core.Exceptions
{
    public class McopException : Exception
    {
        public string UserMessage { get; set; }

        public McopException()
            : base("Unknown exception") { }

        public McopException(string msg) : base(msg)
        {
            UserMessage = msg;
            Log.Error(msg);
        }

        public McopException(Exception inner, string msg) : base(msg, inner)
        {
            UserMessage = msg;
            Log.Error(inner, inner.TargetSite?.ReflectedType?.FullName ?? "");
        }
    }

}

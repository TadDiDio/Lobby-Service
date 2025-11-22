using System;
using System.Threading.Tasks;

namespace LobbyService.Procedure
{
    public enum InvokePermission
    {
        Owner,
        All
    }

    public class LobbyProcedure
    {
        /// <summary>
        /// Who is allowed to invoke this.
        /// </summary>
        public InvokePermission InvokePermission;

        /// <summary>
        /// Should we invoked this locally when executing?
        /// </summary>
        public bool ExecuteLocally;

        /// <summary>
        /// A unique key used to invoke this procedure.
        /// </summary>
        public string Key;

        /// <summary>
        /// The procedure to run.
        /// </summary>
        public Func<string[], Task> Procedure;
    }
}

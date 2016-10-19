using System;

namespace Core.Interfaces
{
    public interface ITransactionUnit:IDisposable
    {
        /// <summary>
        /// Return ID of this operation
        /// </summary>
        /// <returns>Unique ID</returns>
        string GetOperationId();

        /// <summary>
        /// Set <paramref name="operationId"/> of this operation
        /// </summary>
        /// <param name="operationId"></param>
        void SetOperationId(string operationId);

        /// <summary>
        /// Rollback operation with <paramref name="operationId"/>
        /// </summary>
        /// <param name="operationId"></param>
        void Rollback(string operationId);

        /// <summary>
        /// Rollback this operation
        /// </summary>
        void Rollback();

        /// <summary>
        /// Execute this operation
        /// </summary>
        void Commit();
    }
}

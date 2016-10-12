namespace Core.Tests.Fakes
{
    using Core.Interfaces;

    class FakeTransactionUnit : ITransactionUnit
    {
        public void Dispose()
        {
        }

        public string GetOperationId()
        {
            return string.Empty;
        }

        public void Rollback(string operationID)
        {
        }

        public void Rollback()
        {
        }

        public void Commit()
        {
        }
    }
}

namespace Genesis.DataStore.UnitTests.Genesis.Logging.Mocks
{
    using global::Genesis.Logging;
    using PCLMock;

    public sealed class LoggerMock : MockBase<ILogger>, ILogger
    {
        public LoggerMock(MockBehavior behavior)
            : base(behavior)
        {
            if (behavior == MockBehavior.Loose)
            {
                this.ConfigureLooseBehavior();
            }
        }

        public string Name => this.Apply(x => x.Name);

        public bool IsDebugEnabled => this.Apply(x => x.IsDebugEnabled);

        public bool IsErrorEnabled => this.Apply(x => x.IsErrorEnabled);

        public bool IsInfoEnabled => this.Apply(x => x.IsInfoEnabled);

        public bool IsPerfEnabled => this.Apply(x => x.IsPerfEnabled);

        public bool IsWarnEnabled => this.Apply(x => x.IsWarnEnabled);

        public void Log(LogLevel level, string message) =>
            this.Apply(x => x.Log(level, message));

        private void ConfigureLooseBehavior()
        {
            this
                .When(x => x.Name)
                .Return("Mock");
            this
                .When(x => x.IsDebugEnabled)
                .Return(false);
            this
                .When(x => x.IsErrorEnabled)
                .Return(false);
            this
                .When(x => x.IsInfoEnabled)
                .Return(false);
            this
                .When(x => x.IsPerfEnabled)
                .Return(false);
            this
                .When(x => x.IsWarnEnabled)
                .Return(false);
        }
    }
}
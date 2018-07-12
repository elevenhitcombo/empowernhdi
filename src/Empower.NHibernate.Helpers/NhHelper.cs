using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate;


namespace Empower.NHibernate.Setup
{
    public class NhHelper
    {
        private static ISessionFactory _sessionFactory;
        private ITransaction _transaction;
       
        public ISession Session { get; private set; }

        public NhHelper(string connectionString, string sessionContext = "web")
        {
            InitialiseSessionFactory(connectionString, sessionContext);

            Session = _sessionFactory.OpenSession();
        }

        public void BeginTransaction()
        {
            _transaction = Session.BeginTransaction();
        }

        public void Commit()
        {
            try
            {
                // commit transaction if there is one active
                if (_transaction != null && _transaction.IsActive)
                    _transaction.Commit();
            }
            catch
            {
                // rollback if there was an exception
                if (_transaction != null && _transaction.IsActive)
                    _transaction.Rollback();

                throw;
            }
            finally
            {
                Session.Dispose();
            }
        }

        public void Rollback()
        {
            try
            {
                if (_transaction != null && _transaction.IsActive)
                    _transaction.Rollback();
            }
            finally
            {
                Session.Dispose();
            }
        }

        private void InitialiseSessionFactory(string connectionString, string sessionContext)
        {
            // We only want one copy of this
            if (_sessionFactory == null)
            {
                var firstConfig =
                Fluently.Configure()
                .Database
                (
                    MsSqlConfiguration.MsSql2008
                        .UseOuterJoin()
                        .ShowSql()
                        .FormatSql()
                        .ConnectionString(c => c.Is(connectionString)))
                        .Mappings(m =>
                        {
                            m.FluentMappings
                                .Conventions
                                .Setup(x =>
                                {
                                    x.Add(AutoImport.Never());
                                })
                                //.AddFromAssemblyOf<FilmMap>();
                                ;

                        }
                );

                _sessionFactory =
                    firstConfig
                       .ExposeConfiguration(
                           c => c.SetProperty(
                               "current_session_context_class",
                               sessionContext))
                   .BuildSessionFactory();
            }
        }
    }
}

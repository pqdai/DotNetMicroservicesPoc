using System.Data;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Bytecode;
using NHibernate.Cfg;
using NHibernate.Connection;
using NHibernate.Dialect;
using NHibernate.Driver;
using PolicyService.Domain;
using ISession = NHibernate.ISession;

namespace PolicyService.DataAccess.NHibernate;

public static class NHibernateInstaller
{
    public static IServiceCollection AddNHibernate(this IServiceCollection services, string cnString)
    {
        var cfg = new Configuration();

        cfg.DataBaseIntegration(db =>
        {
            db.Dialect<PostgreSQL83Dialect>();
            db.Driver<NpgsqlDriver>();
            db.ConnectionProvider<DriverConnectionProvider>();
            db.BatchSize = 500;
            db.IsolationLevel = System.Data.IsolationLevel.ReadCommitted;
            db.LogSqlInConsole = false;
            db.ConnectionString = cnString;
            db.Timeout = 30;
            db.SchemaAction = SchemaAutoAction.Update;
        });

        cfg.Proxy(p => p.ProxyFactoryFactory<StaticProxyFactoryFactory>());
        cfg.Cache(c => c.UseQueryCache = false);
        cfg.AddAssembly(typeof(NHibernateInstaller).Assembly);

        var sessionFactory = cfg.BuildSessionFactory();
        EnsureOutboxSchema(sessionFactory);

        services.AddSingleton(sessionFactory);
        services.AddScoped(s => s.GetRequiredService<ISessionFactory>().OpenSession());
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static void EnsureOutboxSchema(ISessionFactory sessionFactory)
    {
        using var session = sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        ExecuteNonQuery(
            session,
            """
            CREATE TABLE IF NOT EXISTS outbox_messages
            (
                id BIGSERIAL PRIMARY KEY,
                type VARCHAR(500) NOT NULL,
                json_payload TEXT NOT NULL DEFAULT ''
            )
            """
        );

        ExecuteNonQuery(
            session,
            """
            DO $$
            BEGIN
                IF EXISTS
                (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_schema = 'public'
                      AND table_name = 'outbox_messages'
                      AND column_name = 'payload'
                )
                AND NOT EXISTS
                (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_schema = 'public'
                      AND table_name = 'outbox_messages'
                      AND column_name = 'json_payload'
                ) THEN
                    ALTER TABLE public.outbox_messages RENAME COLUMN payload TO json_payload;
                END IF;
            END $$;
            """
        );

        ExecuteNonQuery(
            session,
            """
            ALTER TABLE outbox_messages
                ADD COLUMN IF NOT EXISTS json_payload TEXT NOT NULL DEFAULT ''
            """
        );

        ExecuteNonQuery(
            session,
            """
            ALTER TABLE outbox_messages
                ALTER COLUMN type TYPE VARCHAR(500)
            """
        );

        tx.Commit();
    }

    private static void ExecuteNonQuery(ISession session, string sql)
    {
        using var command = session.Connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.ExecuteNonQuery();
    }
}

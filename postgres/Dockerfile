FROM postgres:11.4
COPY createdatabases.postgres.psql /docker-entrypoint-initdb.d/createdatabases.postgres.psql
COPY init-db.sh /docker-entrypoint-initdb.d/init-db.sh

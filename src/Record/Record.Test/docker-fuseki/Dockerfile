FROM eclipse-temurin:21 AS runtime
ADD https://dlcdn.apache.org/jena/binaries/apache-jena-fuseki-6.0.0.tar.gz apache-jena-fuseki.tar.gz
RUN mkdir -p /opt/jena && \
    tar -xzf apache-jena-fuseki.tar.gz -C /opt/jena && \
    rm apache-jena-fuseki.tar.gz && \
    mv /opt/jena/apache-jena-fuseki-* /opt/jena/jena-fuseki2

COPY config/in_memory.ttl /opt/jena/config.ttl

WORKDIR /opt/jena/jena-fuseki2
EXPOSE 3030

ENTRYPOINT ["./fuseki-server", "--config", "/opt/jena/config.ttl"]
FROM eclipse-temurin:21 AS runtime
ADD https://archive.apache.org/dist/jena/binaries/apache-jena-fuseki-6.3.0.tar.gz apache-jena-fuseki.tar.gz
RUN mkdir -p /opt/jena && \
    tar -xzf apache-jena-fuseki.tar.gz -C /opt/jena && \
    rm apache-jena-fuseki.tar.gz && \
    mv /opt/jena/apache-jena-fuseki-* /opt/jena/jena-fuseki2

COPY config/in_memory.ttl /opt/jena/config.ttl
COPY config/shiro.ini /opt/jena/jena-fuseki2/run/shiro.ini
COPY config/config-mem /opt/jena/jena-fuseki2/run/templates/config-mem

WORKDIR /opt/jena/jena-fuseki2
ENV JVM_ARGS="-Xmx4G"
EXPOSE 3030

ENTRYPOINT ["./fuseki-server", "--config", "/opt/jena/config.ttl"]

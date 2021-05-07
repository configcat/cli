FROM alpine:3.8

RUN apk add --no-cache \
        ca-certificates \
        krb5-libs \
        libintl \
        libssl1.0 \
        libstdc++ \
        zlib

COPY ./linux-musl-x64/configcat /usr/local/bin

ENTRYPOINT ["configcat"]
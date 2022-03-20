FROM alpine:3.15

RUN apk add --no-cache \
        git \
        openssh-client \
        ca-certificates \
        krb5-libs \
        libintl \
        libssl1.1 \
        libstdc++ \
        zlib

COPY ./linux-musl-x64/configcat /usr/local/bin

ENTRYPOINT ["configcat"]
FROM alpine:3.19

RUN apk add --no-cache \
        git \
        ca-certificates \
        libgcc \
        libssl3 \
        libstdc++ \
        zlib

COPY ./linux-musl-x64/configcat /usr/local/bin

ENTRYPOINT ["configcat"]

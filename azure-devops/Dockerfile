FROM node:18-alpine

COPY ./linux-musl-x64/configcat /usr/local/bin

RUN apk add --no-cache \
        git ca-certificates \
        libgcc libssl3 libstdc++ zlib \
        bash sudo shadow

RUN apk add --no-cache --virtual .pipeline-deps readline linux-pam \
  && apk del .pipeline-deps

LABEL "com.azure.dev.pipelines.agent.handler.node.path"="/usr/local/bin/node"

CMD [ "node" ]

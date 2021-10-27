FROM node:10-alpine

COPY ./linux-musl-x64/configcat /usr/local/bin

RUN apk add --no-cache \
        git openssh-client ca-certificates \
        krb5-libs libintl libssl1.1 libstdc++ zlib \
        bash sudo shadow

RUN apk add --no-cache --virtual .pipeline-deps readline linux-pam \
  && apk del .pipeline-deps

LABEL "com.azure.dev.pipelines.agent.handler.node.path"="/usr/local/bin/node"

CMD [ "node" ]

#!/bin/bash

# Install the ConfigCat CLI.
# https://github.com/configcat/cli
#
# Usage: curl -fsSL https://raw.githubusercontent.com/configcat/cli/main/scripts/install.sh | bash -s -- -d=<INSTALL-DIR> -v=<VERSION> -a=<ARCHITECTURE>

set -e

for i in "$@"
do
case $i in
    -d=*|--dir=*)
    DIR="${i#*=}"
    ;;
    -v=*|--version=*)
    VERSION="${i#*=}"
    ;;
    -a=*|--arch=*)
    ARCH="${i#*=}"
    ;;
    *)
        echo "==> ERROR: Unknown option ${i}."
        exit 1	
    ;;
esac
done

if [ -z "$VERSION" ]; then
	VERSION=$(curl -s "https://api.github.com/repos/configcat/cli/releases/latest" | grep -Po '"tag_name": "v\K.*?(?=")')
fi

DIR="${DIR:-/usr/local/bin}"

echo "==> Installing ConfigCat CLI v${VERSION}."

UCPATH=$(mktemp -d "${TMPDIR:-/tmp}/configcat.XXXXXXXXX")

case "$(uname -s)" in
    Linux)
        OS='linux'
        ARCH="${ARCH:-x64}"
    ;;
    Darwin)
        OS='osx'
        ARCH='x64'
    ;;
    *)
        echo '==> ERROR: Not supported operating system.'
        exit 1	
    ;;
esac

FILE_NAME="configcat-cli_${VERSION}_${OS}-${ARCH}.tar.gz"
DOWNLOAD_URL="https://github.com/configcat/cli/releases/download/v${VERSION}/${FILE_NAME}"

echo "==> Downloading '${DOWNLOAD_URL}'."
(cd "$UCPATH" && curl -sL --retry 3 "$DOWNLOAD_URL" -o "$FILE_NAME")

echo "==> Extracting '${FILE_NAME}' into '${UCPATH}'."
(cd "$UCPATH" && tar -xzf ${FILE_NAME})

echo "==> Moving binary to '${DIR}'."
cp "$UCPATH/configcat" "${DIR}"

echo "==> Deleting '${UCPATH}'."
rm -rf "$UCPATH"

echo "==> ConfigCat CLI v${VERSION} successfully installed. Happy Feature Flagging!"
configcat cat

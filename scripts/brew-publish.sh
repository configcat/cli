#!/bin/bash

set -e

CLONE_DIR=$(mktemp -d)

git config --global user.email "$GH_USER_EMAIL"
git config --global user.name "$GH_USER_NAME"

git clone --single-branch --branch main "https://$GH_USER_NAME:$GH_API_TOKEN@github.com/configcat/homebrew-tap.git" "$CLONE_DIR"

mkdir -p "$CLONE_DIR/Formula"
cp brew/fromula-template.rb "$CLONE_DIR/Formula/configcat.rb"

OSX_TAR="configcat-cli_${VERSION}_osx-x64.tar.gz"
LINUX_TAR="configcat-cli_${VERSION}_linux-x64.tar.gz"

OSX_CHECKSUM=$(sha256sum $OSX_TAR | awk '{print $1}')
LINUX_CHECKSUM=$(sha256sum $LINUX_TAR | awk '{print $1}')

sed -i "s/#VERSION_PLACEHOLDER#/$VERSION/g" "$CLONE_DIR/Formula/configcat.rb"
sed -i "s+#OSX-TAR-PATH#+https://github.com/configcat/cli/releases/download/v$VERSION/$OSX_TAR+g" "$CLONE_DIR/Formula/configcat.rb"
sed -i "s+#LINUX-TAR-PATH#+https://github.com/configcat/cli/releases/download/v$VERSION/$LINUX_TAR+g" "$CLONE_DIR/Formula/configcat.rb"
sed -i "s/#OSX-TAR-SUM#/$OSX_CHECKSUM/g" "$CLONE_DIR/Formula/configcat.rb"
sed -i "s/#LINUX-TAR-SUM#/$LINUX_CHECKSUM/g" "$CLONE_DIR/Formula/configcat.rb"

cd "$CLONE_DIR"
git add .
git status

git diff-index --quiet HEAD || git commit -m "Updating ConfigCat CLI Formula due to new release v$VERSION"

cat "$CLONE_DIR/Formula/configcat.rb"

#git push "https://$GH_USER_NAME:$GH_API_TOKEN@github.com/configcat/homebrew-tap.git"
#!/bin/bash

set -e

CLONE_DIR=$(mktemp -d)

git config --global user.email "$GH_USER_EMAIL"
git config --global user.name "$GH_USER_NAME"

git clone --single-branch --branch main "https://$GH_USER_NAME:$GH_API_TOKEN@github.com/configcat/scoop-configcat.git" "$CLONE_DIR"

mkdir -p "$CLONE_DIR/bucket"
cp scoop/configcat-template.json "$CLONE_DIR/bucket/configcat.json"

WIN64_PATH="configcat-cli_${VERSION}_win-x64.zip"
WIN32_PATH="configcat-cli_${VERSION}_win-x86.zip"

WIN64_SUM=$(sha256sum $WIN64_PATH | awk '{print $1}')
WIN32_SUM=$(sha256sum $WIN32_PATH | awk '{print $1}')

sed -i "s/#VERSION_PLACEHOLDER#/$VERSION/g" "$CLONE_DIR/bucket/configcat.json"
sed -i "s+#WIN-64-PATH#+https://github.com/configcat/cli/releases/download/v$VERSION/$WIN64_PATH+g" "$CLONE_DIR/bucket/configcat.json"
sed -i "s+#WIN-32-PATH#+https://github.com/configcat/cli/releases/download/v$VERSION/$WIN32_PATH+g" "$CLONE_DIR/bucket/configcat.json"
sed -i "s/#WIN-64-SUM#/$WIN64_SUM/g" "$CLONE_DIR/bucket/configcat.json"
sed -i "s/#WIN-32-SUM#/$WIN32_SUM/g" "$CLONE_DIR/bucket/configcat.json"

cd "$CLONE_DIR"
git add .
git status

git diff-index --quiet HEAD || git commit -m "Updating ConfigCat CLI Scoop manifest due to new release v$VERSION"

cat "$CLONE_DIR/bucket/configcat.json"

git push "https://$GH_USER_NAME:$GH_API_TOKEN@github.com/configcat/scoop-configcat.git"
#!/bin/bash
set -e

echo "=== Preparing Debian package structure ==="
mkdir -p "$DEB_ROOT/usr/local/bin"
mkdir -p "$DEB_ROOT/DEBIAN"

echo "=== Copying application files ==="
cp -r "${PUBLISH_DIR}/"* "$DEB_ROOT/usr/local/bin/"
chmod +x "$DEB_ROOT/usr/local/bin/$APP_NAME" || true

echo "=== Creating control file ==="
cat > "$DEB_ROOT/DEBIAN/control" <<EOF
Package: ${APP_NAME}
Version: ${VERSION}
Section: utils
Priority: optional
Architecture: ${DEB_ARCH}
Maintainer: $(whoami)
Description: ${APP_NAME} .NET application
 Automatically generated .deb package.
EOF

echo "=== Building .deb package ==="
dpkg-deb --build "$DEB_ROOT"

FINAL_DEB="${DEB_ROOT}.deb"
mv "${DEB_ROOT}.deb" "${APP_NAME}.deb"

echo "=== DONE ==="
echo "Created package: ${APP_NAME}.deb"

#!/usr/bin/env bash
# Builds the Linux release artifacts into dist/:
#   simplemarkdown-v<N>-linux-x64.tar.gz   generic self-contained build
#   simplemarkdown-<N>-1.x86_64.rpm        Fedora package
# The version <N> is read from <Version> in src/app/SimpleMarkdown.csproj.
set -euo pipefail

root="$(cd "$(dirname "$0")/.." && pwd)"
csproj="$root/src/app/SimpleMarkdown.csproj"
dist="$root/dist"

version="$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$csproj" | head -1)"
if ! [[ "$version" =~ ^[0-9]+$ ]]; then
  echo "error: <Version> in $csproj must be a whole number, found '$version'" >&2
  exit 1
fi

rm -rf "$dist"
publish="$dist/publish"
dotnet publish "$csproj" -c Release -r linux-x64 --self-contained true -o "$publish"

tar -czf "$dist/simplemarkdown-v$version-linux-x64.tar.gz" -C "$publish" .

rpmbuild -bb "$root/packaging/simplemarkdown.spec" \
  --define "_topdir $dist/rpmbuild" \
  --define "app_version $version" \
  --define "publish_dir $publish" \
  --define "pkg_dir $root/packaging" \
  --define "license_file $root/LICENSE"
cp "$dist"/rpmbuild/RPMS/x86_64/*.rpm "$dist/"

echo "version=$version"
ls -lh "$dist"/*.tar.gz "$dist"/*.rpm

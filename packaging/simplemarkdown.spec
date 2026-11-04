# Packages a self-contained `dotnet publish` output.
# Required defines: app_version, publish_dir, pkg_dir, license_file

%global debug_package %{nil}
%global __os_install_post %{nil}
%global __strip /bin/true
%global source_date_epoch_from_changelog 0

Name:           simplemarkdown
Version:        %{app_version}
Release:        1
Summary:        Minimal dark-themed markdown viewer
License:        MIT
URL:            https://github.com/larsfagerbakke/SimpleMarkdown
ExclusiveArch:  x86_64
AutoReqProv:    no
Requires:       libicu
Requires:       fontconfig
Requires:       libX11
Requires:       libwayland-client

%description
SimpleMarkdown is a minimal, dark-themed markdown viewer built with .NET and
Avalonia. It renders GitHub-flavored markdown, reloads when the file changes
and can switch to the raw markdown syntax.

%install
install -d %{buildroot}%{_prefix}/lib/simplemarkdown
cp -a %{publish_dir}/. %{buildroot}%{_prefix}/lib/simplemarkdown/
install -d %{buildroot}%{_bindir}
ln -s ../lib/simplemarkdown/SimpleMarkdown %{buildroot}%{_bindir}/simplemarkdown
install -Dm644 %{pkg_dir}/SimpleMarkdown.desktop %{buildroot}%{_datadir}/applications/SimpleMarkdown.desktop
install -Dm644 %{pkg_dir}/simplemarkdown.png %{buildroot}%{_datadir}/icons/hicolor/256x256/apps/simplemarkdown.png
install -Dm644 %{license_file} %{buildroot}%{_licensedir}/simplemarkdown/LICENSE

%files
%{_prefix}/lib/simplemarkdown
%{_bindir}/simplemarkdown
%{_datadir}/applications/SimpleMarkdown.desktop
%{_datadir}/icons/hicolor/256x256/apps/simplemarkdown.png
%license %{_licensedir}/simplemarkdown/LICENSE

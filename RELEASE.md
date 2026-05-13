# Release Output

Build the desktop release with:

```powershell
dotnet build MyShop/MyShop.csproj -f net10.0-desktop -c Release
```

The executable output is kept under:

```text
Release/MyShop-win-x64/MyShop.exe
```

The installer is generated at:

```text
Release/MyShopSetup.exe
```

Rebuild the installer with Inno Setup:

```powershell
& "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe" installer\MyShopSetup.iss
```

The installer script excludes `.env`, `.env copy`, `.env.*`, and `*.pdb` so real API keys are not packaged.

After installing with `MyShopSetup.exe`, put runtime `.env` configuration here:

```text
%APPDATA%\MyShop\.env
```

For example:

```powershell
notepad "$env:APPDATA\MyShop\.env"
```
